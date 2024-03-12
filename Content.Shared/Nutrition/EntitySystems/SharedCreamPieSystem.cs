using Content.Shared.Nutrition.Components;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using JetBrains.Annotations;
// WL Golem species start
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;
using Content.Shared.Buckle;
using Content.Shared._WL.Slippery;
// WL Golem species end

namespace Content.Shared.Nutrition.EntitySystems
{
    [UsedImplicitly]
    public abstract class SharedCreamPieSystem : EntitySystem
    {
        [Dependency] private SharedStunSystem _stunSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        // WL Golem species start
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototype = default!;
        [Dependency] private readonly IEntityManager _entity = default!;
        [Dependency] private readonly SharedBuckleSystem _buckle = default!;
        // WL Golem species end

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<CreamPieComponent, ThrowDoHitEvent>(OnCreamPieHit);
            SubscribeLocalEvent<CreamPieComponent, LandEvent>(OnCreamPieLand);
            SubscribeLocalEvent<CreamPiedComponent, ThrowHitByEvent>(OnCreamPiedHitBy);
        }

        public void SplatCreamPie(EntityUid uid, CreamPieComponent creamPie)
        {
            // Already splatted! Do nothing.
            if (creamPie.Splatted)
                return;

            creamPie.Splatted = true;

            SplattedCreamPie(uid, creamPie);
        }

        protected virtual void SplattedCreamPie(EntityUid uid, CreamPieComponent creamPie) {}

        public void SetCreamPied(EntityUid uid, CreamPiedComponent creamPied, bool value)
        {
            if (value == creamPied.CreamPied)
                return;

            creamPied.CreamPied = value;

            if (EntityManager.TryGetComponent(uid, out AppearanceComponent? appearance))
            {
                _appearance.SetData(uid, CreamPiedVisuals.Creamed, value, appearance);
            }
        }

        private void OnCreamPieLand(EntityUid uid, CreamPieComponent component, ref LandEvent args)
        {
            SplatCreamPie(uid, component);
        }

        private void OnCreamPieHit(EntityUid uid, CreamPieComponent component, ThrowDoHitEvent args)
        {
            SplatCreamPie(uid, component);
        }

        private void OnCreamPiedHitBy(EntityUid uid, CreamPiedComponent creamPied, ThrowHitByEvent args)
        {
            if (!EntityManager.EntityExists(args.Thrown) || !EntityManager.TryGetComponent(args.Thrown, out CreamPieComponent? creamPie)) return;

            SetCreamPied(uid, creamPied, true);

            CreamedEntity(uid, creamPied, args);

            _stunSystem.TryParalyze(uid, TimeSpan.FromSeconds(creamPie.ParalyzeTime), true);

            // WL Golem species start
            if (!_buckle.IsBuckled(uid))
            {
                if (_entity.TryGetComponent<HardSlipComponent>(uid, out var hardslip))
                {
                    if (hardslip is not null)
                    {
                        var damageSpec = new DamageSpecifier(_prototype.Index<DamageTypePrototype>("Blunt"), hardslip.FallDamage);
                        _damageableSystem.TryChangeDamage(uid, damageSpec);
                    }
                }
            }
            // WL Golem species end
        }

        protected virtual void CreamedEntity(EntityUid uid, CreamPiedComponent creamPied, ThrowHitByEvent args) {}
    }
}
