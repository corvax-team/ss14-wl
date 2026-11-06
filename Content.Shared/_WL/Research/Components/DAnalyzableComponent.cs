using Content.Shared._WL.Research;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent]
public sealed partial class DAnalyzableComponent : Component
{
    [DataField("id", required: true)]
    public string AnalyzeId;

    [DataField("dType")]
    public DAnalyzeType AnalyzeType = DAnalyzeType.All;

    [DataField(required: true)]
    public ResearchPointsSpecifier DeconstructPoints = new();

    [DataField]
    public ResearchPointsSpecifier AnalyzePoints = new();
}
