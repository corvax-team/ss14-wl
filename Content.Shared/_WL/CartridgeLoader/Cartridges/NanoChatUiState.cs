using Robust.Shared.Serialization;
using Content.Shared._WL.NanoChat;

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

    public readonly Dictionary<uint, NanoChatGroup> Groups;
    public readonly Dictionary<uint, List<NanoChatMessage>> GroupMessages;
    public readonly HashSet<NanoChatConversationId> HasOlderMessages;
    public readonly HashSet<uint> BlockedNumbers;

    /// <summary>
    ///     Directory search results (other listed NanoChat users on the same station). Null if not on a station.
    /// </summary>
    public readonly List<NanoChatRecipient>? Directory;

    public readonly uint? CurrentChat;
    public readonly uint? CurrentGroup;
    public readonly uint OwnNumber;
    public readonly int MaxRecipients;
    public readonly bool NotificationsMuted;
    public readonly bool ListNumber;

    public NanoChatUiState(
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
        bool listNumber)
    {
        Recipients = recipients;
        Messages = messages;
        Groups = groups;
        GroupMessages = groupMessages;
        HasOlderMessages = hasOlderMessages;
        BlockedNumbers = blockedNumbers;
        Directory = directory;
        CurrentChat = currentChat;
        CurrentGroup = currentGroup;
        OwnNumber = ownNumber;
        MaxRecipients = maxRecipients;
        NotificationsMuted = notificationsMuted;
        ListNumber = listNumber;
    }
}
