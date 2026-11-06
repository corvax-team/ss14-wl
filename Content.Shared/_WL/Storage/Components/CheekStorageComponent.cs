using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Storage.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class CheekStorageComponent : Component;

public sealed partial class OpenCheekStorageEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class CheekStorageDoAfterEvent : SimpleDoAfterEvent;
