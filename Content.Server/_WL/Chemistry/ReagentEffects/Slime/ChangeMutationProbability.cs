using Content.Server._WL.Slimes;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._WL.Chemistry.ReagentEffects;

public sealed partial class ChangeMutationProbability : ReagentEffect
{
    [DataField("amount")]
    public float Amount = 0.01f;

    [DataField("increaseChance")]
    public float IncreaseChance = 0.5f;

    public override void Effect(ReagentEffectArgs args)
    {
        var EntityManager = args.EntityManager;
        var random = IoCManager.Resolve<IRobustRandom>();

        if (!EntityManager.TryGetComponent<SlimeComponent>(args.SolutionEntity, out var slimeComp))
            return;

        if (random.Prob(IncreaseChance))
            slimeComp.CurrentMutationProbability += Amount;
        else slimeComp.CurrentMutationProbability -= Amount;

        Math.Clamp(slimeComp.CurrentMutationProbability, 0f, 1f);
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
            => Loc.GetString("reagent-effect-guidebook-slime-mutation-prob-change",
                ("chance", Probability),
                ("amount", Math.Round(Amount * 100, 2)),
                ("mutprob", Math.Round(IncreaseChance * 100, 2)));
}
