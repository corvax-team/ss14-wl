using System.Numerics;
using Content.Shared._WL.Research.Methods;
using Content.Shared._WL.Research.Prototypes;
using Content.Server.Radiation.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Research.Methods;

public sealed partial class RadiationedResearchConditionSystem : ResearchConditionSystem<RadiationReceiverComponent, RadiationedResearchCondition>
{
    protected override void Condition(Entity<RadiationReceiverComponent> entity, ref ResearchConditionEvent<RadiationedResearchCondition> args)
    {
        args.Result = entity.Comp.CurrentRadiation;

        var extrimRatio = Math.Min(0f, entity.Comp.CurrentRadiation - args.Condition.ExtrimMin) / Math.Min(0.1f, args.Condition.ExtrimHalfPeak - args.Condition.ExtrimMin);
        args.Points.PointsDict.Add(args.Condition.ExtrimalType, extrimRatio * 100);

        args.Points.PointsDict.Add(args.Condition.BaseType, 100);
    }
}
