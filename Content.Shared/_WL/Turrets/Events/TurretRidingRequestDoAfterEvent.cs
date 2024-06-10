using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Turrets.Events
{
    [Serializable, NetSerializable]
    public sealed partial class TurretRidingRequestDoAfterEvent : SimpleDoAfterEvent
    {
    }
}
