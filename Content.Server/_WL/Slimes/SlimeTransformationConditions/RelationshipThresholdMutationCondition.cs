using Content.Shared._WL.Slimes;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Slimes.SlimeTransformationConditions;


public sealed partial class RelationshipThresholdMutationCondition : SlimeTransformationCondition
{
    [DataField("requiredAll")]
    public bool RequiredAll = false;

    [DataField("radius")]
    public float Radius = 2f;

    [DataField("min")]
    public float Min = 0;

    [DataField("max")]
    public float Max = 600;

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

            if (!slimeTransform.Coordinates.InRange(EntityManager, _transform, targetTransform.Coordinates, Radius))
                continue;

            if (relationship.Value >= Min && relationship.Value <= Max)
                success += 1;
        }
        return RequiredAll
            ? success == slimeComp.Relationships.Count
            : success > 0;
    }

    public override string GetDescriptionString(IEntityManager entityManager, IPrototypeManager protoMan)
            => Loc.GetString("slime-transformation-condition-relationship",
                ("min", Min),
                ("max", Max),
                ("all", RequiredAll ? 1 : 0),
                ("radius", Radius));
}
