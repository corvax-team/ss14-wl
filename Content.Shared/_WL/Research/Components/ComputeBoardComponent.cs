using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ComputeBoardComponent : Component
{
    [DataField(required: true)]
    public LocId Name;

    [DataField("startTime"), AutoNetworkedField]
    public TimeSpan? ColdStartTimeOverride = null;
}
