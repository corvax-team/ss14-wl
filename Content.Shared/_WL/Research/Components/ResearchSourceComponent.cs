using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ResearchSourceComponent : Component
{
    [DataField("pointsType", required: true)]
    public ProtoId<ResearchPointsTypePrototype> DefaultPointsType;

    [DataField(required: true)]
    public ProtoId<ResearchCategoryPrototype> Category;

    [DataField, AutoNetworkedField]
    public bool Active = true;
}
