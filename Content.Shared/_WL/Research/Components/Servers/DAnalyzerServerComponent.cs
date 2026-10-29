using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent]
public sealed partial class DAnalyzerServerComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<string, DAnalyzeState> DAnalyzedData = new();
}

public enum DAnalyzeState : byte
{
    NotAnalyzed = 0,
    Analyzing = 1,
    Analyzed = 2,
    DAnalyzing = 3,
    DAnalyzed = 4,
    Invalid = 5
}
