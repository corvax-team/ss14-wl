using Content.Shared._WL.Stand_Fall_Crouch;
using Content.Shared._WL.StandFallCrouch;
using Content.Shared.GameTicking;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client._WL.StandFallCrouch
{
    public sealed partial class ClientStandFallCrouchSystem : SharedStandFallCrouchSystem
    {
        private Dictionary<EntProtoId, Shared.DrawDepth.DrawDepth> _cachedDrawDepths = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<StandFallCrouchComponent, AfterAutoHandleStateEvent>(AfterHandle);
            SubscribeNetworkEvent<RoundRestartCleanupEvent>(OnRoundEnd);

            _cachedDrawDepths = new();
        }

        private void AfterHandle(EntityUid ent, StandFallCrouchComponent comp, ref AfterAutoHandleStateEvent args)
        {
            var user = ent;
            var prototype = Prototype(user);

            if (prototype == null)
                return;

            if (!TryComp<StandFallCrouchComponent>(user, out var standFallCrouchComp))
                return;

            if (!TryComp<SpriteComponent>(user, out var spriteComp))
                return;

            if (!_cachedDrawDepths.TryGetValue(prototype.ID, out var oldDrawDepth))
            {
                if (prototype.Components.TryGetComponent("Sprite", out var innerSpriteCompNull) &&
                    innerSpriteCompNull is SpriteComponent innerSpriteComp)
                {
                    var dd = (Shared.DrawDepth.DrawDepth)innerSpriteComp.DrawDepth;
                    _cachedDrawDepths[prototype.ID] = dd;
                    oldDrawDepth = dd;
                }
                else
                {
                    //Fallback вариант
                    oldDrawDepth = Shared.DrawDepth.DrawDepth.Objects;
                }
            }

            spriteComp.DrawDepth = standFallCrouchComp.IsCrawling
                ? (int)Shared.DrawDepth.DrawDepth.WallTops
                : (int)oldDrawDepth;
        }

        private void OnRoundEnd(RoundRestartCleanupEvent _)
        {
            _cachedDrawDepths.Clear();
        }
    }
}
