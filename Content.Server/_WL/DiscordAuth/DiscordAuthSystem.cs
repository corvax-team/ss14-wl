using System.Text;
using System.Diagnostics.CodeAnalysis;
using Content.Shared._WL.CCVars;
using Content.Shared._WL.DiscordAuth;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Player;

namespace Content.Server._WL.DiscordAuth
{
    public sealed partial class DiscordAuthSystem : EntitySystem
    {
        [Dependency] private ISharedPlayerManager _playMan = default!;
        [Dependency] private IConfigurationManager _confMan = default!;
        [Dependency] private IRobustRandom _random = default!;
        [Dependency] private IGameTiming _timing = default!;

        private Dictionary<NetUserId, string> _playersTokensKeys = default!;

        private const string AllowedCodeSymbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_.,@#!?1234567890";

        private TimeSpan _expirationTime = TimeSpan.FromSeconds(WLCVars.DiscordAuthTokensExpirationTime.DefaultValue);

        private TimeSpan _expirationAccum = TimeSpan.Zero;

        public override void Initialize()
        {
            base.Initialize();

            _expirationTime = TimeSpan.FromSeconds(_confMan.GetCVar(WLCVars.DiscordAuthTokensExpirationTime));
            _confMan.OnValueChanged(WLCVars.DiscordAuthTokensExpirationTime, (value) => _expirationTime = TimeSpan.FromSeconds(value), true);

            SubscribeNetworkEvent<DiscordAuthTokenQueryEvent>((args, session) =>
            {
                var user = session.SenderSession.UserId;
                if (!_playersTokensKeys.TryGetValue(user, out var code))
                    code = EnsureUser(user);

                RaiseNetworkEvent(new DiscordAuthTokenChangedEvent(code, user), session.SenderSession);
            });

            SubscribeNetworkEvent<DiscordAuthExpirationTimeSyncEvent>((args, session) =>
            {
                RaiseNetworkEvent(new DiscordAuthExpirationTimeSyncEvent(_expirationAccum, _expirationTime), session.SenderSession);
            });

            _playersTokensKeys = new();

            _playMan.PlayerStatusChanged += (sender, args) =>
            {
                var session = args.Session;
                var status = args.NewStatus;

                switch (status)
                {
                    case Robust.Shared.Enums.SessionStatus.Connected:
                        EnsureUser(session.UserId);
                        break;
                    case Robust.Shared.Enums.SessionStatus.Disconnected:
                        UnensureUser(session.UserId);
                        break;
                    default:
                        break;
                }
            };
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            _expirationAccum += _timing.TickPeriod;
            if (_expirationAccum >= _expirationTime)
            {
                _expirationAccum = TimeSpan.Zero;

                foreach (var playerAndKey in _playersTokensKeys)
                {
                    var userId = playerAndKey.Key;

                    if (!_playMan.TryGetSessionById(userId, out var session))
                        continue;

                    var token = EnsureUser(userId);

                    var ev = new DiscordAuthTokenChangedEvent(token, userId);

                    RaiseNetworkEvent(ev, session);
                }
            }
        }

        private string EnsureUser(NetUserId userId)
        {
            var code = GenerateUCode();
            _playersTokensKeys[userId] = code;

            return code;
        }

        private bool UnensureUser(NetUserId userId)
        {
            return _playersTokensKeys.Remove(userId);
        }

        public string GenerateUCode(int length = 32)
        {
            var stb = new StringBuilder();

            var symbols = AllowedCodeSymbols.ToCharArray();

            for (var i = 0; i < length; i++)
            {
                var @char = _random.Pick(symbols);
                stb.Append(@char);
            }

            return stb.ToString();
        }

        [return: NotNullIfNotNull(nameof(id))]
        public string? GetUserCode(NetUserId? id)
        {
            if (id == null)
                return null;

            if (!_playersTokensKeys.TryGetValue(id.Value, out var code))
                code = EnsureUser(id.Value);

            return code;
        }
    }
}
