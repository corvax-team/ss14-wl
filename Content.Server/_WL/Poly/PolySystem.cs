using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Shared._WL.CCVars;
using Content.Shared._WL.Poly.Events;
using Content.Shared.Chat;
using Content.Shared.GameTicking;
using Content.Shared.Random.Helpers;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.IO;
using System.Linq;

namespace Content.Server._WL.Poly
{
    public sealed partial class PolySystem : EntitySystem
    {
        [Dependency] private readonly IChatManager _chatManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly GameTicker _ticker = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly IConfigurationManager _configMan = default!;
        [Dependency] private readonly IPlayerManager _playMan = default!;
        [Dependency] private readonly ILogManager _logMan = default!;

        private ISawmill _sawmill = default!;

        private List<MessageEntry> _messages = default!;

        private TimeSpan _time = TimeSpan.Zero;
        private bool _readyToPick = false;
        private TimeSpan _chooseInterval = default!;
        private bool _neededCleanup = false;

        private Dictionary<string, ChatMessage> _queriedEntities = default!;
        private Dictionary<string, byte[]?> _handledImages = default!;

        private const int MAX_QUERIES_PER_PLAYER = 20;

        public override void Initialize()
        {
            base.Initialize();

            _messages = new();
            _queriedEntities = new();
            _handledImages = new();

            _sawmill = _logMan.GetSawmill("poly.server");

            _readyToPick = _configMan.GetCVar(WLCVars.PolyNeededRoundEndCleanup);
            _chooseInterval = TimeSpan.FromSeconds(_configMan.GetCVar(WLCVars.PolyMessageChooseCooldown));

            Subs.CVar(_configMan, WLCVars.PolyMessageChooseCooldown, (new_value) => _chooseInterval = TimeSpan.FromSeconds(new_value), true);
            Subs.CVar(_configMan, WLCVars.PolyNeededRoundEndCleanup, (needed) => _neededCleanup = needed);

            SubscribeLocalEvent<RoundRestartCleanupEvent>((_) =>
            {
                if (!_neededCleanup)
                    return;

                Clean();
            });

            SubscribeNetworkEvent<PolyClientResponseEvent>(OnClientPolyResponse);

            _chatManager.OnAfterChatMessage += (msg) =>
            {
                if (!_readyToPick)
                    return;

                var sender = msg.SenderEntity;

                DebugTools.AssertNotNull(sender);

                if (!sender.Valid)
                    return;

                if (!ShouldMessageBeChosen(msg))
                    return;

                QueryAddMessage(msg);

                ResetTimer();
                _readyToPick = false;
            };

            _playMan.PlayerStatusChanged += (sender, args) =>
            {
                if (_playMan.Sessions.Length - 1 != 0)
                    return;

                if (args.NewStatus is not SessionStatus.Connected)
                    return;

                HandleQueries();
            };
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            if (!_timing.IsFirstTimePredicted)
                return;

            _time += _timing.TickPeriod;

            if (_time >= _chooseInterval)
            {
                ResetTimer();
                _readyToPick = true;
            }
        }

        private void QueryAddMessage(ChatMessage msg)
        {
            var id = Guid.NewGuid().ToString();

            _queriedEntities.Add(id, msg);

            HandleQueries();
        }

        private void OnClientPolyResponse(PolyClientResponseEvent args, EntitySessionEventArgs sessionArgs)
        {
            var queried = args.QueryId;

            if (!_queriedEntities.TryGetValue(queried, out var msg))
                return;

            _queriedEntities.Remove(queried);

            var entry = MessageToEntry(msg, queried);

            _messages.Add(entry);
            _handledImages.Add(queried, args.Stream);
        }

        private void HandleQueries()
        {
            var sessions = _playMan.Sessions
                .Where(s => s.Status == SessionStatus.InGame)
                .ToDictionary(k => k, v => 0);

            if (sessions.Count == 0)
                return;

            var session_pair = PickSession();

            if (session_pair == null)
                return;

            var session = session_pair.Value.Key;
            var queries = session_pair.Value.Value;

            foreach (var item in _queriedEntities)
            {
                if (queries >= MAX_QUERIES_PER_PLAYER)
                {
                    session_pair = PickSession();

                    if (session_pair == null)
                        break;

                    session = session_pair.Value.Key;
                    queries = session_pair.Value.Value;
                }

                var sender = item.Value.SenderEntity;
                var id = item.Key;

                var ev = new PolyServerQueryEvent(sender, id);
                RaiseNetworkEvent(ev, session);

                sessions[session] = ++queries;
            }

            KeyValuePair<ICommonSession, int>? PickSession()
            {
                if (sessions.Count == 0)
                    return null;

                var pair = _random.Pick(sessions);

                if (pair.Value >= MAX_QUERIES_PER_PLAYER)
                {
                    sessions.Remove(pair.Key);
                    return PickSession();
                }

                return pair;
            }
        }

        #region Public
        public void ResetTimer()
        {
            _time = TimeSpan.Zero;
        }

        /// <summary>
        /// Возвращаемый объект Stream должен быть явно очищен. Dispose()
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Stream - если изображение готово. Null - если нет.</returns>
        public Stream? PickImage(string id)
        {
            if (!_handledImages.TryGetValue(id, out var bytes) || bytes == null)
            {
                _handledImages.Remove(id);
                return null;
            }

            _handledImages.Remove(id);

            return new MemoryStream(bytes, false);
        }

        public void Clean()
        {
            _messages.Clear();
            _queriedEntities.Clear();
        }

        public bool IsReadyToPick()
        {
            return _readyToPick;
        }

        public MessageEntry MessageToEntry(ChatMessage msg, string id)
        {
            var sender_ent = GetEntity(msg.SenderEntity);

            return new()
            {
                ColorHex = (msg.MessageColorOverride ?? msg.Channel.TextColor()).ToHex(),
                Content = msg.Message,
                SenderEntityName = Name(sender_ent),
                RoundId = _ticker.RoundId,
                IsRoundFlow = _ticker.RunLevel == GameRunLevel.InRound,
                ID = id,
                Type = (ushort)msg.Channel
            };
        }

        public MessageEntry? Pick()
        {
            if (_messages.Count == 0)
                return null;

            return _random.PickAndTake(_messages);
        }

        public static bool ShouldMessageBeChosen(ChatMessage msg)
        {
            if (msg.HideChat)
                return false;

            if (string.IsNullOrEmpty(msg.Message))
                return false;

            var unnecessary_flags =
                ChatChannel.None |
                ChatChannel.Damage |
                ChatChannel.Visual |
                ChatChannel.Notifications |
                //ChatChannel.AdminRelated | //я всегда буду злодеем >:3
                ChatChannel.Unspecified;

            if (unnecessary_flags.HasFlag(msg.Channel))
                return false;

            return true;
        }

        public TimeSpan HowLongBeforeReady()
        {
            return _chooseInterval - _time;
        }
        #endregion
    }

    public readonly record struct MessageEntry(
        string Content,
        string ColorHex,
        string SenderEntityName,
        int RoundId,
        bool IsRoundFlow,
        string ID,
        ushort Type);
}
