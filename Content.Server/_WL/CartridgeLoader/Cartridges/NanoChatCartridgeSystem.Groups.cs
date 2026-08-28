using System.Linq;
using Content.Server._WL.NanoChat;
using Content.Shared.CartridgeLoader;
using Content.Shared.Database;
using Content.Shared._WL.CartridgeLoader.Cartridges;
using Content.Shared._WL.NanoChat;
using Robust.Shared.Utility;

namespace Content.Server._WL.CartridgeLoader.Cartridges;

public sealed partial class NanoChatCartridgeSystem
{
    private void HandleCreateGroup(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg)
    {
        if (card.Comp.Number is not { } creatorNumber ||
            msg.Content is null ||
            msg.Content.Length > NanoChatGroup.MaxNameMarkupLength ||
            msg.MemberNumbers is null ||
            msg.MemberNumbers.Count > NanoChatGroup.MaxMembers - 1)
            return;

        if (!_nanoChat.TryUseGroupManagementCooldown((card, card.Comp)))
            return;

        var name = FormattedMessage.RemoveMarkupPermissive(msg.Content).Trim();
        if (name.Length is 0 or > NanoChatGroup.MaxNameLength || GetGroupMember(creatorNumber) is not { } creator)
            return;

        var invited = new List<NanoChatGroupMember>();
        foreach (var number in msg.MemberNumbers.Distinct())
        {
            if (number == creatorNumber ||
                _nanoChat.IsBlocked((card, card.Comp), number) ||
                GetGroupMember(number) is not { } member ||
                IsBlockedByAnyCard(number, creatorNumber))
                continue;

            invited.Add(member);
        }

        if (!_nanoChatServer.TryCreateGroup(name, creator, invited, out var group))
            return;

        SyncGroupToMembers(group);
        _nanoChat.SetCurrentGroup((card, card.Comp), group.Id);
    }

    private void HandleRenameGroup(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg)
    {
        if (card.Comp.Number is not { } actor ||
            msg.Conversation is not { Type: NanoChatConversationType.Group } conversation ||
            msg.Content is null ||
            msg.Content.Length > NanoChatGroup.MaxNameMarkupLength ||
            !_nanoChat.TryUseGroupManagementCooldown((card, card.Comp)))
            return;

        var name = FormattedMessage.RemoveMarkupPermissive(msg.Content).Trim();
        if (name.Length is 0 or > NanoChatGroup.MaxNameLength ||
            !_nanoChatServer.TryRenameGroup(conversation.Id, actor, name))
            return;

        if (_nanoChatServer.TryGetGroup(conversation.Id, out var group))
            SyncGroupToMembers(group);
    }

    private void HandleAddGroupMember(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg)
    {
        if (card.Comp.Number is not { } actor ||
            msg.Conversation is not { Type: NanoChatConversationType.Group } conversation ||
            msg.TargetNumber is not { } target ||
            !_nanoChat.TryUseGroupManagementCooldown((card, card.Comp)))
            return;

        if (_nanoChat.IsBlocked((card, card.Comp), target) ||
            GetGroupMember(target) is not { } member ||
            IsBlockedByAnyCard(target, actor))
        {
            _popup.PopupEntity(
                Loc.GetString("nanochat-contact-not-found", ("number", target.ToString("D4"))),
                msg.Actor,
                msg.Actor);
            return;
        }

        if (!_nanoChatServer.TryAddGroupMember(conversation.Id, actor, member))
            return;

        if (_nanoChatServer.TryGetGroup(conversation.Id, out var group))
            SyncGroupToMembers(group);
    }

    private void HandleRemoveGroupMember(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg)
    {
        if (card.Comp.Number is not { } actor ||
            msg.Conversation is not { Type: NanoChatConversationType.Group } conversation ||
            msg.TargetNumber is not { } target ||
            !_nanoChat.TryUseGroupManagementCooldown((card, card.Comp)) ||
            !_nanoChatServer.TryRemoveGroupMember(conversation.Id, actor, target))
            return;

        RemoveGroupFromNumber(target, conversation.Id);
        if (_nanoChatServer.TryGetGroup(conversation.Id, out var group))
            SyncGroupToMembers(group);
    }

