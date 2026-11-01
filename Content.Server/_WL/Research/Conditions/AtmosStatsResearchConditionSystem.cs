using System.Numerics;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared._WL.Research.Methods;
using Content.Shared.FixedPoint;
using Content.Server.Atmos.EntitySystems;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Research.Methods;

public sealed partial class AtmosStatsResearchConditionSystem : ResearchConditionSystem<TransformComponent, AtmosStatsResearchCondition>
{
    [Dependency] private AtmosphereSystem _atmosphereSystem = default!;

    protected override void Condition(Entity<TransformComponent> entity, ref ResearchConditionEvent<AtmosStatsResearchCondition> args)
    {
        if (_atmosphereSystem.GetContainingMixture(entity.Owner) is not { } mixture)
            return;

        switch (args.Condition.Type)
        {
            case AtmosStatsType.Temperature:
                args.Result = mixture.Temperature;
                break;

            case AtmosStatsType.Pressure:
                args.Result = mixture.Pressure;
                break;
        }

        var delta = FixedPoint2.Max(FixedPoint2.Max(args.Result - args.Condition.MinExtrimal, args.Condition.MaxExtrimal - args.Result), 0);

        args.Points.PointsDict.Add(args.Condition.ExtrimalType, delta * args.Condition.Slope);

        args.Points.PointsDict.Add(args.Condition.BaseType, 100);
    }
}
