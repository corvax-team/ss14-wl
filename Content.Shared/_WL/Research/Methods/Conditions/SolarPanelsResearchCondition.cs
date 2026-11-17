using System.Numerics;
using Content.Shared._WL.Research.Methods;
using Content.Shared.FixedPoint;

namespace Content.Shared._WL.Research.Methods;

public sealed partial class SolarPanelsResearchCondition : ResearchConditionBase<SolarPanelsResearchCondition>
{
    [DataField]
    public FixedPoint4 Factor = 100;
}
