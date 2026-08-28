using Robust.Shared.Serialization;
using Content.Shared._WL.NanoChat;

namespace Content.Shared._WL.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class NanoChatUiState(
        Dictionary<uint, NanoChatRecipient> recipients,
        Dictionary<uint, List<NanoChatMessage>> messages,
        Dictionary<uint, NanoChatGroup> groups,
        Dictionary<uint, List<NanoChatMessage>> groupMessages,
        HashSet<NanoChatConversationId> hasOlderMessages,
        HashSet<uint> blockedNumbers,
        List<NanoChatRecipient>? directory,
        uint? currentChat,
        uint? currentGroup,
        uint ownNumber,
        int maxRecipients,
        bool notificationsMuted,
        bool listNumber) : BoundUserInterfaceState
{
    /// <summary>
    ///     Own contacts (active chats), keyed by recipient number.
    /// </summary>
    public readonly Dictionary<uint, NanoChatRecipient> Recipients = recipients;

    /// <summary>
    ///     Own message history, keyed by recipient number.
    /// </summary>
    public readonly Dictionary<uint, List<NanoChatMessage>> Messages = messages;

    public readonly Dictionary<uint, NanoChatGroup> Groups = groups;
    public readonly Dictionary<uint, List<NanoChatMessage>> GroupMessages = groupMessages;
    public readonly HashSet<NanoChatConversationId> HasOlderMessages = hasOlderMessages;
    public readonly HashSet<uint> BlockedNumbers = blockedNumbers;

    /// <summary>
    ///     Directory search results (other listed NanoChat users on the same station). Null if not on a station.
    /// </summary>
    public readonly List<NanoChatRecipient>? Directory = directory;

    public readonly uint? CurrentChat = currentChat;
    public readonly uint? CurrentGroup = currentGroup;
    public readonly uint OwnNumber = ownNumber;
    public readonly int MaxRecipients = maxRecipients;
    public readonly bool NotificationsMuted = notificationsMuted;
    public readonly bool ListNumber = listNumber;
}
