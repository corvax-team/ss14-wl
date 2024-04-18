using Content.Shared._WL.Slimes;
using Content.Shared._WL.Slimes.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using System.Linq;

namespace Content.Server._WL.Slimes.SlimeTransformationConditions;

public sealed partial class RandomMutationCondition : SlimeTransformationCondition
{
    [DataField]
    public List<SlimeTransformationCondition> Conditions = new();

    private SlimeTransformationCondition? ChosenCondition = null;

    public override bool Condition(SlimeTransformationConditionArgs args)
    {
        var random = IoCManager.Resolve<IRobustRandom>();
        var protoMan = IoCManager.Resolve<IPrototypeManager>();

        ChosenCondition ??= random.Pick(Conditions).GetRandomCondition(args.EntityManager, protoMan, random);

        return ChosenCondition.Condition(args);
    }

    public override string GetDescriptionString(IEntityManager entityManager, IPrototypeManager protoMan)
    {
        var random = IoCManager.Resolve<IRobustRandom>();

        ChosenCondition ??= random.Pick(Conditions).GetRandomCondition(entityManager, protoMan, random);

        return ChosenCondition.GetDescriptionString(entityManager, protoMan);
    }

    public override SlimeTransformationCondition GetRandomCondition(IEntityManager entMan, IPrototypeManager protoMan, IRobustRandom random)
    {
        ChosenCondition ??= random.Pick(Conditions).GetRandomCondition(entMan, protoMan, random);

        return ChosenCondition.GetRandomCondition(entMan, protoMan, random);
    }
}
