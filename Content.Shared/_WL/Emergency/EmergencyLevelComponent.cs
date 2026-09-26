using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._WL.Emergency;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(EmergencyLevelSystem))]

public sealed partial class EmergencyLevelComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public List<ProtoId<EmergencyLevelPrototype>> AvailableEmergencyLevels = new();

    [DataField, AutoNetworkedField]
    public ProtoId<EmergencyLevelPrototype> DefaultEmergencyLevel = "NoEmergency";

    [DataField, AutoNetworkedField]
    public ProtoId<EmergencyLevelPrototype> CurrentEmergencyLevel = "NoEmergency";

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan? DelayedUntil;
}
