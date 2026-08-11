using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Radio;
using Content.Server.Radio.EntitySystems;
using Content.Server.Station.Systems;
using Content.Shared.Access.Components;
using Content.Shared.CartridgeLoader;
using Content.Shared.Database;
using Content.Shared._WL.CartridgeLoader.Cartridges;
using Content.Shared._WL.NanoChat;
using Content.Shared.PDA;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._WL.CartridgeLoader.Cartridges;

public sealed partial class NanoChatCartridgeSystem : EntitySystem
{
    [Dependency] private CartridgeLoaderSystem _cartridge = default!;
    [Dependency] private IAdminLogManager _adminLogger = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private SharedNanoChatSystem _nanoChat = default!;
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
            case NanoChatUiMessageType.SelectChat:
                HandleSelectChat(card, msg);
                break;
            case NanoChatUiMessageType.ToggleMute:
                HandleToggleMute(card);
                break;
            case NanoChatUiMessageType.DeleteChat:
                HandleDeleteChat(card, msg);
                break;
            case NanoChatUiMessageType.SendMessage:
                HandleSendMessage(ent, card, loaderUid, msg);
                break;
            case NanoChatUiMessageType.ToggleListNumber:
                HandleToggleListNumber(card);
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
        if (msg.RecipientNumber == null || msg.Content == null || msg.RecipientNumber == card.Comp.Number)
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

    private void HandleSelectChat(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg)
    {
        if (msg.RecipientNumber == null || msg.RecipientNumber == card.Comp.Number)
            return;

        if (_nanoChat.GetRecipient((card, card.Comp), msg.RecipientNumber.Value) == null)
            return;

        _nanoChat.SetCurrentChat((card, card.Comp), msg.RecipientNumber);

        if (_nanoChat.GetRecipient((card, card.Comp), msg.RecipientNumber.Value) is { } recipient)
            _nanoChat.SetRecipient((card, card.Comp), msg.RecipientNumber.Value, recipient with { HasUnread = false });
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
        if (msg.RecipientNumber == null || msg.Content == null || card.Comp.Number == null ||
            msg.RecipientNumber == card.Comp.Number)
            return;

        if (string.IsNullOrWhiteSpace(msg.Content))
            return;

        var content = msg.Content.Trim();
        if (content.Length > NanoChatMessage.MaxLength)
            return;

        if (card.Comp.LastMessageTime != TimeSpan.Zero &&
            _timing.CurTime - card.Comp.LastMessageTime < card.Comp.MessageSendDelay)
            return;

        if (!_nanoChat.EnsureRecipientExists((card, card.Comp), msg.RecipientNumber.Value, GetCardInfo(msg.RecipientNumber.Value)))
            return;

        var message = new NanoChatMessage(_timing.CurTime, content, card.Comp.Number.Value);
        var candidates = AttemptMessageDelivery(cartridge, loader, msg.RecipientNumber.Value);
        var deliveredRecipients = new List<Entity<NanoChatCardComponent>>();

        foreach (var recipient in candidates)
        {
            if (DeliverMessageToRecipient(card, recipient, message))
                deliveredRecipients.Add(recipient);
        }

        var deliveryFailed = deliveredRecipients.Count == 0;
        message = message with { DeliveryFailed = deliveryFailed };

        _nanoChat.AddMessage((card, card.Comp), msg.RecipientNumber.Value, message, sentByOwner: true);

        var recipientsText = deliveredRecipients.Count > 0
            ? string.Join(", ", deliveredRecipients.Select(r => ToPrettyString(r.Owner)))
            : $"#{msg.RecipientNumber:D4}";

        _adminLogger.Add(LogType.Chat,
            LogImpact.Low,
            $"{ToPrettyString(msg.Actor):user} sent NanoChat message to {recipientsText}: {content}{(deliveryFailed ? " [DELIVERY FAILED]" : "")}");

        var msgEv = new NanoChatMessageReceivedEvent(card);
        RaiseLocalEvent(ref msgEv);
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
        var channel = _prototype.Index(sender.Comp.RadioChannel);
        var sendAttemptEvent = new RadioSendAttemptEvent(channel, sender);
        RaiseLocalEvent(ref sendAttemptEvent);
        RaiseLocalEvent(sender.Owner, ref sendAttemptEvent);
        if (sendAttemptEvent.Cancelled)
            return new List<Entity<NanoChatCardComponent>>();

        var senderStation = _station.GetOwningStation(senderLoader);
        var senderMap = Transform(senderLoader).MapID;

        var foundRecipients = new List<Entity<NanoChatCardComponent>>();
        var cardQuery = EntityQueryEnumerator<NanoChatCardComponent>();
        while (cardQuery.MoveNext(out var cardUid, out var card))
        {
            if (card.Number != recipientNumber || !HasAccessibleNanoChat((cardUid, card), out _))
                continue;

            foundRecipients.Add((cardUid, card));
        }

        if (foundRecipients.Count == 0)
            return foundRecipients;

        var deliverable = new List<Entity<NanoChatCardComponent>>();
        foreach (var recipient in foundRecipients)
        {
            if (!HasAccessibleNanoChat(recipient, out var recipientPda))
                continue;

            var recipientStation = _station.GetOwningStation(recipientPda);

            if (senderStation == null || recipientStation == null)
                continue;

            if (!channel.LongRange && recipientStation != senderStation)
                continue;

            if (!_radio.HasActiveServer(senderMap, sender.Comp.RadioChannel) ||
                !_radio.HasActiveServer(Transform(recipientPda).MapID, sender.Comp.RadioChannel))
                continue;

            var receiveAttemptEv = new RadioReceiveAttemptEvent(channel, sender, recipientPda);
            RaiseLocalEvent(ref receiveAttemptEv);
            RaiseLocalEvent(recipientPda, ref receiveAttemptEv);
            if (receiveAttemptEv.Cancelled)
                continue;

            deliverable.Add(recipient);
        }

        return deliverable;
    }

