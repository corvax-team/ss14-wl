using Content.Shared._WL.Slimes;
using Content.Shared.FixedPoint;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._WL.Slimes.SlimeTransformationConditions;


public sealed partial class RelationshipThresholdMutationCondition : SlimeTransformationCondition
{
    [DataField("requiredAll")]
    public bool RequiredAll = false;

    [DataField("radius")]
    public FixedPoint2 Radius = 2f;

    [DataField("min")]
    public FixedPoint2 Min = 0;

    [DataField("max")]
    public FixedPoint2 Max = 100;

    public override bool Condition(SlimeTransformationConditionArgs args)
    {
        var EntityManager = args.EntityManager;
        var _transform = EntityManager.System<TransformSystem>();

        if (!EntityManager.TryGetComponent<SlimeComponent>(args.Slime, out var slimeComp))
            return false;

        if (slimeComp.Relationships.Count == 0)
            return false;

        int success = 0;
        foreach (var relationship in slimeComp.Relationships)
        {
            if (!EntityManager.TryGetComponent<TransformComponent>(relationship.Key, out var targetTransform))
                continue;

            if (!EntityManager.TryGetComponent<TransformComponent>(args.Slime, out var slimeTransform))
                continue;

            if (!slimeTransform.Coordinates.InRange(EntityManager, _transform, targetTransform.Coordinates, Radius.Float()))
                continue;

            if (relationship.Value >= Min && relationship.Value <= Max)
                success += 1;
        }
        return RequiredAll
            ? success == slimeComp.Relationships.Count
            : success > 0;
    }
    public override SlimeTransformationCondition GetRandomCondition(IEntityManager entMan, IPrototypeManager protoMan, IRobustRandom random)
    {
        var min = random.NextFloat(1, 95);
        var max = random.NextFloat(min + 5, 100);
        return new RelationshipThresholdMutationCondition()
        {
            RequiredAll = random.Prob(0.5f),
            Radius = random.NextFloat(1f, 5f),
            Min = min,
            Max = max
        };
    }

    public override string GetDescriptionString(IEntityManager entityManager, IPrototypeManager protoMan)
            => Loc.GetString("slime-transformation-condition-relationship",
                ("min", Min),
                ("max", Max),
                ("all", RequiredAll ? 1 : 0),
                ("radius", Radius));
}
