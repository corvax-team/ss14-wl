using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.InputMover
{
    [Serializable, NetSerializable]
    public sealed partial class RunningOnShiftNeedsUpdateEvent : EntityEventArgs
    {
        public readonly NetUserId NetUserId;
        public readonly bool Value;

        public RunningOnShiftNeedsUpdateEvent(NetUserId netUserId, bool value)
        {
            NetUserId = netUserId;
            Value = value;
        }
    }
}
