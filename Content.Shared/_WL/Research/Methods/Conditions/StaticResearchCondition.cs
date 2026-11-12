using System.Numerics;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Methods;

public sealed partial class StaticResearchConditionSystem : ResearchConditionSystem<ResearchSourceComponent, StaticResearchCondition>
{
    protected override void Condition(Entity<ResearchSourceComponent> entity, ref ResearchConditionEvent<StaticResearchCondition> args)
    {
        args.Result = args.Condition.Value;

        args.Points.PointsDict.Add(args.Condition.ExtrimalType, args.Condition.ExtrimalValue * args.Condition.BaseValue);
    }
}

public sealed partial class StaticResearchCondition : ResearchConditionBase<StaticResearchCondition>
{
    [DataField]
    public FixedPoint4 Value = 0;

    [DataField]
    public FixedPoint4 ExtrimalValue = 0;
}
