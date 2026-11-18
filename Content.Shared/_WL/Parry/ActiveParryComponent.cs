using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Parry;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedParrySystem))]
public sealed partial class ActiveParryComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Weapon;

    [DataField, AutoNetworkedField]
    public bool IsParrying = true;

    [DataField, AutoNetworkedField]
    public TimeSpan ParryStartTime;
}

[Serializable, NetSerializable]
public sealed class ParryStartEvent : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class ParryStopEvent : EntityEventArgs;
