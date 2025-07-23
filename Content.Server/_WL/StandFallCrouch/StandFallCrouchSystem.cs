using Content.Shared._WL.Input;
using Content.Shared._WL.Stand_Fall_Crouch;
using Content.Shared._WL.StandFallCrouch;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;

namespace Content.Server._WL.StandFallCrouch
{
    public sealed partial class StandFallCrouchSystem : SharedStandFallCrouchSystem
    {
        public override void Initialize()
        {
            base.Initialize();

            CommandBinds.Builder
                .Bind(WLContentKeyFunctions.FallDownAndStandUp, InputCmdHandler.FromDelegate(sesion => StandOrUp(sesion)))
                .Register<StandFallCrouchSystem>();
        }

        public bool StandOrUp(ICommonSession? session)
        {
            if (session?.AttachedEntity == null)
                return false;

            var ent = session.AttachedEntity.Value;

            if (!TryComp<StandFallCrouchComponent>(ent, out var comp))
                return false;

            TryStartStandFallDoAfter(ent, ent, comp);

            return true;
        }
    }
}
