using Robust.Shared.Serialization;

namespace Content.Shared._WL.Turrets
{
    [Serializable, NetSerializable]
    public sealed partial class TurretMinderConsolePressedUiButtonMessage : BoundUserInterfaceMessage
    {
        public readonly NetEntity Turret;

        public TurretMinderConsolePressedUiButtonMessage(NetEntity turret)
        {
            Turret = turret;
        }
    }
}
