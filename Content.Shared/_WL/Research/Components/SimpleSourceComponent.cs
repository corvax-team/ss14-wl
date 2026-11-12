using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Prototypes;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent]
public sealed partial class SimpleSourceComponent : Component
{
    [DataField(required: true)]
    public string Id;

    [DataField]
    public ResearchPointsSpecifier Points = new();
}
