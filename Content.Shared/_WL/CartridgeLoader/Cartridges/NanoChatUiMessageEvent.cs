using System.Linq;
using Content.Shared.CartridgeLoader;
using Content.Shared.StatusIcon;
using Content.Shared._WL.NanoChat;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class NanoChatUiMessageEvent : CartridgeMessageEvent
{
    /// <summary>
    ///     The type of UI message being sent.
    /// </summary>
    public readonly NanoChatUiMessageType Type;

    /// <summary>
    ///     The recipient's NanoChat number, if applicable.
    /// </summary>
    public readonly uint? RecipientNumber;

    /// <summary>
    ///     Message content or a group name, depending on <see cref="Type"/>.
    /// </summary>
    public readonly string? Content;

    public readonly NanoChatConversationId? Conversation;
    public readonly uint? TargetNumber;
    public readonly List<uint>? MemberNumbers;
    public readonly bool? Value;

    public NanoChatUiMessageEvent(NanoChatUiMessageType type,
        uint? recipientNumber = null,
        string? content = null,
        NanoChatConversationId? conversation = null,
        uint? targetNumber = null,
        List<uint>? memberNumbers = null,
        bool? value = null)
    {
        Type = type;
        RecipientNumber = recipientNumber;
        Content = content;
        Conversation = conversation;
        TargetNumber = targetNumber;
        MemberNumbers = memberNumbers;
        Value = value;
    }
}

[Serializable, NetSerializable]
public enum NanoChatUiMessageType : byte
{
    NewChat,
    SendMessage,
    DeleteChat,
    ToggleMute,
    ToggleListNumber,
    CreateGroup,
    SelectConversation,
    RenameGroup,
    AddGroupMember,
    RemoveGroupMember,
    SetGroupAdmin,
    LeaveGroup,
    DeleteGroup,
    ToggleGroupMute,
    BlockContact,
    UnblockContact,
    LoadOlderMessages,
}

[Serializable, NetSerializable, DataRecord]
public partial struct NanoChatRecipient
{
    /// <summary>
    ///     The recipient's unique NanoChat number.
    /// </summary>
    public uint Number;

    /// <summary>
    ///     The recipient's display name, typically from their ID card.
    /// </summary>
    public string Name;

    /// <summary>
    ///     The recipient's job title, if available.
    /// </summary>
    public string? JobTitle;

    /// <summary>
    ///     The job icon copied from the recipient's ID card.
    /// </summary>
    public ProtoId<JobIconPrototype> JobIcon;

    /// <summary>
    ///     Whether this recipient has unread messages.
    /// </summary>
    public bool HasUnread;

    public NanoChatRecipient(
        uint number,
        string name,
        string? jobTitle = null,
        bool hasUnread = false,
        ProtoId<JobIconPrototype>? jobIcon = null)
    {
        Number = number;
        Name = name;
        JobTitle = jobTitle;
        HasUnread = hasUnread;
        JobIcon = jobIcon ?? "JobIconUnknown";
    }
}

[Serializable, NetSerializable, DataRecord]
public partial struct NanoChatMessage
{
    public const int MaxLength = 256;
    public const int MaxMarkupLength = 1024;

    /// <summary>
    ///     When the message was sent.
    /// </summary>
    public TimeSpan Timestamp;

    /// <summary>
    ///     The content of the message.
    /// </summary>
    public string Content;

    /// <summary>
    ///     The NanoChat number of the sender.
    /// </summary>
    public uint Sender;

    /// <summary>
    ///     Whether the message failed to deliver to the recipient.
    ///     Happens if the recipient is out of range or there's no active telecomms server.
    /// </summary>
    public bool DeliveryFailed;

    /// <summary>
    ///     Delivery counters used by group conversations.
    /// </summary>
    public byte DeliveredRecipients;
    public byte IntendedRecipients;

    public NanoChatMessage(
        TimeSpan timestamp,
        string content,
        uint sender,
        bool deliveryFailed = false,
        byte deliveredRecipients = 0,
        byte intendedRecipients = 0)
    {
        Timestamp = timestamp;
        Content = content;
        Sender = sender;
        DeliveryFailed = deliveryFailed;
        DeliveredRecipients = deliveredRecipients;
        IntendedRecipients = intendedRecipients;
    }
}

/// <summary>
///     NanoChat log data, used by the LogProbe cartridge.
/// </summary>
[Serializable, NetSerializable, DataRecord]
public readonly partial struct NanoChatData(
    Dictionary<uint, NanoChatRecipient> recipients,
    Dictionary<uint, List<NanoChatMessage>> messages,
    Dictionary<uint, int> messageCounts,
    Dictionary<uint, NanoChatGroup> groups,
    Dictionary<uint, List<NanoChatMessage>> groupMessages,
    Dictionary<uint, int> groupMessageCounts,
    HashSet<uint> blockedNumbers,
    uint? cardNumber,
    NetEntity card)
{
    public Dictionary<uint, NanoChatRecipient> Recipients { get; } = recipients;
    public Dictionary<uint, List<NanoChatMessage>> Messages { get; } = messages;
    public Dictionary<uint, int> MessageCounts { get; } = messageCounts;
    public Dictionary<uint, NanoChatGroup> Groups { get; } = groups;
    public Dictionary<uint, List<NanoChatMessage>> GroupMessages { get; } = groupMessages;
    public Dictionary<uint, int> GroupMessageCounts { get; } = groupMessageCounts;
    public HashSet<uint> BlockedNumbers { get; } = blockedNumbers;
    public uint? CardNumber { get; } = cardNumber;
    public NetEntity Card { get; } = card;

    public static NanoChatData FromCard(NanoChatCardComponent card, NetEntity cardEntity)
        => new(
            new Dictionary<uint, NanoChatRecipient>(card.Recipients),
            card.Messages.ToDictionary(pair => pair.Key, pair => new List<NanoChatMessage>(pair.Value)),
            card.Messages.ToDictionary(pair => pair.Key, pair => pair.Value.Count),
            card.Groups.ToDictionary(pair => pair.Key, pair => CloneGroup(pair.Value)),
            card.GroupMessages.ToDictionary(pair => pair.Key, pair => new List<NanoChatMessage>(pair.Value)),
            card.GroupMessages.ToDictionary(pair => pair.Key, pair => pair.Value.Count),
            new HashSet<uint>(card.BlockedNumbers),
            card.Number,
            cardEntity);

    private static NanoChatGroup CloneGroup(NanoChatGroup group)
        => group with { Members = new Dictionary<uint, NanoChatGroupMember>(group.Members) };
}

/// <summary>
///     Raised on the NanoChat card whenever a recipient gets added or updated.
/// </summary>
[ByRefEvent]
public readonly record struct NanoChatRecipientUpdatedEvent(EntityUid CardUid);

/// <summary>
///     Raised on the NanoChat card whenever it sends or receives a message.
/// </summary>
[ByRefEvent]
public readonly record struct NanoChatMessageReceivedEvent(EntityUid CardUid);
