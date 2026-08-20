using Robust.Shared.Serialization;

namespace Content.Shared._WL.Turrets;

[Serializable, NetSerializable]
public sealed partial class TurretMinderConsoleBoundUserInterfaceState(
        Dictionary<NetEntity, TurretMinderConsoleBUIStateEntry> netEntities) : BoundUserInterfaceState
{
    public readonly Dictionary<NetEntity, TurretMinderConsoleBUIStateEntry> NetEntities = netEntities;
}

[Serializable, NetSerializable]
public readonly record struct TurretMinderConsoleBUIStateEntry(
    bool Disabled,
    string Address,
    string? Prototype);
