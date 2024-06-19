using Content.Shared._WL.CCVars;
using Content.Shared._WL.InputMover;
using Robust.Client.Player;
using Robust.Shared.Configuration;

namespace Content.Client._WL.InputMover
{
    public sealed partial class ClientInputMoverSystem : EntitySystem
    {
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] private readonly IPlayerManager _playMan = default!;

        public bool RunningOnShift { get; private set; } = WLCVars.RunningOnShift.DefaultValue;

        public override void Initialize()
        {
            base.Initialize();

            var cvarValue = _cfg.GetCVar(WLCVars.RunningOnShift);

            RunningOnShift = cvarValue;

            Update(cvarValue);

            Subs.CVar(_cfg, WLCVars.RunningOnShift, Update, true);
        }

        private void Update(bool value)
        {
            var session = _playMan.LocalSession;
            if (session != null)
                RaiseNetworkEvent(new RunningOnShiftNeedsUpdateEvent(session.UserId, value));
        }
    }
}
