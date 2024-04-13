using Content.Shared._WL.Slimes;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Server._WL.Slimes.SlimeTransformationConditions;

public sealed partial class RandomMutationCondition : SlimeTransformationCondition
{
    [DataField(required: true)]
    public List<SlimeTransformationCondition> Conditions = new();

    public override bool Condition(SlimeTransformationConditionArgs args)
    {
        var _random = IoCManager.Resolve<IRobustRandom>();

        var condition = _random.Pick(Conditions);

        if (Conditions.Count > 1)
            Conditions.RemoveRange(1, Conditions.Count - 1);

        return condition.Condition(args);
    }

    public override string GetDescriptionString(IEntityManager entityManager, IPrototypeManager protoMan)
    {
        if (Conditions.Count > 1)
            Conditions.RemoveRange(1, Conditions.Count - 1);

        return Conditions.First().GetDescriptionString(entityManager, protoMan);
    }
}
