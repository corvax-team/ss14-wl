using Content.Shared._WL.DiscordAuth;
using Robust.Client.Player;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Client._WL.DiscordAuth
{
    public sealed partial class ClientDiscordAuthSystem : EntitySystem
    {
        [Dependency] private IPlayerManager _playMan = default!;
        [Dependency] private IGameTiming _timing = default!;

        private event Action<string>? OnTokenChanged;

        private string? _token;

        private TimeSpan _expirationAccum = TimeSpan.Zero;
        private TimeSpan? _expirationTime;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeNetworkEvent<DiscordAuthTokenChangedEvent>(OnTokenChangedEvent);
            SubscribeNetworkEvent<DiscordAuthExpirationTimeSyncEvent>((args) =>
            {
                _expirationAccum = args.ExpirationAccum;
                _expirationTime = args.ExpirationTime;
            });

            OnTokenChanged = (_) => { };

            QueryCode();
            QuerySync();
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            if (!_timing.IsFirstTimePredicted)
                return;

            _expirationAccum += _timing.TickPeriod;
            if (_expirationAccum >= _expirationTime)
                _expirationAccum = TimeSpan.Zero;
        }

        private void OnTokenChangedEvent(DiscordAuthTokenChangedEvent args)
        {
            if (args.Session.UserId != _playMan.LocalSession?.UserId)
                return;

            _token = args.NewToken;
            OnTokenChanged?.Invoke(args.NewToken);
        }

        public void SubscribeOnTokenChanged(Action<string> action)
        {
            OnTokenChanged += action;
        }

        public float GetExpirationTimeFraction(bool revert = true)
        {
            if (_expirationTime == null)
                return 0f;

            var fraction = _expirationAccum / _expirationTime;

            return (float)(revert
                ? 1d - fraction
                : fraction);
        }

        public void QueryCode()
        {
            if (_playMan?.LocalSession is not ICommonSession session)
                return;

            RaiseNetworkEvent(new DiscordAuthTokenQueryEvent());
        }

        public void QuerySync()
        {
            if (_playMan?.LocalSession is not ICommonSession session)
                return;

            RaiseNetworkEvent(new DiscordAuthExpirationTimeSyncEvent(TimeSpan.Zero, TimeSpan.Zero));
        }

        public string? GetUserCode()
        {
            return _token;
        }
    }
}