    private bool DeliverMessageToRecipient(Entity<NanoChatCardComponent> sender,
        Entity<NanoChatCardComponent> recipient,
        NanoChatMessage message)
    {
        if (sender.Comp.Number is not { } senderNumber)
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
            TruncateMessage(message.Content),
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
            }

            return new NanoChatRecipient(number, name, jobTitle);
        }

        return null;
    }

    private bool HasAccessibleNanoChat(Entity<NanoChatCardComponent> card, out EntityUid pdaUid)
    {
        pdaUid = default;

        if (card.Comp.PdaUid is not { } pda || !TryComp<CartridgeLoaderComponent>(pda, out var loader))
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
        if (_station.GetOwningStation(loader) is { } station && ownCard is { CanAccessStationDirectory: true })
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
                    idCard.FullName is not { } fullName ||
                    _station.GetOwningStation(recipientPda) != station)
                    continue;

                directory.Add(new NanoChatRecipient(number, fullName, idCard.LocalizedJobTitle));
            }

            directory.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
        }
        else
        {
            directory = null;
        }

        var recipients = new Dictionary<uint, NanoChatRecipient>();
        var messages = new Dictionary<uint, List<NanoChatMessage>>();
        uint? currentChat = null;
        var maxRecipients = 50;
        var notificationsMuted = false;
        var listNumber = true;

        if (ent.Comp.Card != null && TryComp<NanoChatCardComponent>(ent.Comp.Card, out var card))
        {
            recipients = card.Recipients;
            messages = card.Messages;
            currentChat = card.CurrentChat;
            ownNumber = card.Number ?? 0;
            maxRecipients = card.MaxRecipients;
            notificationsMuted = card.NotificationsMuted;
            listNumber = card.ListNumber;
        }

        var state = new NanoChatUiState(recipients, messages, directory, currentChat, ownNumber, maxRecipients, notificationsMuted, listNumber);
        _cartridge.UpdateCartridgeUiState(loader, state);
    }
}
