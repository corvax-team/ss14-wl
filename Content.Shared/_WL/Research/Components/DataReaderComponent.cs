using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent]
public sealed partial class DataReaderComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string SlotId = "storage_slot";
}

[Serializable, NetSerializable]
public enum PortState : byte
{
    Empty = 0,
    WrongFormat = 1,
    ReadOnly = 2,
    ReadWrite = 3
}
