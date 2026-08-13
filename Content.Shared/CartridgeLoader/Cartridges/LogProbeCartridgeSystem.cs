//WL-Changes-NanoChat-Start
using System.Linq;
using Content.Shared._WL.CartridgeLoader.Cartridges;
using Content.Shared._WL.NanoChat;
using Robust.Shared.Network;
//WL-Changes-NanoChat-End
using Content.Shared.Access.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Labels.EntitySystems;
using Content.Shared.Paper;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Text;

namespace Content.Shared.CartridgeLoader.Cartridges;

public sealed partial class LogProbeCartridgeSystem : EntitySystem
{
    //WL-Changes-LogProbe-Formatting-Start
    private const string PrintAccentColor = "#445F78";
    private const string PrintDividerColor = "#77777D";
    //WL-Changes-LogProbe-Formatting-End

    [Dependency] private CartridgeLoaderSystem _cartridge = default!;
    //WL-Changes-NanoChat-NetworkGuard
    [Dependency] private INetManager _net = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private LabelSystem _label = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private PaperSystem _paper = default!;
    //WL-Changes-NanoChat-Dependency
    [Dependency] private SharedNanoChatSystem _nanoChat = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LogProbeCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<LogProbeCartridgeComponent, CartridgeRelayedEvent<AfterInteractEvent>>(AfterInteract);
        SubscribeLocalEvent<LogProbeCartridgeComponent, CartridgeMessageEvent>(OnMessage);
    }

    /// <summary>
    /// Updates the program's list of logs with those from the device.
    /// </summary>
    private void AfterInteract(Entity<LogProbeCartridgeComponent> ent, ref CartridgeRelayedEvent<AfterInteractEvent> args)
    {
        if (!_net.IsServer || args.Args.Handled || !args.Args.CanReach || args.Args.Target is not { } target)
            return;

        var hasAccessReader = TryComp(target, out AccessReaderComponent? accessReaderComponent);
        var hasNanoChat = TryComp(target, out NanoChatCardComponent? nanoChatCard);

        if (!hasAccessReader && !hasNanoChat)
            return;

        //Play scanning sound with slightly randomized pitch
        _audio.PlayPredicted(ent.Comp.SoundScan, target, args.Args.User);
        _popup.PopupCursor(Loc.GetString("log-probe-scan", ("device", target)), args.Args.User);

        ent.Comp.EntityName = Name(target);
        ent.Comp.PulledAccessLogs.Clear();

        if (accessReaderComponent != null)
        {
            foreach (var accessRecord in accessReaderComponent.AccessLog)
            {
                var log = new PulledAccessLog(
                    accessRecord.AccessTime,
                    accessRecord.Accessor
                );

                ent.Comp.PulledAccessLogs.Add(log);
            }
        }

        // Reverse the list so the oldest is at the bottom
        ent.Comp.PulledAccessLogs.Reverse();

        //WL-Changes-NanoChat-Start
        ent.Comp.NanoChat = nanoChatCard != null
            ? NanoChatData.FromCard(nanoChatCard, GetNetEntity(target))
            : null;
        //WL-Changes-NanoChat-End

        Dirty(ent);
        UpdateUiState(ent, args.Loader);
    }

    /// <summary>
    /// This gets called when the ui fragment needs to be updated for the first time after activating
    /// </summary>
    private void OnUiReady(Entity<LogProbeCartridgeComponent> ent, ref CartridgeUiReadyEvent args)
    {
        UpdateUiState(ent, args.Loader);
    }

    private void OnMessage(Entity<LogProbeCartridgeComponent> ent, ref CartridgeMessageEvent args)
    {
        if (args is LogProbePrintMessage cast)
            PrintLogs(ent, cast.User);
    }

    private void PrintLogs(Entity<LogProbeCartridgeComponent> ent, EntityUid user)
    {
        if (string.IsNullOrEmpty(ent.Comp.EntityName))
            return;

        if (_timing.CurTime < ent.Comp.NextPrintAllowed)
            return;

        ent.Comp.NextPrintAllowed = _timing.CurTime + ent.Comp.PrintCooldown;

        var paper = EntityManager.PredictedSpawn(ent.Comp.PaperPrototype, _transform.GetMapCoordinates(user));
        _label.Label(paper, ent.Comp.EntityName); // label it for easy identification

        _audio.PlayEntity(ent.Comp.PrintSound, user, paper);
        _hands.PickupOrDrop(user, paper, checkActionBlocker: false);

        //WL-Changes-LogProbe-Formatting-Start
        // Generate the actual printout text.
        var builder = new StringBuilder();
        builder.Append("[head=3][color=").Append(PrintAccentColor).Append(']')
            .Append(FormattedMessage.EscapeText(Loc.GetString("log-probe-printout-title")))
            .AppendLine("[/color][/head]");
        builder.AppendLine(Loc.GetString("log-probe-printout-device-formatted",
            ("name", FormattedMessage.EscapeText(ent.Comp.EntityName))));
        builder.Append("[color=").Append(PrintDividerColor)
            .AppendLine("]────────────────────────[/color]");

        //WL-Changes-NanoChat-Start
        if (ent.Comp.NanoChat is { } nanoChat)
            AppendNanoChatPrintout(builder, nanoChat);
        //WL-Changes-NanoChat-End

        if (ent.Comp.NanoChat == null || ent.Comp.PulledAccessLogs.Count > 0)
        {
            AppendPrintSectionHeading(builder, Loc.GetString("log-probe-section-access-logs"));
            var number = ent.Comp.PulledAccessLogs.Count;
            foreach (var log in ent.Comp.PulledAccessLogs)
            {
                var time = TimeSpan.FromSeconds(Math.Truncate(log.Time.TotalSeconds)).ToString();
                builder.AppendLine(Loc.GetString("log-probe-printout-entry-formatted",
                    ("number", number),
                    ("time", time),
                    ("accessor", FormattedMessage.EscapeText(log.Accessor))));
                number--;
            }

            if (ent.Comp.PulledAccessLogs.Count == 0)
                builder.AppendLine(Loc.GetString("log-probe-printout-no-access-logs"));
        }

        var paperComp = Comp<PaperComponent>(paper);
        _paper.SetContent((paper, paperComp), builder.ToString());
        //WL-Changes-LogProbe-Formatting-End

        _adminLogger.Add(LogType.EntitySpawn, LogImpact.Low, $"{ToPrettyString(user):user} printed out LogProbe logs ({paper}) of {ent.Comp.EntityName}");
        Dirty(ent);
    }

    private void UpdateUiState(Entity<LogProbeCartridgeComponent> ent, EntityUid loaderUid)
    {
        //WL-Changes-NanoChat-NetworkGuard-Start
        if (!_net.IsServer)
            return;
        //WL-Changes-NanoChat-NetworkGuard-End

        //WL-Changes-NanoChat-Start
        NanoChatData? nanoChatPreview = ent.Comp.NanoChat is { } nanoChat
            ? NanoChatLogProbePreview.Create(
                nanoChat,
                ent.Comp.NanoChatUiMessageLimit,
                ent.Comp.NanoChatUiMessagesPerConversation)
            : null;
        var state = new LogProbeUiState(ent.Comp.EntityName, ent.Comp.PulledAccessLogs, nanoChatPreview);
        //WL-Changes-NanoChat-End
        _cartridge.UpdateCartridgeUiState(loaderUid, state);
    }

    //WL-Changes-NanoChat-Start
    private void AppendNanoChatPrintout(StringBuilder builder, NanoChatData nanoChat)
    {
        AppendPrintSectionHeading(builder, Loc.GetString("log-probe-section-nanochat"));
        builder.AppendLine(nanoChat.CardNumber is { } cardNumber
            ? Loc.GetString("log-probe-nanochat-header-formatted", ("number", cardNumber.ToString("D4")))
            : Loc.GetString("log-probe-nanochat-header-no-number-formatted"));

        if (nanoChat.BlockedNumbers.Count > 0)
        {
            var blocked = string.Join(", ", nanoChat.BlockedNumbers.Order().Select(number => $"#{number:D4}"));
            builder.AppendLine(Loc.GetString("log-probe-nanochat-blocked-list", ("numbers", blocked)));
        }

        if (nanoChat.Recipients.Count > 0)
        {
            AppendPrintSectionHeading(builder, Loc.GetString("log-probe-section-direct-chats"));
            foreach (var (recipientNumber, recipient) in nanoChat.Recipients.OrderBy(pair => pair.Value.Name))
            {
                var messages = nanoChat.Messages.GetValueOrDefault(recipientNumber);
                var messageCount = nanoChat.MessageCounts.GetValueOrDefault(recipientNumber, messages?.Count ?? 0);
                var blocked = nanoChat.BlockedNumbers.Contains(recipientNumber)
                    ? Loc.GetString("log-probe-nanochat-blocked-suffix")
                    : string.Empty;
                builder.AppendLine(Loc.GetString("log-probe-nanochat-contact-formatted",
                    ("name", FormattedMessage.EscapeText(recipient.Name)),
                    ("number", recipientNumber.ToString("D4")),
                    ("count", messageCount),
                    ("blocked", blocked)));

                if (messages == null)
                    continue;

                foreach (var message in messages)
                {
                    var direction = message.Sender == nanoChat.CardNumber
                        ? Loc.GetString("log-probe-nanochat-direction-outgoing")
                        : Loc.GetString("log-probe-nanochat-direction-incoming");
                    builder.AppendLine(Loc.GetString("log-probe-nanochat-message-formatted",
                        ("time", message.Timestamp.ToString(@"hh\:mm")),
                        ("direction", direction),
                        ("message", FormattedMessage.EscapeText(FormattedMessage.RemoveMarkupPermissive(message.Content)))));
                }
            }
        }

        if (nanoChat.Groups.Count == 0)
        {
            if (nanoChat.Recipients.Count == 0)
                builder.AppendLine(Loc.GetString("log-probe-printout-no-conversations"));
            return;
        }

        AppendPrintSectionHeading(builder, Loc.GetString("log-probe-section-group-chats"));
        foreach (var (groupId, group) in nanoChat.Groups.OrderBy(pair => pair.Value.Name))
        {
            var messages = nanoChat.GroupMessages.GetValueOrDefault(groupId);
            var messageCount = nanoChat.GroupMessageCounts.GetValueOrDefault(groupId, messages?.Count ?? 0);
            builder.AppendLine(Loc.GetString("log-probe-nanochat-group-formatted",
                ("name", FormattedMessage.EscapeText(group.Name)),
                ("count", messageCount)));

            if (messages == null)
                continue;

            foreach (var message in messages)
            {
                var sender = group.Members.TryGetValue(message.Sender, out var member)
                    ? member.Name
                    : $"#{message.Sender:D4}";
                builder.AppendLine(Loc.GetString("log-probe-nanochat-group-message-formatted",
                    ("time", message.Timestamp.ToString(@"hh\:mm")),
                    ("sender", FormattedMessage.EscapeText(sender)),
                    ("message", FormattedMessage.EscapeText(FormattedMessage.RemoveMarkupPermissive(message.Content)))));
            }
        }
    }

    private static void AppendPrintSectionHeading(StringBuilder builder, string title)
    {
        builder.AppendLine();
        builder.Append("[color=").Append(PrintAccentColor).Append("][bold]› ")
            .Append(FormattedMessage.EscapeText(title))
            .AppendLine("[/bold][/color]");
    }

    //WL-Changes-NanoChat-End
}
