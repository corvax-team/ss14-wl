using System.Numerics;
using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Methods;

public sealed partial class RadiationedResearchCondition : ResearchConditionBase<RadiationedResearchCondition>
{
    [DataField]
    public float ExtrimMin = 0f;

    [DataField]
    public float ExtrimHalfPeak = 1f;
}
