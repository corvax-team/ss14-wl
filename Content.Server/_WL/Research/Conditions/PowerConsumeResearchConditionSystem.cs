using System.Numerics;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared._WL.Research.Methods;
using Content.Shared.FixedPoint;
using Content.Server.Power.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Research.Methods;

public sealed partial class PowerConsumeResearchConditionSystem : ResearchConditionSystem<PowerConsumerComponent, PowerConsumeResearchCondition>
{
    protected override void Condition(Entity<PowerConsumerComponent> entity, ref ResearchConditionEvent<PowerConsumeResearchCondition> args)
    {
        args.Result = entity.Comp.ReceivedPower;

        var delta = FixedPoint4.Max(args.Result - args.Condition.ExtrimalValue * args.Condition.Slope, 0);

        args.Points.PointsDict.Add(args.Condition.ExtrimalType, delta * args.Condition.Slope);
        args.Points.PointsDict.Add(args.Condition.BaseType, 100);
    }
}

public sealed partial class PowerConsumeResearchCondition : ResearchConditionBase<PowerConsumeResearchCondition>
{
    [DataField]
    public float ExtrimalValue = 0f;

    [DataField]
    public float Slope = 0.01f;
}
