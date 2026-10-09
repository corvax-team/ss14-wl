using Content.Shared.Lathe;
using Content.Shared.Research.Prototypes;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared._WL.Research.Systems;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ComputeServerComponent: Component
{
    [DataField(required: true), AutoNetworkedField]
    public string ContainerName = "server_boards";

    [DataField, AutoNetworkedField]
    public EntityUid? CurrentBoard = null;
}
