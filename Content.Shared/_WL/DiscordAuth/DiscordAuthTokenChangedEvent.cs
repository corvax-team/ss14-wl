using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.DiscordAuth;

[Serializable, NetSerializable]
public sealed partial class DiscordAuthTokenChangedEvent(string newToken, NetUserId session) : EntityEventArgs
{
    public readonly string NewToken = newToken;
    public readonly NetUserId Session = session;
}

