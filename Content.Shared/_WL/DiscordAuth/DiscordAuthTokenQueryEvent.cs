using Robust.Shared.Serialization;

namespace Content.Shared._WL.DiscordAuth;

[Serializable, NetSerializable]
public sealed partial class DiscordAuthTokenQueryEvent : EntityEventArgs;
