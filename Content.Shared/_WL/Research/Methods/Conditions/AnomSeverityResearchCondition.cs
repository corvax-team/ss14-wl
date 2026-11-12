using System.Numerics;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.Anomaly.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Methods;

public sealed partial class AnomSeverityResearchConditionSystem : ResearchConditionSystem<AnomalyComponent, AnomSeverityResearchCondition>
{
    protected override void Condition(Entity<AnomalyComponent> entity, ref ResearchConditionEvent<AnomSeverityResearchCondition> args)
    {
        args.Result = entity.Comp.Severity * 100;

        args.Points.PointsDict.Add(args.Condition.ExtrimalType, Math.Min(0f, entity.Comp.Severity - args.Condition.MinExtrimal) * args.Condition.BaseValue);
    }
}

public sealed partial class AnomSeverityResearchCondition : ResearchConditionBase<AnomSeverityResearchCondition>
{
    [DataField]
    public float MinExtrimal = 0.6f;
}
