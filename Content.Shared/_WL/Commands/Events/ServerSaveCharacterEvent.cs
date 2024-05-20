using Content.Shared.Preferences;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Commands.Events
{
    [Serializable, NetSerializable]
    public sealed partial class ServerSaveCharacterEvent : EntityEventArgs
    {
        public readonly ICharacterProfile Profile;
        public readonly int Slot;

        public ServerSaveCharacterEvent(ICharacterProfile profile, int slot)
        {
            Profile = profile;
            Slot = slot;
        }
    }
}
