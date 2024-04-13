using Content.Server.Humanoid;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Humanoid;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._WL.Chemistry.ReagentEffects
{
    public sealed partial class ChangeSex : ReagentEffect
    {
        [DataField("sex")]
        public Sex? Sex = null;

        /// <summary>
        /// If has value, then the fractions will change for creatures within a given radius.
        /// </summary>
        [DataField("radius")]
        public float? Radius = null;

        [DataField("prob")]
        public float ChangeSexProbability = 1f;

        public override void Effect(ReagentEffectArgs args)
        {
            var entityManager = args.EntityManager;
            var _lookup = entityManager.System<EntityLookupSystem>();
            var _random = IoCManager.Resolve<IRobustRandom>();
            var _humanoidAppearance = entityManager.System<HumanoidAppearanceSystem>();

            var entities = Radius == null
                ? new HashSet<EntityUid>([args.SolutionEntity])
                : _lookup.GetEntitiesInRange(args.SolutionEntity, Radius.Value, LookupFlags.Dynamic);

            foreach (var entity in entities)
            {
                if (!_random.Prob(ChangeSexProbability))
                    continue;

                if (!entityManager.TryGetComponent<HumanoidAppearanceComponent>(entity, out var humAppComp))
                    continue;

                var sex = Sex == null
                    ? humAppComp.Sex == Shared.Humanoid.Sex.Unsexed
                        ? Shared.Humanoid.Sex.Male
                        : humAppComp.Sex + 1
                    : Sex;

                _humanoidAppearance.SetSex(entity, sex.Value, true, humAppComp);
            }
        }

        protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
            => Loc.GetString("reagent-effect-guidebook-change-sex",
                ("chance", Probability),
                ("radius", Radius == null ? 0 : Radius.Value),
                ("sex", Sex == null ? 0 : Sex.Value.ToString()));
    }
}
