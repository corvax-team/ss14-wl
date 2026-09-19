using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ResearchScannableComponent : Component
{
    [DataField("pointsType", required: true)]
    public ProtoId<ResearchPointsTypePrototype> DefaultPointsType;

    [DataField(required: true)]
    public ProtoId<ResearchCategoryPrototype> Category;

    [DataField]
    public string Name = string.Empty;

    [DataField]
    public string Description = string.Empty;

    [DataField, AutoNetworkedField]
    public bool Active = true;
}
