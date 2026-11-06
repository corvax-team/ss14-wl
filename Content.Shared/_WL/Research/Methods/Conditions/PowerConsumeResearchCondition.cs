using System.Numerics;
using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Methods;

public sealed partial class PowerConsumeResearchCondition : ResearchConditionBase<PowerConsumeResearchCondition>
{
    [DataField]
    public float ExtrimalValue = 0f;
}
