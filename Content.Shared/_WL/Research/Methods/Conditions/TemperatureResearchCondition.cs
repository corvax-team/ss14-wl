using System.Numerics;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.Temperature.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Methods;

public sealed partial class TemperatureResearchConditionSystem : ResearchConditionSystem<TemperatureComponent, TemperatureResearchCondition>
{
    protected override void Condition(Entity<TemperatureComponent> entity, ref ResearchConditionEvent<TemperatureResearchCondition> args)
    {
        args.Result = entity.Comp.Temperature;

        var extraPoints = 0f;

        if (entity.Comp.Temperature < args.Condition.MinExtrimal)
        {
            extraPoints = args.Condition.MinExtrimal - entity.Comp.Temperature;
        }
        else if (entity.Comp.Temperature > args.Condition.MaxExtrimal)
        {
            extraPoints = entity.Comp.Temperature - args.Condition.MaxExtrimal;
        }

        args.Points.PointsDict.Add(args.Condition.ExtrimalType, extraPoints / 100 * args.Condition.BaseValue);
    }
}

public sealed partial class TemperatureResearchCondition : ResearchConditionBase<TemperatureResearchCondition>
{
    [DataField]
    public float MinExtrimal = 0f;

    [DataField]
    public float MaxExtrimal = float.PositiveInfinity;
}