    private void HandleSetGroupAdmin(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg)
    {
        if (card.Comp.Number is not { } actor ||
            msg.Conversation is not { Type: NanoChatConversationType.Group } conversation ||
            msg.TargetNumber is not { } target ||
            msg.Value is not { } admin ||
            !_nanoChat.TryUseGroupManagementCooldown((card, card.Comp)) ||
            !_nanoChatServer.TrySetGroupAdmin(conversation.Id, actor, target, admin))
            return;

        if (_nanoChatServer.TryGetGroup(conversation.Id, out var group))
            SyncGroupToMembers(group);
    }

    private void HandleLeaveGroup(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg)
    {
        if (card.Comp.Number is not { } actor ||
            msg.Conversation is not { Type: NanoChatConversationType.Group } conversation ||
            !_nanoChat.TryUseGroupManagementCooldown((card, card.Comp)) ||
            !_nanoChatServer.TryLeaveGroup(conversation.Id, actor, out _))
            return;

        RemoveGroupFromNumber(actor, conversation.Id);
        if (_nanoChatServer.TryGetGroup(conversation.Id, out var group))
            SyncGroupToMembers(group);
    }

    private void HandleDeleteGroup(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg)
    {
        if (card.Comp.Number is not { } actor ||
            msg.Conversation is not { Type: NanoChatConversationType.Group } conversation ||
            !_nanoChatServer.TryGetGroup(conversation.Id, out var group) ||
            !_nanoChat.TryUseGroupManagementCooldown((card, card.Comp)) ||
            !_nanoChatServer.TryDeleteGroup(conversation.Id, actor))
            return;

        foreach (var number in group.Members.Keys)
            RemoveGroupFromNumber(number, conversation.Id);
    }

    private void HandleToggleGroupMute(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg)
    {
        if (msg.Conversation is not { Type: NanoChatConversationType.Group } conversation ||
            !_nanoChat.TryGetGroup((card, card.Comp), conversation.Id, out var group))
            return;

        group.NotificationsMuted = !group.NotificationsMuted;
        _nanoChat.SetGroup((card, card.Comp), group);
    }

    private void HandleSendGroupMessage(
        Entity<NanoChatCartridgeComponent> cartridge,
        Entity<NanoChatCardComponent> card,
        EntityUid loader,
        NanoChatUiMessageEvent msg)
    {
        if (card.Comp.Number is not { } senderNumber ||
            msg.Conversation is not { Type: NanoChatConversationType.Group } conversation ||
            msg.Content is null ||
            !_nanoChatServer.TryGetGroup(conversation.Id, out var group) ||
            !group.Members.ContainsKey(senderNumber))
            return;

        if (!TryPrepareMessage(card.Comp, msg.Content, out var content, out var plainContent))
            return;

        var intended = group.Members.Count - 1;
        var baseMessage = new NanoChatMessage(_gameTicker.RoundDuration(), content, senderNumber);
        var deliveredNumbers = new HashSet<uint>();
        var recipientNumbers = group.Members.Keys
            .Where(number => number != senderNumber)
            .ToHashSet();
        var candidatesByNumber = AttemptMessageDelivery(cartridge, loader, recipientNumbers);

        foreach (var (recipientNumber, candidates) in candidatesByNumber)
        {
            foreach (var recipient in candidates)
            {
                if (DeliverGroupMessage(recipient, group, baseMessage))
                    deliveredNumbers.Add(recipientNumber);
            }
        }

        var message = baseMessage with
        {
            DeliveryFailed = deliveredNumbers.Count == 0,
            DeliveredRecipients = (byte)deliveredNumbers.Count,
            IntendedRecipients = (byte)intended,
        };

        var localGroup = _nanoChat.TryGetGroup((card, card.Comp), group.Id, out var existingGroup)
            ? group.Snapshot(existingGroup.HasUnread, existingGroup.NotificationsMuted)
            : group.Snapshot();
        _nanoChat.SetGroup((card, card.Comp), localGroup);
        _nanoChat.AddGroupMessage((card, card.Comp), group.Id, message, sentByOwner: true);

        _adminLogger.Add(LogType.Chat,
            LogImpact.Low,
            $"{ToPrettyString(msg.Actor):user} sent NanoChat group message to '{group.Name}' " +
            $"({deliveredNumbers.Count}/{intended} recipients): {plainContent}");

        var msgEv = new NanoChatMessageReceivedEvent(card);
        RaiseLocalEvent(ref msgEv);
    }

