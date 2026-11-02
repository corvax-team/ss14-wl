using System.Numerics;
using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Physics.Components;

namespace Content.Shared._WL.Research.Methods;

public sealed partial class GridSpeedResearchConditionSystem : ResearchConditionSystem<TransformComponent, GridSpeedResearchCondition>
{
    protected override void Condition(Entity<TransformComponent> entity, ref ResearchConditionEvent<GridSpeedResearchCondition> args)
    {
        if (entity.Comp.GridUid is not {} gridUid ||
                !TryComp<PhysicsComponent>(gridUid, out var gridBody))
            return;

        switch (args.Condition.Type)
        {
            case GridSpeedType.Linear:
                args.Result = gridBody.LinearVelocity.Lenght();
                break;

            case GridSpeedType.Linear:
                args.Result = gridBody.AngularVelocity;
                break;
        }

        var delta = FixedPoint4.Max(args.Result - args.Condition.ExtrimalValue, 0);

        args.Points.PointsDict.Add(args.Condition.ExtrimalType, delta);
        args.Points.PointsDict.Add(args.Condition.BaseType, 100);
    }
}

public sealed partial class GridSpeedResearchCondition : ResearchConditionBase<GridSpeedResearchCondition>
{
    [DataField]
    public float ExtrimalValue = 100f;

    [DataField]
    public GridSpeedType Type = GridSpeedType.None;
}

public enum GridSpeedType : byte
{
    None = 0,
    Linear = 1,
    Angular = 2
}
