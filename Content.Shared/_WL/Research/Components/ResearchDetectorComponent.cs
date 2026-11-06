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

    [DataField("extrimalType")]
    public ProtoId<ResearchPointsTypePrototype> ExtrimalPointsType = "Experimental";

    [DataField("pointsType")]
    public ProtoId<ResearchPointsTypePrototype>? DefaultPointsType = null;

    [DataField]
    public FixedPoint4 ResearchPercent = 0.01;

    [DataField]
    public float Range = 10f;
}

[ByRefEvent]
public record struct GetResearchDataEvent
{
    public EntityUid Detector;

    public ProtoId<ResearchTypePrototype> ResearchType;

    public Dictionary<ProtoId<ResearchCategoryPrototype>, (FixedPoint4, ResearchPointsSpecifier)> ResearchData;

    public GetResearchDataEvent(EntityUid detector, ProtoId<ResearchTypePrototype> researchType)
    {
        Detector = detector;
        ResearchType = researchType;
        ResearchData = new();
    }
}
