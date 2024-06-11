using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Turrets
{
    [Serializable, NetSerializable]
    public sealed partial class TurretMinderConsoleBoundUserInterfaceState : BoundUserInterfaceState
    {
        public readonly Dictionary<NetEntity, TurretMinderConsoleBUIStateEntry> NetEntities;

        public TurretMinderConsoleBoundUserInterfaceState(Dictionary<NetEntity, TurretMinderConsoleBUIStateEntry> netEntities)
        {
            NetEntities = netEntities;
        }
    }

    [Serializable, NetSerializable]
    public readonly record struct TurretMinderConsoleBUIStateEntry(
        bool Disabled,
        string Address,
        string? Prototype);
}
