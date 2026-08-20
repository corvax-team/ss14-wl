using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.NanoChat;

[Serializable, NetSerializable]
public readonly record struct NanoChatConversationId(NanoChatConversationType Type, uint Id);

[Serializable, NetSerializable]
public enum NanoChatConversationType : byte
{
    Direct,
    Group,
}

/// <summary>
/// A display snapshot of a group member. Group permissions remain server-authoritative.
/// </summary>
[Serializable, NetSerializable, DataRecord]
public partial struct NanoChatGroupMember(
        uint number,
        string name,
        string? jobTitle = null,
        ProtoId<JobIconPrototype>? jobIcon = null,
        bool isAdmin = false)
{
    public uint Number = number;
    public string Name = name;
    public string? JobTitle = jobTitle;
    public ProtoId<JobIconPrototype> JobIcon = jobIcon ?? "JobIconUnknown";
    public bool IsAdmin = isAdmin;
}

/// <summary>
/// Card-local group metadata. Message history is stored separately on the card.
/// </summary>
[Serializable, NetSerializable, DataRecord]
public partial struct NanoChatGroup(
        uint id,
        string name,
        Dictionary<uint, NanoChatGroupMember> members,
        bool hasUnread = false,
        bool notificationsMuted = false)
{
    public const int MaxMembers = 64;
    public const int MaxNameLength = 32;
    public const int MaxNameMarkupLength = 128;
    public const int MaxGroupsPerCreator = 8;

    public uint Id = id;
    public string Name = name;
    public Dictionary<uint, NanoChatGroupMember> Members = members;
    public bool HasUnread = hasUnread;
    public bool NotificationsMuted = notificationsMuted;
}
