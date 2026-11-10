using Content.Shared._WL.Research.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent]
public sealed partial class ResearchDetectorComponent : Component
{
    [DataField("detectTypes", required: true)]
    public List<ProtoId<ResearchTypePrototype>> ResearchTypes;

    [DataField("pointsType")]
    public ProtoId<ResearchPointsTypePrototype>? DefaultPointsType = null;

    [DataField]
    public FixedPoint4 ResearchPercent = 0.01;

    [DataField]
    public bool SelfOnly = false;

    [DataField]
    public bool DetectorPriority = true;
}

[ByRefEvent]
public record struct GetResearchDataEvent
{
    public EntityUid Detector;

    public List<ProtoId<ResearchTypePrototype>> ResearchTypes;

    public Dictionary<ProtoId<ResearchTypePrototype>, FixedPoint4> ResearchData;

    public ResearchPointsSpecifier PointsVec;

    public GetResearchDataEvent(EntityUid detector, List<ProtoId<ResearchTypePrototype>> researchTypes)
    {
        Detector = detector;
        ResearchTypes = researchTypes;
        ResearchData = new();
        PointsVec = new();
    }
}
