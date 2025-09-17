using Content.Shared.Standing;
using Content.Shared.GameTicking;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client._WL.Standing
{
    public sealed partial class ClientStandingStateSystem : EntitySystem
    {
        private Dictionary<EntProtoId, Shared.DrawDepth.DrawDepth> _cachedDrawDepths = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<StandingStateComponent, DownedEvent>(OnDowned);
            SubscribeLocalEvent<StandingStateComponent, StoodEvent>(OnStood);

            SubscribeNetworkEvent<RoundRestartCleanupEvent>(OnRoundEnd);

            _cachedDrawDepths = new();
        }

        private void OnStood(EntityUid ent, StandingStateComponent comp, ref StoodEvent args)
        {
            var user = ent;
            var prototype = Prototype(user);

            if (prototype == null)
                return;

            if (!TryComp<SpriteComponent>(user, out var spriteComp))
                return;

            if (!_cachedDrawDepths.TryGetValue(prototype.ID, out var oldDrawDepth))
            {
                oldDrawDepth = Shared.DrawDepth.DrawDepth.Objects;
            }

            spriteComp.DrawDepth = (int)oldDrawDepth;
        }

        private void OnDowned(EntityUid ent, StandingStateComponent comp, ref DownedEvent args)
        {
            var user = ent;
            var prototype = Prototype(user);

            if (prototype == null)
                return;

            if (!TryComp<SpriteComponent>(user, out var spriteComp))
                return;

            if (!_cachedDrawDepths.TryGetValue(prototype.ID, out var oldDrawDepth))
            {
                if (TryComp<SpriteComponent>(user, out var innerSpriteCompNull) &&
                    innerSpriteCompNull is SpriteComponent innerSpriteComp)
                {
                    var dd = (Shared.DrawDepth.DrawDepth)innerSpriteComp.DrawDepth;
                    _cachedDrawDepths[prototype.ID] = dd;
                }
            }

            spriteComp.DrawDepth = (int)Shared.DrawDepth.DrawDepth.WallTops;
        }

        private void OnRoundEnd(RoundRestartCleanupEvent _)
        {
            _cachedDrawDepths.Clear();
        }
    }
}
