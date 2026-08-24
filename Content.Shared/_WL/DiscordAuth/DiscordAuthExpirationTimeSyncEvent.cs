using Robust.Shared.Serialization;

namespace Content.Shared._WL.DiscordAuth;

[Serializable, NetSerializable]
public sealed partial class DiscordAuthExpirationTimeSyncEvent(TimeSpan expirationAccum, TimeSpan expirationTime) : EntityEventArgs
{
    public readonly TimeSpan ExpirationAccum = expirationAccum;
    public readonly TimeSpan ExpirationTime = expirationTime;
}
