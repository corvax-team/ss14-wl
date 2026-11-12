using System.Numerics;
using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Methods;

public sealed partial class AtmosStatsResearchCondition : ResearchConditionBase<AtmosStatsResearchCondition>
{
    [DataField]
    public float MinExtrimal = 273f;

    [DataField]
    public float MaxExtrimal = 333f;

    [DataField]
    public float Slope = 1f;

    [DataField]
    public AtmosStatsType Type = AtmosStatsType.None;
}

public enum AtmosStatsType : byte
{
    None = 0,
    Temperature = 1,
    Pressure = 2
}
