using System.Numerics;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared._WL.Research.Methods;
using Content.Shared.FixedPoint;
using Content.Shared.Power.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Methods;

public sealed partial class BatteryChargeResearchConditionSystem : ResearchConditionSystem<BatteryComponent, BatteryChargeResearchCondition>
{
    protected override void Condition(Entity<BatteryComponent> entity, ref ResearchConditionEvent<BatteryChargeResearchCondition> args)
    {
        args.Result = entity.Comp.LastCharge / entity.Comp.MaxCharge * args.Condition.ValueScale;

        var delta = FixedPoint4.Max(args.Result - args.Condition.ExtrimalCharge * args.Condition.ValueScale, 0);

        args.Points.PointsDict.Add(args.Condition.ExtrimalType, delta / args.Condition.ValueScale * args.Condition.BaseValue);

        args.Active = args.Result != 0;
    }
}

public sealed partial class BatteryChargeResearchCondition : ResearchConditionBase<BatteryChargeResearchCondition>
{
    [DataField]
    public float ValueScale = 100f;

    [DataField]
    public float ExtrimalCharge = 1f;
}
