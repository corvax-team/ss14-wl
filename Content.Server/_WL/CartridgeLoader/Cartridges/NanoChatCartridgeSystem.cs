using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Radio;
using Content.Server.Radio.EntitySystems;
using Content.Server.Station.Systems;
using Content.Server.GameTicking;
using Content.Server._WL.NanoChat;
using Content.Shared.Access.Components;
using Content.Shared.CartridgeLoader;
using Content.Shared.Database;
using Content.Shared.Station.Components;
using Content.Shared._WL.CartridgeLoader.Cartridges;
using Content.Shared._WL.NanoChat;
using Content.Shared.PDA;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._WL.CartridgeLoader.Cartridges;

public sealed partial class NanoChatCartridgeSystem : EntitySystem
{
    [Dependency] private CartridgeLoaderSystem _cartridge = default!;
    [Dependency] private IAdminLogManager _adminLogger = default!;
    [Dependency] private GameTicker _gameTicker = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private SharedNanoChatSystem _nanoChat = default!;
    [Dependency] private NanoChatSystem _nanoChatServer = default!;
    [Dependency] private RadioSystem _radio = default!;
    [Dependency] private StationSystem _station = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    // Messages get cut off in notification previews after this point.
    private const int NotificationMaxLength = 64;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NanoChatCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<NanoChatCartridgeComponent, CartridgeMessageEvent>(OnMessage);

    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Keep the cached card reference on the cartridge in sync with whatever ID card is
        // currently inserted into the parent PDA, refreshing the UI whenever it changes.
        var query = EntityQueryEnumerator<NanoChatCartridgeComponent, CartridgeComponent>();
        while (query.MoveNext(out var uid, out var nanoChat, out var cartridge))
        {
            if (cartridge.LoaderUid is not { } loader || !TryComp<PdaComponent>(loader, out var pda))
                continue;

            var newCard = pda.ContainedId;
            if (newCard == nanoChat.Card)
                continue;

            nanoChat.Card = newCard;
            nanoChat.LoadedMessageCounts.Clear();
            UpdateUi((uid, nanoChat), loader);
        }
    }

    private void OnMessage(Entity<NanoChatCartridgeComponent> ent, ref CartridgeMessageEvent args)
    {
        if (args is not NanoChatUiMessageEvent msg)
            return;

        var loaderUid = GetEntity(args.LoaderUid);

        if (!GetCardEntity(loaderUid, out var card))
            return;

        switch (msg.Type)
        {
            case NanoChatUiMessageType.NewChat:
                HandleNewChat(card, msg);
                break;
            case NanoChatUiMessageType.ToggleMute:
                HandleToggleMute(card);
                break;
            case NanoChatUiMessageType.DeleteChat:
                HandleDeleteChat(card, msg);
                break;
            case NanoChatUiMessageType.SendMessage:
                if (msg.Conversation is { Type: NanoChatConversationType.Group })
                    HandleSendGroupMessage(ent, card, loaderUid, msg);
                else
                    HandleSendMessage(ent, card, loaderUid, msg);
                break;
            case NanoChatUiMessageType.ToggleListNumber:
                HandleToggleListNumber(card);
                break;
            case NanoChatUiMessageType.LoadOlderMessages:
                HandleLoadOlder(ent.Comp, card.Comp, msg);
                break;
            case NanoChatUiMessageType.BlockContact:
                HandleBlockContact(card, msg, true);
                break;
            case NanoChatUiMessageType.UnblockContact:
                HandleBlockContact(card, msg, false);
                break;
            case NanoChatUiMessageType.CreateGroup:
                HandleCreateGroup(card, msg);
                break;
            case NanoChatUiMessageType.SelectConversation:
                HandleSelectConversation(card, msg);
                break;
            case NanoChatUiMessageType.RenameGroup:
                HandleRenameGroup(card, msg);
                break;
            case NanoChatUiMessageType.AddGroupMember:
                HandleAddGroupMember(card, msg);
                break;
            case NanoChatUiMessageType.RemoveGroupMember:
                HandleRemoveGroupMember(card, msg);
                break;
            case NanoChatUiMessageType.SetGroupAdmin:
                HandleSetGroupAdmin(card, msg);
                break;
            case NanoChatUiMessageType.LeaveGroup:
                HandleLeaveGroup(card, msg);
                break;
            case NanoChatUiMessageType.DeleteGroup:
                HandleDeleteGroup(card, msg);
                break;
            case NanoChatUiMessageType.ToggleGroupMute:
                HandleToggleGroupMute(card, msg);
                break;
        }

        UpdateUi(ent, loaderUid);
    }

    private bool GetCardEntity(EntityUid loaderUid, out Entity<NanoChatCardComponent> card)
    {
        card = default;

        if (!TryComp<PdaComponent>(loaderUid, out var pda) ||
            pda.ContainedId is not { } idCardUid ||
            !TryComp<NanoChatCardComponent>(idCardUid, out var idCard))
            return false;

        card = (idCardUid, idCard);
        return true;
    }

    private void HandleNewChat(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg)
    {
        if (msg.RecipientNumber == null ||
            msg.RecipientNumber == card.Comp.Number)
            return;

        if (GetCardInfo(msg.RecipientNumber.Value) is not { } recipient)
        {
            _popup.PopupEntity(
                Loc.GetString("nanochat-contact-not-found", ("number", msg.RecipientNumber.Value.ToString("D4"))),
                msg.Actor,
                msg.Actor);
            return;
        }

        if (!_nanoChat.EnsureRecipientExists((card, card.Comp), msg.RecipientNumber.Value, recipient))
        {
            _popup.PopupEntity(Loc.GetString("nanochat-contact-limit-reached"), msg.Actor, msg.Actor);
            return;
        }

        _nanoChat.SetCurrentChat((card, card.Comp), msg.RecipientNumber);

        _adminLogger.Add(LogType.Action,
            LogImpact.Low,
            $"{ToPrettyString(msg.Actor):user} started a NanoChat conversation with #{msg.RecipientNumber:D4} ({recipient.Name})");

        var recipientEv = new NanoChatRecipientUpdatedEvent(card);
        RaiseLocalEvent(ref recipientEv);
    }

    private static void HandleLoadOlder(
        NanoChatCartridgeComponent cartridge,
        NanoChatCardComponent card,
        NanoChatUiMessageEvent msg)
    {
        if (msg.Conversation is not { } conversation)
            return;

        var historyCount = conversation.Type == NanoChatConversationType.Direct
            ? card.Messages.GetValueOrDefault(conversation.Id)?.Count
            : card.GroupMessages.GetValueOrDefault(conversation.Id)?.Count;
        if (historyCount is not { } count)
            return;

        var loaded = cartridge.LoadedMessageCounts.GetValueOrDefault(
            conversation,
            NanoChatCartridgeComponent.MessagePageSize);
        cartridge.LoadedMessageCounts[conversation] = Math.Min(count, loaded + NanoChatCartridgeComponent.MessagePageSize);
    }

    private void HandleBlockContact(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg, bool blocked)
    {
        if (msg.TargetNumber is not { } target || target == card.Comp.Number)
            return;

        _nanoChat.SetBlocked((card, card.Comp), target, blocked);
    }

    private void HandleSelectConversation(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg)
    {
        if (msg.Conversation is not { } conversation)
            return;

        if (conversation.Type == NanoChatConversationType.Direct)
        {
            if (_nanoChat.GetRecipient((card, card.Comp), conversation.Id) is not { } recipient)
                return;

            _nanoChat.SetCurrentChat((card, card.Comp), conversation.Id);
            _nanoChat.SetRecipient((card, card.Comp), conversation.Id, recipient with { HasUnread = false });
            return;
        }

        if (!_nanoChat.TryGetGroup((card, card.Comp), conversation.Id, out var group))
            return;

        group.HasUnread = false;
        _nanoChat.SetGroup((card, card.Comp), group);
        _nanoChat.SetCurrentGroup((card, card.Comp), conversation.Id);
    }

    private void HandleDeleteChat(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg)
    {
        if (msg.RecipientNumber == null)
            return;

        if (!_nanoChat.TryDeleteChat((card, card.Comp), msg.RecipientNumber.Value))
            return;

        _adminLogger.Add(LogType.Action,
            LogImpact.Low,
            $"{ToPrettyString(msg.Actor):user} deleted NanoChat conversation with #{msg.RecipientNumber:D4}");
    }

    private void HandleToggleMute(Entity<NanoChatCardComponent> card)
    {
        _nanoChat.SetNotificationsMuted((card, card.Comp), !_nanoChat.GetNotificationsMuted((card, card.Comp)));
    }

    private void HandleToggleListNumber(Entity<NanoChatCardComponent> card)
    {
        _nanoChat.SetListNumber((card, card.Comp), !_nanoChat.GetListNumber((card, card.Comp)));
        UpdateAllUis();
    }

    private void HandleSendMessage(Entity<NanoChatCartridgeComponent> cartridge,
        Entity<NanoChatCardComponent> card,
        EntityUid loader,
        NanoChatUiMessageEvent msg)
    {
        var recipientNumber = msg.Conversation is { Type: NanoChatConversationType.Direct } conversation
            ? conversation.Id
            : msg.RecipientNumber;
        if (recipientNumber == null ||
            msg.Content == null ||
            card.Comp.Number == null ||
            recipientNumber == card.Comp.Number)
            return;

        if (!TryPrepareMessage(card.Comp, msg.Content, out var content, out var plainContent))
            return;

        if (_nanoChat.IsBlocked((card, card.Comp), recipientNumber.Value))
            return;

        if (!_nanoChat.EnsureRecipientExists((card, card.Comp), recipientNumber.Value, GetCardInfo(recipientNumber.Value)))
            return;

        var message = new NanoChatMessage(_gameTicker.RoundDuration(), content, card.Comp.Number.Value);
        var candidates = AttemptMessageDelivery(cartridge, loader, recipientNumber.Value);
        var deliveredRecipients = new List<Entity<NanoChatCardComponent>>();

        foreach (var recipient in candidates)
        {
            if (DeliverMessageToRecipient(card, recipient, message))
                deliveredRecipients.Add(recipient);
        }

        var deliveryFailed = deliveredRecipients.Count == 0;
        message = message with { DeliveryFailed = deliveryFailed };

        _nanoChat.AddMessage((card, card.Comp), recipientNumber.Value, message, sentByOwner: true);

        var recipientsText = deliveredRecipients.Count > 0
            ? string.Join(", ", deliveredRecipients.Select(r => ToPrettyString(r.Owner)))
            : $"#{recipientNumber:D4}";

        _adminLogger.Add(LogType.Chat,
            LogImpact.Low,
            $"{ToPrettyString(msg.Actor):user} sent NanoChat message to {recipientsText}: {plainContent}{(deliveryFailed ? " [DELIVERY FAILED]" : "")}");

        var msgEv = new NanoChatMessageReceivedEvent(card);
        RaiseLocalEvent(ref msgEv);
    }

    private bool TryPrepareMessage(
        NanoChatCardComponent card,
        string rawContent,
        out string content,
        out string plainContent)
    {
        content = rawContent.Trim();
        plainContent = FormattedMessage.RemoveMarkupPermissive(content).Trim();

        return rawContent.Length <= NanoChatMessage.MaxMarkupLength &&
               plainContent.Length is > 0 and <= NanoChatMessage.MaxLength &&
               (card.LastMessageTime == TimeSpan.Zero ||
                _timing.CurTime - card.LastMessageTime >= card.MessageSendDelay);
    }

    /// <summary>
    ///     Attempts to deliver a message: checks the sender's radio channel, finds all cards with the
    ///     matching number, and filters them down to those reachable via an active telecomms server.
    /// </summary>
    private List<Entity<NanoChatCardComponent>> AttemptMessageDelivery(
        Entity<NanoChatCartridgeComponent> sender,
        EntityUid senderLoader,
        uint recipientNumber)
    {
        var candidates = AttemptMessageDelivery(sender, senderLoader, new HashSet<uint> { recipientNumber });
        return candidates.GetValueOrDefault(recipientNumber) ?? new List<Entity<NanoChatCardComponent>>();
    }

    /// <summary>
    ///     Resolves multiple NanoChat numbers in one card scan. Sender-side radio checks are shared,
    ///     while station, channel and receive events are still evaluated for every destination PDA.
    /// </summary>
    private Dictionary<uint, List<Entity<NanoChatCardComponent>>> AttemptMessageDelivery(
        Entity<NanoChatCartridgeComponent> sender,
        EntityUid senderLoader,
        IReadOnlySet<uint> recipientNumbers)
    {
        var deliverable = new Dictionary<uint, List<Entity<NanoChatCardComponent>>>();
        if (recipientNumbers.Count == 0)
            return deliverable;

        var channel = _prototype.Index(sender.Comp.RadioChannel);
        var sendAttemptEvent = new RadioSendAttemptEvent(channel, sender);
        RaiseLocalEvent(ref sendAttemptEvent);
        RaiseLocalEvent(sender.Owner, ref sendAttemptEvent);
        if (sendAttemptEvent.Cancelled)
            return deliverable;

        var senderStation = GetCommunicationStation(senderLoader);
        var senderMap = Transform(senderLoader).MapID;
        if (senderStation == null || !_radio.HasActiveServer(senderMap, sender.Comp.RadioChannel))
            return deliverable;

        var activeServerByMap = new Dictionary<MapId, bool>();
        var cardQuery = EntityQueryEnumerator<NanoChatCardComponent>();
        while (cardQuery.MoveNext(out var cardUid, out var card))
        {
            if (card.Number is not { } recipientNumber ||
                !recipientNumbers.Contains(recipientNumber) ||
                !HasAccessibleNanoChat((cardUid, card), out var recipientPda))
                continue;

            var recipientStation = GetCommunicationStation(recipientPda);
            if (recipientStation == null)
                continue;

            if (!channel.LongRange && recipientStation != senderStation)
                continue;

            var recipientMap = Transform(recipientPda).MapID;
            if (!activeServerByMap.TryGetValue(recipientMap, out var hasActiveServer))
            {
                hasActiveServer = _radio.HasActiveServer(recipientMap, sender.Comp.RadioChannel);
                activeServerByMap[recipientMap] = hasActiveServer;
            }

            if (!hasActiveServer)
                continue;

            var receiveAttemptEv = new RadioReceiveAttemptEvent(channel, sender, recipientPda);
            RaiseLocalEvent(ref receiveAttemptEv);
            RaiseLocalEvent(recipientPda, ref receiveAttemptEv);
            if (receiveAttemptEv.Cancelled)
                continue;

            if (!deliverable.TryGetValue(recipientNumber, out var cards))
            {
                cards = new List<Entity<NanoChatCardComponent>>();
                deliverable[recipientNumber] = cards;
            }

            cards.Add((cardUid, card));
        }

        return deliverable;
    }

    private bool DeliverMessageToRecipient(Entity<NanoChatCardComponent> sender,
        Entity<NanoChatCardComponent> recipient,
        NanoChatMessage message)
    {
        if (sender.Comp.Number is not { } senderNumber)
            return false;

        if (_nanoChat.IsBlocked((recipient, recipient.Comp), senderNumber))
            return false;

        if (!_nanoChat.EnsureRecipientExists((recipient, recipient.Comp), senderNumber, GetCardInfo(senderNumber)))
            return false;

        _nanoChat.AddMessage((recipient, recipient.Comp), senderNumber, message with { DeliveryFailed = false });

        if (!IsChatVisible(recipient, senderNumber))
            HandleUnreadNotification(recipient, message, senderNumber);

        var msgEv = new NanoChatMessageReceivedEvent(recipient);
        RaiseLocalEvent(ref msgEv);
        UpdateUiForCard(recipient);
        return true;
    }

    private void HandleUnreadNotification(Entity<NanoChatCardComponent> recipient, NanoChatMessage message, uint senderNumber)
    {
        if (_nanoChat.GetRecipient((recipient, recipient.Comp), senderNumber) is { } senderRecipient)
            _nanoChat.SetRecipient((recipient, recipient.Comp), senderNumber, senderRecipient with { HasUnread = true });

        var senderName = _nanoChat.GetRecipient((recipient, recipient.Comp), senderNumber)?.Name ?? $"#{senderNumber:D4}";

        if (recipient.Comp.NotificationsMuted ||
            recipient.Comp.PdaUid is not { } pdaUid ||
            !TryComp<CartridgeLoaderComponent>(pdaUid, out var loader))
            return;

        _cartridge.SendNotification(pdaUid,
            Loc.GetString("nanochat-notification-title", ("sender", FormattedMessage.EscapeText(senderName))),
            TruncateMessage(FormattedMessage.RemoveMarkupPermissive(message.Content)),
            loader);
    }

    private bool IsChatVisible(Entity<NanoChatCardComponent> recipient, uint senderNumber)
    {
        if (_nanoChat.GetCurrentChat((recipient, recipient.Comp)) != senderNumber ||
            recipient.Comp.PdaUid is not { } pdaUid ||
            !TryComp<CartridgeLoaderComponent>(pdaUid, out var loader) ||
            loader.ActiveProgram is not { } activeProgram ||
            !HasComp<NanoChatCartridgeComponent>(activeProgram))
            return false;

        return _ui.IsUiOpen(pdaUid, PdaUiKey.Key);
    }

    private bool IsGroupVisible(Entity<NanoChatCardComponent> recipient, uint groupId)
    {
        if (recipient.Comp.CurrentGroup != groupId ||
            recipient.Comp.PdaUid is not { } pdaUid ||
            !TryComp<CartridgeLoaderComponent>(pdaUid, out var loader) ||
            loader.ActiveProgram is not { } activeProgram ||
            !HasComp<NanoChatCartridgeComponent>(activeProgram))
            return false;

        return _ui.IsUiOpen(pdaUid, PdaUiKey.Key);
    }

    private void UpdateUiForCard(EntityUid cardUid)
    {
        var query = EntityQueryEnumerator<NanoChatCartridgeComponent, CartridgeComponent>();
        while (query.MoveNext(out var uid, out var comp, out var cartridge))
        {
            if (comp.Card != cardUid || cartridge.LoaderUid is not { } loader)
                continue;

            UpdateUi((uid, comp), loader);
        }
    }

    private void UpdateAllUis()
    {
        var query = EntityQueryEnumerator<NanoChatCartridgeComponent, CartridgeComponent>();
        while (query.MoveNext(out var uid, out var comp, out var cartridge))
        {
            if (cartridge.LoaderUid is { } loader)
                UpdateUi((uid, comp), loader);
        }
    }

    /// <summary>
    ///     Builds directory info for a NanoChat number by finding its card and reading the linked ID card's name/job.
    /// </summary>
    private NanoChatRecipient? GetCardInfo(uint number)
    {
        var query = EntityQueryEnumerator<NanoChatCardComponent>();
        while (query.MoveNext(out var uid, out var card))
        {
            if (card.Number != number || !HasAccessibleNanoChat((uid, card), out _))
                continue;

            string? jobTitle = null;
            var name = Loc.GetString("nanochat-unknown-contact");
            if (TryComp<IdCardComponent>(uid, out var idCard))
            {
                jobTitle = idCard.LocalizedJobTitle;
                name = idCard.FullName ?? name;
                return new NanoChatRecipient(number, name, jobTitle, jobIcon: idCard.JobIcon);
            }

            return new NanoChatRecipient(number, name, jobTitle);
        }

        return null;
    }

    private bool HasAccessibleNanoChat(Entity<NanoChatCardComponent> card, out EntityUid pdaUid)
    {
        pdaUid = default;

        if (card.Comp.PdaUid is not { } pda ||
            !TryComp<CartridgeLoaderComponent>(pda, out var loader))
            return false;

        if (!_cartridge.HasProgram<NanoChatCartridgeComponent>((pda, loader)))
            return false;

        pdaUid = pda;
        return true;
    }

    private static string TruncateMessage(string message)
    {
        return message.Length <= NotificationMaxLength
            ? message
            : message[..(NotificationMaxLength - 4)] + " [...]";
    }

    private void OnUiReady(Entity<NanoChatCartridgeComponent> ent, ref CartridgeUiReadyEvent args)
    {
        UpdateUi(ent, args.Loader);
    }

    private void UpdateUi(Entity<NanoChatCartridgeComponent> ent, EntityUid loader)
    {
        NanoChatCardComponent? ownCard = null;
        if (ent.Comp.Card is { } ownCardUid)
            TryComp(ownCardUid, out ownCard);

        var ownNumber = ownCard?.Number ?? 0;
        List<NanoChatRecipient>? directory;
        if (GetCommunicationStation(loader) is { } station &&
            ownCard is { CanAccessStationDirectory: true })
        {
            ent.Comp.Station = station;

            directory = new List<NanoChatRecipient>();
            var query = AllEntityQuery<NanoChatCardComponent, IdCardComponent>();
            while (query.MoveNext(out var cardUid, out var nanoChatCard, out var idCard))
            {
                if (!HasAccessibleNanoChat((cardUid, nanoChatCard), out var recipientPda))
                    continue;

                if (!nanoChatCard.ListNumber ||
                    nanoChatCard.Number is not { } number ||
                    number == ownNumber ||
                    ownCard != null && _nanoChat.IsBlocked((ent.Comp.Card!.Value, ownCard), number) ||
                    idCard.FullName is not { } fullName ||
                    GetCommunicationStation(recipientPda) != station)
                    continue;

                directory.Add(new NanoChatRecipient(number, fullName, idCard.LocalizedJobTitle, jobIcon: idCard.JobIcon));
            }

            directory.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
        }
        else
        {
            directory = null;
        }

        var recipients = new Dictionary<uint, NanoChatRecipient>();
        var messages = new Dictionary<uint, List<NanoChatMessage>>();
        var groups = new Dictionary<uint, NanoChatGroup>();
        var groupMessages = new Dictionary<uint, List<NanoChatMessage>>();
        var hasOlderMessages = new HashSet<NanoChatConversationId>();
        uint? currentChat = null;
        uint? currentGroup = null;
        var maxRecipients = 50;
        var notificationsMuted = false;
        var listNumber = true;

        if (ent.Comp.Card != null && TryComp<NanoChatCardComponent>(ent.Comp.Card, out var card))
        {
            recipients = card.Recipients;
            messages = PageMessages(card.Messages,
                NanoChatConversationType.Direct,
                ent.Comp.LoadedMessageCounts,
                hasOlderMessages);
            groups = card.Groups;
            groupMessages = PageMessages(card.GroupMessages,
                NanoChatConversationType.Group,
                ent.Comp.LoadedMessageCounts,
                hasOlderMessages);
            currentChat = card.CurrentChat;
            currentGroup = card.CurrentGroup;
            ownNumber = card.Number ?? 0;
            maxRecipients = card.MaxRecipients;
            notificationsMuted = card.NotificationsMuted;
            listNumber = card.ListNumber;
        }

        var state = new NanoChatUiState(
            recipients,
            messages,
            groups,
            groupMessages,
            hasOlderMessages,
            ent.Comp.Card != null && TryComp<NanoChatCardComponent>(ent.Comp.Card, out var blockedCard)
                ? _nanoChat.GetBlockedNumbers((ent.Comp.Card.Value, blockedCard))
                : new HashSet<uint>(),
            directory,
            currentChat,
            currentGroup,
            ownNumber,
            maxRecipients,
            notificationsMuted,
            listNumber);
        _cartridge.UpdateCartridgeUiState(loader, state);
    }

    internal static Dictionary<uint, List<NanoChatMessage>> PageMessages(
        Dictionary<uint, List<NanoChatMessage>> source,
        NanoChatConversationType type,
        Dictionary<NanoChatConversationId, int> requestedCounts,
        HashSet<NanoChatConversationId> hasOlder)
    {
        var result = new Dictionary<uint, List<NanoChatMessage>>(source.Count);
        foreach (var (id, history) in source)
        {
            var conversation = new NanoChatConversationId(type, id);
            // Keep an initial page for every conversation so selecting a chat can render its
            // final layout immediately, without briefly showing only the preview message.
            var requested = requestedCounts.GetValueOrDefault(
                conversation,
                NanoChatCartridgeComponent.MessagePageSize);
            var skip = Math.Max(0, history.Count - requested);
            if (skip > 0)
                hasOlder.Add(conversation);
            result[id] = history.GetRange(skip, history.Count - skip);
        }

        return result;
    }

    /// <summary>
    ///     Gets the station whose local NanoChat network an entity may use. Grids that are not station
    ///     members can use the network while they are on a map belonging to exactly one station.
    /// </summary>
    private EntityUid? GetCommunicationStation(EntityUid entity)
    {
        if (_station.GetOwningStation(entity) is { } station)
            return station;

        var mapId = Transform(entity).MapID;
        EntityUid? mapStation = null;
        var query = EntityQueryEnumerator<StationDataComponent>();
        while (query.MoveNext(out var stationUid, out _))
        {
            if (_station.GetLargestGrid(stationUid) is not { } stationGrid ||
                Transform(stationGrid).MapID != mapId)
                continue;

            // An unowned grid on a multi-station map cannot be assigned safely.
            if (mapStation != null)
                return null;

            mapStation = stationUid;
        }

        return mapStation;
    }
}
