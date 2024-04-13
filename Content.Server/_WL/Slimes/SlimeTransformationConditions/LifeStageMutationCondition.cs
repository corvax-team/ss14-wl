using Content.Server._WL.Slimes.Systems;
using Content.Shared._WL.Slimes;
using Content.Shared._WL.Slimes.Enums;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Slimes.SlimeTransformationConditions;

public sealed partial class LifeStageMutationCondition : SlimeTransformationCondition
{
    [DataField("min")]
    public SlimeLifeStage MinStage = SlimeLifeStage.Dead;

    [DataField("max")]
    public SlimeLifeStage MaxStage = SlimeLifeStage.Humanoid;

    public override bool Condition(SlimeTransformationConditionArgs args)
    {
        var entityManager = args.EntityManager;

        if (!entityManager.TryGetComponent<SlimeComponent>(args.Slime, out var slimeComp))
            return false;

        if (slimeComp.CurrentAge < MinStage || slimeComp.CurrentAge > MaxStage)
            return false;

        return true;
    }

    public override string GetDescriptionString(IEntityManager entityManager, IPrototypeManager protoMan)
    {
        var slimeSystem = entityManager.System<SlimeSystem>();

        return Loc.GetString("slime-transformation-condition-life-stage",
                ("min", slimeSystem.GetLocLifeStage(MinStage)),
                ("max", slimeSystem.GetLocLifeStage(MaxStage)),
                ("both", MinStage == MaxStage ? 1 : 0));
    }
}