    private bool DeliverGroupMessage(
        Entity<NanoChatCardComponent> recipient,
        NanoChatServerGroup serverGroup,
        NanoChatMessage message)
    {
        if (_nanoChat.IsBlocked((recipient, recipient.Comp), message.Sender))
            return false;

        if (!_nanoChat.TryGetGroup((recipient, recipient.Comp), serverGroup.Id, out var localGroup))
            localGroup = serverGroup.Snapshot();
        else
            localGroup = serverGroup.Snapshot(localGroup.HasUnread, localGroup.NotificationsMuted);

        _nanoChat.SetGroup((recipient, recipient.Comp), localGroup);
        _nanoChat.AddGroupMessage((recipient, recipient.Comp), serverGroup.Id, message);

        if (!IsGroupVisible(recipient, serverGroup.Id))
        {
            localGroup.HasUnread = true;
            _nanoChat.SetGroup((recipient, recipient.Comp), localGroup);
            HandleGroupNotification(recipient, serverGroup, message);
        }

        var msgEv = new NanoChatMessageReceivedEvent(recipient);
        RaiseLocalEvent(ref msgEv);
        UpdateUiForCard(recipient);
        return true;
    }

    private void HandleGroupNotification(
        Entity<NanoChatCardComponent> recipient,
        NanoChatServerGroup group,
        NanoChatMessage message)
    {
        if (recipient.Comp.NotificationsMuted ||
            _nanoChat.TryGetGroup((recipient, recipient.Comp), group.Id, out var localGroup) &&
             localGroup.NotificationsMuted ||
            recipient.Comp.PdaUid is not { } pdaUid ||
            !TryComp<CartridgeLoaderComponent>(pdaUid, out var loader))
            return;

        var sender = group.Members.TryGetValue(message.Sender, out var member)
            ? member.Name
            : $"#{message.Sender:D4}";
        _cartridge.SendNotification(
            pdaUid,
            $"{FormattedMessage.EscapeText(group.Name)} · {FormattedMessage.EscapeText(sender)}",
            TruncateMessage(FormattedMessage.RemoveMarkupPermissive(message.Content)),
            loader);
    }

    private NanoChatGroupMember? GetGroupMember(uint number)
    {
        if (GetCardInfo(number) is not { } recipient)
            return null;

        return new NanoChatGroupMember(
            recipient.Number,
            recipient.Name,
            recipient.JobTitle,
            recipient.JobIcon);
    }

    private bool IsBlockedByAnyCard(uint number, uint blockedNumber)
    {
        var query = EntityQueryEnumerator<NanoChatCardComponent>();
        while (query.MoveNext(out var uid, out var card))
        {
            if (card.Number == number && _nanoChat.IsBlocked((uid, card), blockedNumber))
                return true;
        }

        return false;
    }

    private void SyncGroupToMembers(NanoChatServerGroup group)
    {
        var query = EntityQueryEnumerator<NanoChatCardComponent>();
        while (query.MoveNext(out var uid, out var card))
        {
            if (card.Number is not { } number || !group.Members.ContainsKey(number))
                continue;

            var hasLocalGroup = _nanoChat.TryGetGroup((uid, card), group.Id, out var local);
            var hasUnread = hasLocalGroup && local.HasUnread;
            var muted = hasLocalGroup && local.NotificationsMuted;
            _nanoChat.SetGroup((uid, card), group.Snapshot(hasUnread, muted));
            UpdateUiForCard(uid);
        }
    }

    private void RemoveGroupFromNumber(uint number, uint groupId)
    {
        var query = EntityQueryEnumerator<NanoChatCardComponent>();
        while (query.MoveNext(out var uid, out var card))
        {
            if (card.Number != number || !_nanoChat.RemoveGroup((uid, card), groupId))
                continue;

            UpdateUiForCard(uid);
        }
    }
}
