using Content.Shared.Standing;
using Content.Shared.GameTicking;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Client._WL.Standing
{
    public sealed partial class ClientStandingStateSystem : EntitySystem
    {
        [Dependency] private SpriteSystem _sprite = default!;
        private Dictionary<EntProtoId, DrawDepth> _cachedDrawDepths = default!;

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
                oldDrawDepth = DrawDepth.Objects;
            }

            _sprite.SetDrawDepth((ent, spriteComp), (int)oldDrawDepth);
        }

        private void OnDowned(EntityUid ent, StandingStateComponent comp, ref DownedEvent args)
        {
            var user = ent;
            var prototype = Prototype(user);

            if (prototype == null)
                return;

            if (!HasComp<SpriteComponent>(user))
                return;

            if (!_cachedDrawDepths.ContainsKey(prototype.ID))
            {
                if (TryComp<SpriteComponent>(user, out var innerSpriteCompNull) &&
                    innerSpriteCompNull is SpriteComponent innerSpriteComp)
                {
                    var dd = (DrawDepth)innerSpriteComp.DrawDepth;
                    _cachedDrawDepths[prototype.ID] = dd;
                }
            }

            _sprite.SetDrawDepth(user, (int)DrawDepth.WallTops);
        }

        private void OnRoundEnd(RoundRestartCleanupEvent _)
        {
            _cachedDrawDepths.Clear();
        }
    }
}
