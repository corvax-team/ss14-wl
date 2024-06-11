using Robust.Shared.Serialization;

namespace Content.Shared._WL.Turrets
{
    [Serializable, NetSerializable]
    public sealed partial class TurretMinderConsoleBoundUserInterfaceState : BoundUserInterfaceState
    {
        public readonly IEnumerable<NetEntity> NetEntities;

        public TurretMinderConsoleBoundUserInterfaceState(IEnumerable<NetEntity> netEntities)
        {
            NetEntities = netEntities;
        }
    }
}
