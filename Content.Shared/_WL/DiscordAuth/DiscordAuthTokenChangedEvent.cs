using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.DiscordAuth
{
    [Serializable, NetSerializable]
    public sealed partial class DiscordAuthTokenChangedEvent : EntityEventArgs
    {
        public readonly string NewToken;
        public readonly NetUserId Session;

        public DiscordAuthTokenChangedEvent(string newToken, NetUserId session)
        {
            NewToken = newToken;
            Session = session;
        }
    }
}
