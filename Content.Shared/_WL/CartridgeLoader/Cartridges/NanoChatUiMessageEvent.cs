using Content.Shared.CartridgeLoader;
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
    ///     The content of the message, or the recipient's name when starting a new chat.
    /// </summary>
    public readonly string? Content;

    /// <summary>
    ///     The recipient's job title, only used when creating a new chat from the directory.
    /// </summary>
    public readonly string? RecipientJob;

    public NanoChatUiMessageEvent(NanoChatUiMessageType type,
        uint? recipientNumber = null,
        string? content = null,
        string? recipientJob = null)
    {
        Type = type;
        RecipientNumber = recipientNumber;
        Content = content;
        RecipientJob = recipientJob;
    }
}

[Serializable, NetSerializable]
public enum NanoChatUiMessageType : byte
{
    NewChat,
    SelectChat,
    SendMessage,
    DeleteChat,
    ToggleMute,
    ToggleListNumber,
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
    ///     Whether this recipient has unread messages.
    /// </summary>
    public bool HasUnread;

    public NanoChatRecipient(uint number, string name, string? jobTitle = null, bool hasUnread = false)
    {
        Number = number;
        Name = name;
        JobTitle = jobTitle;
        HasUnread = hasUnread;
    }
}

[Serializable, NetSerializable, DataRecord]
public partial struct NanoChatMessage
{
    public const int MaxLength = 256;

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

    public NanoChatMessage(TimeSpan timestamp, string content, uint sender, bool deliveryFailed = false)
    {
        Timestamp = timestamp;
        Content = content;
        Sender = sender;
        DeliveryFailed = deliveryFailed;
    }
}

/// <summary>
///     NanoChat log data, used by the LogProbe cartridge.
/// </summary>
[Serializable, NetSerializable, DataRecord]
public readonly partial struct NanoChatData(
    Dictionary<uint, NanoChatRecipient> recipients,
    Dictionary<uint, List<NanoChatMessage>> messages,
    uint? cardNumber,
    NetEntity card)
{
    public Dictionary<uint, NanoChatRecipient> Recipients { get; } = recipients;
    public Dictionary<uint, List<NanoChatMessage>> Messages { get; } = messages;
    public uint? CardNumber { get; } = cardNumber;
    public NetEntity Card { get; } = card;
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
