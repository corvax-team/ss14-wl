using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ResearchSourceComponent : Component
{
    [DataField(required: true)]
    public ProtoId<ResearchCategoryPrototype> Category;

    [DataField, AutoNetworkedField]
    public bool Active = true;
}
