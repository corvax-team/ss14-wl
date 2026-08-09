using Robust.Shared.Serialization;

namespace Content.Shared._WL.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class NanoChatUiState : BoundUserInterfaceState
{
    /// <summary>
    ///     Own contacts (active chats), keyed by recipient number.
    /// </summary>
    public readonly Dictionary<uint, NanoChatRecipient> Recipients;

    /// <summary>
    ///     Own message history, keyed by recipient number.
    /// </summary>
    public readonly Dictionary<uint, List<NanoChatMessage>> Messages;

    /// <summary>
    ///     Directory search results (other listed NanoChat users on the same station). Null if not on a station.
    /// </summary>
    public readonly List<NanoChatRecipient>? Directory;

    public readonly uint? CurrentChat;
    public readonly uint OwnNumber;
    public readonly int MaxRecipients;
    public readonly bool NotificationsMuted;
    public readonly bool ListNumber;

    public NanoChatUiState(
        Dictionary<uint, NanoChatRecipient> recipients,
        Dictionary<uint, List<NanoChatMessage>> messages,
        List<NanoChatRecipient>? directory,
        uint? currentChat,
        uint ownNumber,
        int maxRecipients,
        bool notificationsMuted,
        bool listNumber)
    {
        Recipients = recipients;
        Messages = messages;
        Directory = directory;
        CurrentChat = currentChat;
        OwnNumber = ownNumber;
        MaxRecipients = maxRecipients;
        NotificationsMuted = notificationsMuted;
        ListNumber = listNumber;
    }
}
