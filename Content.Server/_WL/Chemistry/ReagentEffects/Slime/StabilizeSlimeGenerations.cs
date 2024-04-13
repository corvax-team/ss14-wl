using Content.Server._WL.Slimes;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Chemistry.ReagentEffects.Slime;

public sealed partial class StabilizeSlimeGenerations : ReagentEffect
{
    [DataField("factor")]
    public float Factor = 0.5f;

    [DataField("increase")]
    public bool Increase = false;

    [DataField("recursive")]
    public bool Recursive = false;

    public override void Effect(ReagentEffectArgs args)
    {
        var entityManager = args.EntityManager;

        if (!entityManager.TryGetComponent<SlimeComponent>(args.SolutionEntity, out var slimeComp))
            return;

        if (slimeComp.CurrentAge is Shared._WL.Slimes.Enums.SlimeLifeStage.Young or Shared._WL.Slimes.Enums.SlimeLifeStage.Dead)
            return;

        slimeComp.CurrentMutationProbability = Increase
            ? Math.Clamp(slimeComp.CurrentMutationProbability + slimeComp.CurrentMutationProbability * Factor, 0, 1)
            : Math.Clamp(slimeComp.CurrentMutationProbability - slimeComp.CurrentMutationProbability * Factor, 0, 1);

        slimeComp.PassMutationPropertiesNextGeneration.PassMutationProbability = true;

        slimeComp.PassMutationPropertiesNextGeneration.Recursive = Recursive;
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-stabilize-generations",
            ("chance", Probability),
            ("increase", Increase ? 1 : 0),
            ("value", Math.Round(Factor * 100, 2)),
            ("recursive", Recursive ? 1 : 0));
}
