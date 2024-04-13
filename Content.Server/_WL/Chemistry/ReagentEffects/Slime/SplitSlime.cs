using Content.Server._WL.Slimes;
using Content.Server._WL.Slimes.Systems;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Chemistry.ReagentEffects
{
    public sealed partial class SplitSlime : ReagentEffect
    {
        /// <summary>
        /// How much will slime's mutation probability decrease.
        /// </summary>
        [DataField("factor")]
        public float Factor = 0.4f;

        public override void Effect(ReagentEffectArgs args)
        {
            var EntityManager = args.EntityManager;
            var slimeSystem = EntityManager.System<SlimeSystem>();

            if (!EntityManager.TryGetComponent<SlimeComponent>(args.SolutionEntity, out var slimeComp))
                return;

            slimeComp.CurrentMutationProbability *= Factor;
            slimeSystem.Split(args.SolutionEntity, slimeComp);
        }

        protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
            => Loc.GetString("reagent-effect-guidebook-split-slime", ("chance", Probability));
    }
}
