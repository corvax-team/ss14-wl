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
public sealed partial class ComputeServerComponent : Component
{
    [DataField, AutoNetworkedField]
    public string ContainerName = "storagebase";

    [DataField, AutoNetworkedField]
    public EntityUid? CurrentBoard = null;

    [AutoNetworkedField]
    public TimeSpan ColdStartTime;

    [DataField, AutoNetworkedField]
    public TimeSpan BaseColdStartCooldown = TimeSpan.FromSeconds(3);

    [ViewVariables, AutoNetworkedField]
    public TimeSpan CurrentColdStartCooldown = TimeSpan.FromSeconds(3);

    [AutoNetworkedField]
    public ColdStartState StartState = ColdStartState.Idle;

}

public enum ColdStartState
{
    Idle = 0,
    Starting = 1,
    Ready = 2,
    Invalid = 3
}
