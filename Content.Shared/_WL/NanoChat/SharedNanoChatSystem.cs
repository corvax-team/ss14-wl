using System.Linq;
using Content.Shared._WL.CartridgeLoader.Cartridges;
using Content.Shared.Examine;
using Robust.Shared.Timing;

namespace Content.Shared._WL.NanoChat;

/// <summary>
///     Base system for NanoChat functionality shared between client and server.
/// </summary>
public abstract partial class SharedNanoChatSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NanoChatCardComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<NanoChatCardComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (ent.Comp.Number is not { } number)
        {
            args.PushMarkup(Loc.GetString("nanochat-card-examine-no-number"));
            return;
        }

        args.PushMarkup(Loc.GetString("nanochat-card-examine-number", ("number", number.ToString("D4"))));
    }

    #region Public API

    public uint? GetNumber(Entity<NanoChatCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return null;

        return card.Comp.Number;
    }

    public void SetNumber(Entity<NanoChatCardComponent?> card, uint number)
    {
        if (!Resolve(card, ref card.Comp))
            return;

        card.Comp.Number = number;
        Dirty(card);
    }

    public IReadOnlyDictionary<uint, NanoChatRecipient> GetRecipients(Entity<NanoChatCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return new Dictionary<uint, NanoChatRecipient>();

        return card.Comp.Recipients;
    }

    public IReadOnlyDictionary<uint, IReadOnlyList<NanoChatMessage>> GetMessages(Entity<NanoChatCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return new Dictionary<uint, IReadOnlyList<NanoChatMessage>>();

        return card.Comp.Messages.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyList<NanoChatMessage>)pair.Value.ToArray());
    }

    public void SetRecipient(Entity<NanoChatCardComponent?> card, uint number, NanoChatRecipient recipient)
    {
        if (!Resolve(card, ref card.Comp))
            return;

        card.Comp.Recipients[number] = recipient;
        Dirty(card);
    }

    public NanoChatRecipient? GetRecipient(Entity<NanoChatCardComponent?> card, uint number)
    {
        if (!Resolve(card, ref card.Comp) || !card.Comp.Recipients.TryGetValue(number, out var recipient))
            return null;

        return recipient;
    }

    public List<NanoChatMessage>? GetMessagesForRecipient(Entity<NanoChatCardComponent?> card, uint recipientNumber)
    {
        if (!Resolve(card, ref card.Comp) || !card.Comp.Messages.TryGetValue(recipientNumber, out var messages))
            return null;

        return [.. messages];
    }

    public void AddMessage(Entity<NanoChatCardComponent?> card,
        uint recipientNumber,
        NanoChatMessage message,
        bool sentByOwner = false)
    {
        if (!Resolve(card, ref card.Comp))
            return;

        if (!card.Comp.Messages.TryGetValue(recipientNumber, out var messages))
        {
            messages = new List<NanoChatMessage>();
            card.Comp.Messages[recipientNumber] = messages;
        }

        messages.Add(message);
        TrimHistory(messages, card.Comp.MaxMessagesPerConversation);

        if (sentByOwner)
            card.Comp.LastMessageTime = _timing.CurTime;

        Dirty(card);
    }

    public uint? GetCurrentChat(Entity<NanoChatCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return null;

        return card.Comp.CurrentChat;
    }

    public void SetCurrentChat(Entity<NanoChatCardComponent?> card, uint? recipient)
    {
        if (!Resolve(card, ref card.Comp))
            return;

        card.Comp.CurrentChat = recipient;
        if (recipient != null)
            card.Comp.CurrentGroup = null;
        Dirty(card);
    }

    public void SetCurrentGroup(Entity<NanoChatCardComponent?> card, uint? groupId)
    {
        if (!Resolve(card, ref card.Comp))
            return;

        card.Comp.CurrentGroup = groupId;
        if (groupId != null)
            card.Comp.CurrentChat = null;
        Dirty(card);
    }

    public void SetGroup(Entity<NanoChatCardComponent?> card, NanoChatGroup group)
    {
        if (!Resolve(card, ref card.Comp))
            return;

        card.Comp.Groups[group.Id] = group;
        card.Comp.GroupMessages.TryAdd(group.Id, new List<NanoChatMessage>());
        Dirty(card);
    }

    public bool TryGetGroup(Entity<NanoChatCardComponent?> card, uint groupId, out NanoChatGroup group)
    {
        group = default;
        return Resolve(card, ref card.Comp) && card.Comp.Groups.TryGetValue(groupId, out group);
    }

    public HashSet<uint> GetBlockedNumbers(Entity<NanoChatCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return new HashSet<uint>();

        return new HashSet<uint>(card.Comp.BlockedNumbers);
    }

    public void AddGroupMessage(Entity<NanoChatCardComponent?> card,
        uint groupId,
        NanoChatMessage message,
        bool sentByOwner = false)
    {
        if (!Resolve(card, ref card.Comp) || !card.Comp.Groups.ContainsKey(groupId))
            return;

        if (!card.Comp.GroupMessages.TryGetValue(groupId, out var messages))
        {
            messages = new List<NanoChatMessage>();
            card.Comp.GroupMessages[groupId] = messages;
        }

        messages.Add(message);
        TrimHistory(messages, card.Comp.MaxMessagesPerConversation);
        if (sentByOwner)
            card.Comp.LastMessageTime = _timing.CurTime;
        Dirty(card);
    }

    public bool TryUseGroupManagementCooldown(Entity<NanoChatCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return false;

        if (card.Comp.LastGroupManagementTime != TimeSpan.Zero &&
            _timing.CurTime - card.Comp.LastGroupManagementTime < card.Comp.GroupManagementDelay)
            return false;

        card.Comp.LastGroupManagementTime = _timing.CurTime;
        Dirty(card);
        return true;
    }

    public static void TrimHistory(List<NanoChatMessage> messages, int maximum)
    {
        var removeCount = messages.Count - Math.Max(0, maximum);
        if (removeCount > 0)
            messages.RemoveRange(0, removeCount);
    }

    public bool RemoveGroup(Entity<NanoChatCardComponent?> card, uint groupId)
    {
        if (!Resolve(card, ref card.Comp))
            return false;

        var removed = card.Comp.Groups.Remove(groupId);
        removed |= card.Comp.GroupMessages.Remove(groupId);
        if (card.Comp.CurrentGroup == groupId)
        {
            card.Comp.CurrentGroup = null;
            removed = true;
        }

        if (removed)
            Dirty(card);
        return removed;
    }

    public bool IsBlocked(Entity<NanoChatCardComponent?> card, uint number)
        => Resolve(card, ref card.Comp) && card.Comp.BlockedNumbers.Contains(number);

    public void SetBlocked(Entity<NanoChatCardComponent?> card, uint number, bool blocked)
    {
        if (!Resolve(card, ref card.Comp) || card.Comp.Number == number)
            return;

        if (blocked)
            card.Comp.BlockedNumbers.Add(number);
        else
            card.Comp.BlockedNumbers.Remove(number);
        Dirty(card);
    }

    public bool GetNotificationsMuted(Entity<NanoChatCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return false;

        return card.Comp.NotificationsMuted;
    }

    public void SetNotificationsMuted(Entity<NanoChatCardComponent?> card, bool muted)
    {
        if (!Resolve(card, ref card.Comp))
            return;

        card.Comp.NotificationsMuted = muted;
        Dirty(card);
    }

    public bool GetListNumber(Entity<NanoChatCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return false;

        return card.Comp.ListNumber;
    }

    public void SetListNumber(Entity<NanoChatCardComponent?> card, bool listNumber)
    {
        if (!Resolve(card, ref card.Comp) || card.Comp.ListNumber == listNumber)
            return;

        card.Comp.ListNumber = listNumber;
        Dirty(card);
    }

    public TimeSpan? GetLastMessageTime(Entity<NanoChatCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return null;

        return card.Comp.LastMessageTime;
    }

    public bool HasUnreadMessages(Entity<NanoChatCardComponent?> card, uint recipientNumber)
    {
        if (!Resolve(card, ref card.Comp) || !card.Comp.Recipients.TryGetValue(recipientNumber, out var recipient))
            return false;

        return recipient.HasUnread;
    }

    public void Clear(Entity<NanoChatCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return;

        card.Comp.Messages.Clear();
        card.Comp.Recipients.Clear();
        card.Comp.Groups.Clear();
        card.Comp.GroupMessages.Clear();
        card.Comp.CurrentChat = null;
        card.Comp.CurrentGroup = null;
        Dirty(card);
    }

    /// <summary>
    ///     Deletes all card-local state associated with a chat conversation.
    /// </summary>
    public bool TryDeleteChat(Entity<NanoChatCardComponent?> card, uint recipientNumber)
    {
        if (!Resolve(card, ref card.Comp))
            return false;

        var removedRecipient = card.Comp.Recipients.Remove(recipientNumber);
        var removedMessages = card.Comp.Messages.Remove(recipientNumber);
        var clearedCurrent = false;

        if (card.Comp.CurrentChat == recipientNumber)
        {
            card.Comp.CurrentChat = null;
            clearedCurrent = true;
        }

        if (removedRecipient || removedMessages || clearedCurrent)
            Dirty(card);

        return removedRecipient || removedMessages || clearedCurrent;
    }

    /// <summary>
    ///     Ensures a recipient exists in the card's contacts and message lists.
    /// </summary>
    /// <returns>True if the recipient was added or already existed</returns>
    public bool EnsureRecipientExists(Entity<NanoChatCardComponent?> card,
        uint recipientNumber,
        NanoChatRecipient? recipientInfo = null)
    {
        if (!Resolve(card, ref card.Comp))
            return false;

        if (!card.Comp.Recipients.ContainsKey(recipientNumber))
        {
            if (recipientInfo == null)
                return false;

            if (card.Comp.Recipients.Count >= card.Comp.MaxRecipients)
                return false;

            card.Comp.Recipients[recipientNumber] = recipientInfo.Value;
        }

        if (!card.Comp.Messages.ContainsKey(recipientNumber))
            card.Comp.Messages[recipientNumber] = new List<NanoChatMessage>();

        Dirty(card);
        return true;
    }

    #endregion
}
