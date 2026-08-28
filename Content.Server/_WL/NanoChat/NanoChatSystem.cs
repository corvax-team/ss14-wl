using System.Globalization;
using System.Linq;
using Content.Server.Access.Systems;
using Content.Server.Administration.Logs;
using Content.Server.Kitchen.Components;
using Content.Server.NameIdentifier;
using Content.Shared.Database;
using Content.Shared.GameTicking;
using Content.Shared._WL.CartridgeLoader.Cartridges;
using Content.Shared._WL.NanoChat;
using Content.Shared.Kitchen;
using Content.Shared.NameIdentifier;
using Content.Shared.PDA;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._WL.NanoChat;

/// <summary>
///     Handles NanoChat features that live on the ID card but aren't specific to the cartridge UI:
///     number assignment, tracking which PDA a card is inserted into, and microwave effects.
/// </summary>
public sealed partial class NanoChatSystem : SharedNanoChatSystem
{
    [Dependency] private IAdminLogManager _adminLogger = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private NameIdentifierSystem _name = default!;

    private readonly ProtoId<NameIdentifierGroupPrototype> _nameIdentifierGroup = "NanoChat";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NanoChatCardComponent, EntGotInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<NanoChatCardComponent, EntGotRemovedFromContainerMessage>(OnRemoved);

        SubscribeLocalEvent<NanoChatCardComponent, MapInitEvent>(OnCardInit);
        SubscribeLocalEvent<NanoChatCardComponent, BeingMicrowavedEvent>(OnMicrowaved, after: [typeof(IdCardSystem)]);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(_ => ResetGroups());
    }

    private void OnInserted(Entity<NanoChatCardComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != PdaComponent.PdaIdSlotId)
            return;

        ent.Comp.PdaUid = args.Container.Owner;
        Dirty(ent);
    }

    private void OnRemoved(Entity<NanoChatCardComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if (args.Container.ID != PdaComponent.PdaIdSlotId)
            return;

        ent.Comp.PdaUid = null;
        Dirty(ent);
    }

    private void OnCardInit(Entity<NanoChatCardComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.Number != null)
            return;

        _name.GenerateUniqueName(ent, _nameIdentifierGroup, out var number);
        ent.Comp.Number = (uint)number;
        Dirty(ent);
    }

    private void OnMicrowaved(Entity<NanoChatCardComponent> ent, ref BeingMicrowavedEvent args)
    {
        // Skip if the card was deleted (e.g. burned by the ID card system) by the time we run.
        if (Deleted(ent))
            return;

        if (!TryComp<MicrowaveComponent>(args.Microwave, out var micro) || micro.Broken)
            return;

        var randomPick = _random.NextFloat();

        if (randomPick <= 0.10f)
        {
            ent.Comp.Messages.Clear();
            ent.Comp.GroupMessages.Clear();

            _adminLogger.Add(LogType.Action,
                LogImpact.Medium,
                $"{ToPrettyString(args.Microwave)} erased all NanoChat messages on {ToPrettyString(ent)}");
        }
        else
        {
            ScrambleMessages(ent.Comp);

            _adminLogger.Add(LogType.Action,
                LogImpact.Medium,
                $"{ToPrettyString(args.Microwave)} scrambled NanoChat messages on {ToPrettyString(ent)}");
        }

        Dirty(ent);
    }

    private void ScrambleMessages(NanoChatCardComponent component)
    {
        foreach (var (recipientNumber, messages) in component.Messages.ToArray())
        {
            for (var i = 0; i < messages.Count; i++)
            {
                if (!_random.Prob(0.5f))
                    continue;

                var message = messages[i];
                message.Content = ScrambleText(message.Content);
                messages[i] = message;
            }

            if (_random.Prob(0.25f) && component.Recipients.Count > 0)
            {
                var newRecipient = _random.Pick(component.Recipients.Keys.ToList());
                if (newRecipient == recipientNumber)
                    continue;

                if (!component.Messages.ContainsKey(newRecipient))
                    component.Messages[newRecipient] = new List<NanoChatMessage>();

                var recipientMessages = component.Messages[newRecipient];
                recipientMessages.AddRange(messages);

                component.Messages[recipientNumber].Clear();
            }
        }

        // Group membership is server-authoritative, so microwaving a card may corrupt
        // the local text but must never move messages between unrelated groups.
        foreach (var messages in component.GroupMessages.Values)
            ScrambleMessageContents(messages);
    }

    private void ScrambleMessageContents(List<NanoChatMessage> messages)
    {
        for (var i = 0; i < messages.Count; i++)
        {
            if (!_random.Prob(0.5f))
                continue;

            var message = messages[i];
            message.Content = ScrambleText(message.Content);
            messages[i] = message;
        }
    }

    private string ScrambleText(string text)
    {
        var elements = new List<string>();
        var enumerator = StringInfo.GetTextElementEnumerator(text);
        while (enumerator.MoveNext())
            elements.Add((string)enumerator.Current);

        var n = elements.Count;

        while (n > 1)
        {
            n--;
            var k = _random.Next(n + 1);
            (elements[k], elements[n]) = (elements[n], elements[k]);
        }

        return string.Concat(elements);
    }
}
