using System.Numerics;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared._WL.Research.Methods;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Methods;

public sealed partial class PowerConsumeResearchCondition : ResearchConditionBase<PowerConsumeResearchCondition>
{
    [DataField]
    public float ExtrimalValue = 0f;

    [DataField]
    public float Slope = 0.01f;
}
