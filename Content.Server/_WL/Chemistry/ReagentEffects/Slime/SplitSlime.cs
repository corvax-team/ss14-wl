using Content.Server._WL.Slimes;
using Content.Server._WL.Slimes.Systems;
using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Chemistry.ReagentEffects
{
    public sealed partial class SplitSlime : ReagentEffect
    {
        public const string SlimeCoreSolutionName = "core";

        /// <summary>
        /// How much will slime's mutation probability decrease.
        /// </summary>
        [DataField("factor")]
        public float Factor = 0.4f;

        /// <summary>
        /// Проверка. Если ядро слайма содержит такой же реагент, который сейчас выпил слайм, то расщепления не будет.
        /// </summary>
        [DataField("checkCore")]
        public bool CheckSlimeCore = true;

        public override void Effect(ReagentEffectArgs args)
        {
            var EntityManager = args.EntityManager;
            var slimeSystem = EntityManager.System<SlimeSystem>();
            var solutionSystem = EntityManager.System<SolutionContainerSystem>();

            if (!EntityManager.TryGetComponent<SlimeComponent>(args.SolutionEntity, out var slimeComp))
                return;

            if (CheckSlimeCore && args.Reagent != null)
            {
                var coreDummy = EntityManager.Spawn(slimeComp.CorePrototype);
                var solution = solutionSystem.EnsureSolution((coreDummy, null), SlimeCoreSolutionName);
                EntityManager.QueueDeleteEntity(coreDummy);
                if (solution.GetReagentQuantity(new(args.Reagent.ID, null)) > 0)
                    return;
            }

            slimeComp.CurrentMutationProbability *= Factor;
            slimeSystem.Split(args.SolutionEntity, slimeComp);
        }

        protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
            => Loc.GetString("reagent-effect-guidebook-split-slime", ("chance", Probability));
    }
}
