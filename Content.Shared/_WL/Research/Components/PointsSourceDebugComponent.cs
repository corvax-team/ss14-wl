using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PointsSourceDebugComponent : Component
{
    [AutoNetworkedField]
    [DataField("pointsType", required: true)]
    public ProtoId<ResearchPointsTypePrototype> PointsType;

    [AutoNetworkedField]
    [DataField("category", required: true)]
    public ProtoId<ResearchCategoryPrototype> DebugCategory;

    [DataField]
    public double Points = 10;

    [DataField]
    public bool Active = true;

    [DataField("researche")]
    public ProtoId<ResearchTypePrototype> Type;

    [DataField("researchValue")]
    public double Value = 10;
}
