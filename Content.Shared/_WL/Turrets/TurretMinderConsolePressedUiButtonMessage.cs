using Robust.Shared.Serialization;

namespace Content.Shared._WL.Turrets;

[Serializable, NetSerializable]
public sealed partial class TurretMinderConsolePressedUiButtonMessage(NetEntity turret) : BoundUserInterfaceMessage
{
    public readonly NetEntity Turret = turret;
}
