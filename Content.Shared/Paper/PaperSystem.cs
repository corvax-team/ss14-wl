using System.Linq;
using Content.Shared.Administration.Logs;
using Content.Shared.Access.Systems; // WL-Changes: Structured paper signatures
using Content.Shared._WL.Paper; // WL-Changes: Structured paper forms
using Content.Shared.UserInterface;
using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Random.Helpers;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Hands.EntitySystems; // WL-Changes: Structured paper forms
using Robust.Shared.Player;
using Robust.Shared.Audio.Systems;
using static Content.Shared.Paper.PaperComponent;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared.Paper;

public sealed partial class PaperSystem : EntitySystem
{
    [Dependency] private ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedPopupSystem _popupSystem = default!;
    [Dependency] private TagSystem _tagSystem = default!;
    [Dependency] private SharedUserInterfaceSystem _uiSystem = default!;
    [Dependency] private MetaDataSystem _metaSystem = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedHandsSystem _hands = default!; // WL-Changes: Structured paper forms
    [Dependency] private SharedIdCardSystem _idCard = default!; // WL-Changes: Structured paper signatures

    [Dependency] private EntityQuery<PaperComponent> _paperQuery = default!;

    private static readonly ProtoId<TagPrototype> WriteIgnoreStampsTag = "WriteIgnoreStamps";
    private static readonly ProtoId<TagPrototype> WriteTag = "Write";

    // WL-Changes-StructuredPaper-Start
    private const int MaxStructuredElements = 128;
    private const int MaxElementIdLength = 64;
    private const int MaxFieldRevisions = 3;
    private const string SignatureEditorToken = "[sign]";
    private readonly Dictionary<(EntityUid Paper, EntityUid User), (PaperEditAccess Access, bool IgnoreStamps)> _editSessions = new();
    private readonly Dictionary<EntityUid, (EntityUid Editor, PaperEditAccess Access)> _displayedEditAccess = new();
    // WL-Changes-StructuredPaper-End


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PaperComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<PaperComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<PaperComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<PaperComponent, BeforeActivatableUIOpenEvent>(BeforeUIOpen);
        SubscribeLocalEvent<PaperComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<PaperComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<PaperComponent, PaperInputTextMessage>(OnInputTextMessage);
        // WL-Changes-StructuredPaper-Start
        SubscribeLocalEvent<PaperComponent, PaperInputFieldMessage>(OnInputFieldMessage);
        SubscribeLocalEvent<PaperComponent, PaperRequestFieldEditMessage>(OnRequestFieldEditMessage);
        SubscribeLocalEvent<PaperComponent, PaperSignFieldMessage>(OnSignFieldMessage);
        SubscribeLocalEvent<PaperComponent, PaperAppendTextMessage>(OnAppendTextMessage);
        SubscribeLocalEvent<PaperComponent, PaperAppendElementsMessage>(OnAppendElementsMessage);
        SubscribeLocalEvent<PaperComponent, PaperInputStructureMessage>(OnInputStructureMessage);
        SubscribeLocalEvent<PaperComponent, BoundUIClosedEvent>(OnUiClosed);
        SubscribeLocalEvent<StructuredPaperComponent, MapInitEvent>(OnStructuredPaperMapInit);
        // WL-Changes-StructuredPaper-End

        SubscribeLocalEvent<RandomPaperContentComponent, MapInitEvent>(OnRandomPaperContentMapInit);

        SubscribeLocalEvent<ActivateOnPaperOpenedComponent, PaperWriteEvent>(OnPaperWrite);
    }

    private void OnMapInit(Entity<PaperComponent> entity, ref MapInitEvent args)
    {
        if (!string.IsNullOrEmpty(entity.Comp.Content))
        {
            SetContent(entity, Loc.GetString(entity.Comp.Content));
        }
    }

    private void OnInit(Entity<PaperComponent> entity, ref ComponentInit args)
    {
        entity.Comp.Mode = PaperAction.Read;

        if (TryComp<AppearanceComponent>(entity, out var appearance))
        {
            if (entity.Comp.Content != "")
                _appearance.SetData(entity, PaperVisuals.Status, PaperStatus.Written, appearance);

            if (entity.Comp.StampState != null)
                _appearance.SetData(entity, PaperVisuals.Stamp, entity.Comp.StampState, appearance);
        }
    }

    // WL-Changes-StructuredPaper-Start
    private void OnShutdown(Entity<PaperComponent> entity, ref ComponentShutdown args)
    {
        ClearEditSessions(entity.Owner);
        _displayedEditAccess.Remove(entity.Owner);
    }
    // WL-Changes-StructuredPaper-End

    private void BeforeUIOpen(Entity<PaperComponent> entity, ref BeforeActivatableUIOpenEvent args)
    {
        // WL-Changes-StructuredPaper-Start
        // A reader opening the same paper must not replace another player's active editing view.
        if (!_displayedEditAccess.ContainsKey(entity.Owner))
            entity.Comp.Mode = PaperAction.Read;
        // WL-Changes-StructuredPaper-End
        UpdateUserInterface(entity);
    }

    private void OnExamined(Entity<PaperComponent> entity, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        using (args.PushGroup(nameof(PaperComponent)))
        {
            if (entity.Comp.Content != "")
            {
                args.PushMarkup(
                    Loc.GetString(
                        "paper-component-examine-detail-has-words",
                        ("paper", entity)
                    )
                );
            }

            if (entity.Comp.StampedBy.Count > 0)
            {
                var commaSeparated =
                    string.Join(", ", entity.Comp.StampedBy.Select(s => Loc.GetString(s.StampedName)));
                args.PushMarkup(
                    Loc.GetString(
                        "paper-component-examine-detail-stamped-by",
                        ("paper", entity),
                        ("stamps", commaSeparated))
                );
            }
        }
    }

    private void OnInteractUsing(Entity<PaperComponent> entity, ref InteractUsingEvent args)
    {
        // only allow editing if there are no stamps or when using a cyberpen
        var editable = entity.Comp.StampedBy.Count == 0 || _tagSystem.HasTag(args.Used, WriteIgnoreStampsTag);
        if (_tagSystem.HasTag(args.Used, WriteTag))
        {
            if (editable)
            {
                if (entity.Comp.EditingDisabled)
                {
                    var paperEditingDisabledMessage = Loc.GetString("paper-tamper-proof-modified-message");
                    _popupSystem.PopupEntity(paperEditingDisabledMessage, entity, args.User);

                    args.Handled = true;
                    return;
                }

                var ev = new PaperWriteAttemptEvent(entity.Owner);
                RaiseLocalEvent(args.User, ref ev);
                if (ev.Cancelled)
                {
                    if (ev.FailReason is not null)
                    {
                        var fileWriteMessage = Loc.GetString(ev.FailReason);
                        _popupSystem.PopupEntity(fileWriteMessage, entity.Owner, args.User);
                    }

                    args.Handled = true;
                    return;
                }

                var writeEvent = new PaperWriteEvent(args.User, entity);
                RaiseLocalEvent(args.Used, ref writeEvent);

                // WL-Changes-StructuredPaper-Start
                var access = _tagSystem.HasTag(args.Used, WriteIgnoreStampsTag)
                    ? PaperEditAccess.Full
                    : HasComp<StructuredPaperComponent>(entity)
                        ? PaperEditAccess.Fields
                        : PaperEditAccess.FreeText;
                ClearEditSessions(entity.Owner);
                _editSessions[(entity.Owner, args.User)] =
                    (access, _tagSystem.HasTag(args.Used, WriteIgnoreStampsTag));
                _displayedEditAccess[entity.Owner] = (args.User, access);
                // WL-Changes-StructuredPaper-End
                entity.Comp.Mode = PaperAction.Write;
                _uiSystem.OpenUi(entity.Owner, PaperUiKey.Key, args.User);
                UpdateUserInterface(entity);
            }
            args.Handled = true;
            return;
        }

        // If a stamp, attempt to stamp paper
        if (TryComp<StampComponent>(args.Used, out var stampComp) && TryStamp(entity, GetStampInfo(stampComp), stampComp.StampState))
        {
            // successfully stamped, play popup
            var stampPaperOtherMessage = Loc.GetString("paper-component-action-stamp-paper-other",
                    ("user", args.User),
                    ("target", args.Target),
                    ("stamp", args.Used));
            var stampPaperSelfMessage = Loc.GetString("paper-component-action-stamp-paper-self",
                    ("target", args.Target),
                    ("stamp", args.Used));
            _popupSystem.PopupEntity(stampPaperSelfMessage, stampPaperOtherMessage, args.User, args.User);

            _audio.PlayPredicted(stampComp.Sound, entity, args.User);

            UpdateUserInterface(entity);
        }
    }

    private static StampDisplayInfo GetStampInfo(StampComponent stamp)
    {
        return new StampDisplayInfo
        {
            StampedName = stamp.StampedName,
            StampedColor = stamp.StampedColor,

            // WL-Changes-start
            StampedTexture = stamp.StampTexture,
            StampedTextureIsBorder = stamp.IsBorderTexture
            // WL-Changes-end
        };
    }

    private void OnInputTextMessage(Entity<PaperComponent> entity, ref PaperInputTextMessage args)
    {
        // WL-Changes-StructuredPaper-Start
        if (!TryValidateEdit(entity, args.Actor, PaperEditAccess.FreeText, out var access) ||
            access is not (PaperEditAccess.FreeText or PaperEditAccess.Full) ||
            HasComp<StructuredPaperComponent>(entity))
            return;

        var submitted = args.Text;
        var result = submitted;
        if (access == PaperEditAccess.FreeText)
        {
            if (string.IsNullOrWhiteSpace(submitted))
                result = entity.Comp.Content;
            else if (string.IsNullOrEmpty(entity.Comp.Content))
                result = submitted;
            else
                result = entity.Comp.Content + (entity.Comp.Content.EndsWith('\n') ? string.Empty : "\n") + submitted;
        }
        // WL-Changes-StructuredPaper-End

        if (result.Length <= entity.Comp.ContentSize)
        {
            SetContent(entity, result);

            var paperStatus = string.IsNullOrWhiteSpace(result) ? PaperStatus.Blank : PaperStatus.Written;

            if (TryComp<AppearanceComponent>(entity, out var appearance))
                _appearance.SetData(entity, PaperVisuals.Status, paperStatus, appearance);

            if (TryComp(entity, out MetaDataComponent? meta))
                _metaSystem.SetEntityDescription(entity, "", meta);

            _adminLogger.Add(LogType.Chat,
                LogImpact.Low,
                $"{ToPrettyString(args.Actor):player} has written on {ToPrettyString(entity):entity} the following text: {submitted}");

            _audio.PlayPvs(entity.Comp.Sound, entity);
        }

        entity.Comp.Mode = PaperAction.Read;
        // WL-Changes-StructuredPaper-Start
        _editSessions.Remove((entity.Owner, args.Actor));
        if (_displayedEditAccess.TryGetValue(entity.Owner, out var displayed) && displayed.Editor == args.Actor)
            _displayedEditAccess.Remove(entity.Owner);
        // WL-Changes-StructuredPaper-End
        UpdateUserInterface(entity);
    }

    // WL-Changes-StructuredPaper-Start
    private void OnInputFieldMessage(Entity<PaperComponent> entity, ref PaperInputFieldMessage args)
    {
        if (!TryGetWritableField(entity, args.Actor, args.FieldId, out var structured, out var element))
            return;

        SetFieldText(entity, structured, element, args.Text, args.Actor, "filled");
    }

    private void OnSignFieldMessage(Entity<PaperComponent> entity, ref PaperSignFieldMessage args)
    {
        if (!TryGetWritableField(entity, args.Actor, args.FieldId, out var structured, out var element) ||
            element.Type != StructuredPaperElementType.Signature)
        {
            return;
        }

        var signature = GetSignature(args.Actor);
        if (string.IsNullOrWhiteSpace(signature))
            return;

        SetFieldText(entity, structured, element, signature, args.Actor, "signed");
    }

    private bool TryGetWritableField(
        Entity<PaperComponent> entity,
        EntityUid actor,
        string fieldId,
        out StructuredPaperComponent structured,
        out StructuredPaperElement element)
    {
        element = default!;
        if (!TryComp(entity, out structured!))
            return false;

        var found = structured.Elements.FirstOrDefault(candidate => candidate.Id == fieldId);
        if (found == null ||
            found.Type is not (StructuredPaperElementType.SingleLineField or
                StructuredPaperElementType.MultilineField or
                StructuredPaperElementType.SignatureField or
                StructuredPaperElementType.Signature))
        {
            return false;
        }

        if (!TryValidateEdit(entity, actor, PaperEditAccess.Fields, out var access))
        {
            if (!_hands.TryGetActiveItem(actor, out var pen) ||
                !_tagSystem.HasTag(pen.Value, WriteTag))
            {
                _popupSystem.PopupEntity(Loc.GetString("paper-component-write-field-no-pen"), entity, actor);
                return false;
            }

            var ignoreStamps = _tagSystem.HasTag(pen.Value, WriteIgnoreStampsTag);
            if (!TryBeginWriteSession(entity, actor, pen.Value, PaperEditAccess.Fields, ignoreStamps))
                return false;

            access = PaperEditAccess.Fields;
        }

        if (access is not (PaperEditAccess.Fields or PaperEditAccess.Full))
            return false;

        element = found;
        return true;
    }

    private bool SetFieldText(
        Entity<PaperComponent> entity,
        StructuredPaperComponent structured,
        StructuredPaperElement element,
        string text,
        EntityUid actor,
        string action)
    {
        var submitted = string.IsNullOrWhiteSpace(text) ? string.Empty : text;
        if (element.Type is (StructuredPaperElementType.SingleLineField or
                StructuredPaperElementType.SignatureField or
                StructuredPaperElementType.Signature) &&
            ContainsLineBreak(submitted))
        {
            return false;
        }

        if (submitted.Length > element.GetMaxLength())
            return false;

        if (string.Equals(element.Text, submitted, StringComparison.Ordinal))
            return false;

        var revisions = element.Revisions.Select(revision => revision.Copy()).ToList();
        AddLegacyRevision(element, revisions);
        if (!string.IsNullOrWhiteSpace(element.Text))
        {
            if (revisions.Count >= MaxFieldRevisions)
            {
                _popupSystem.PopupEntity(Loc.GetString("paper-component-field-too-many-corrections"), entity, actor);
                return false;
            }

            revisions.Add(new PaperFieldRevision(element.Text, element.HandwritingStyle));
        }

        var newLength = GetStructuredLength(structured.Elements)
            - element.Text.Length
            - GetRevisionLength(element)
            + submitted.Length
            + revisions.Sum(revision => revision.Text.Length);
        if (newLength > entity.Comp.ContentSize)
            return false;

        element.PreviousText = string.Empty;
        element.PreviousHandwritingStyle = PaperHandwritingStyle.Default;
        element.Revisions = revisions;
        element.Text = submitted;
        element.HandwritingStyle = TryComp<PaperHandwritingComponent>(actor, out var handwriting)
            ? handwriting.Style
            : PaperHandwritingStyle.Default;
        RefreshStructuredContent(entity, structured);
        LogStructuredEdit(actor, entity, $"{action} field {element.Id} with: {submitted}");
        _audio.PlayPvs(entity.Comp.Sound, entity);
        UpdateUserInterface(entity);
        return true;
    }

    private void OnAppendTextMessage(Entity<PaperComponent> entity, ref PaperAppendTextMessage args)
    {
        if (!TryComp<StructuredPaperComponent>(entity, out var structured) ||
            string.IsNullOrWhiteSpace(args.Text) ||
            args.Text.Length > StructuredPaperElement.DefaultMultilineMaxLength ||
            structured.Elements.Count >= MaxStructuredElements)
        {
            return;
        }

        if (!_hands.TryGetActiveItem(args.Actor, out var pen) ||
            !_tagSystem.HasTag(pen.Value, WriteTag))
        {
            _popupSystem.PopupEntity(Loc.GetString("paper-component-write-field-no-pen"), entity, args.Actor);
            return;
        }

        var ignoreStamps = _tagSystem.HasTag(pen.Value, WriteIgnoreStampsTag);
        if (!TryBeginWriteSession(entity, args.Actor, pen.Value, PaperEditAccess.Fields, ignoreStamps))
            return;

        var needsLeadingLineBreak = structured.Elements.Count > 0 && !structured.Elements[^1].NewLineAfter;
        if (GetStructuredLength(structured.Elements) + args.Text.Length + 1 + (needsLeadingLineBreak ? 1 : 0) >
            entity.Comp.ContentSize)
            return;

        if (needsLeadingLineBreak)
            structured.Elements[^1].NewLineAfter = true;

        var element = new StructuredPaperElement(
            $"note-{Guid.NewGuid():N}",
            StructuredPaperElementType.HandwrittenText,
            args.Text)
        {
            HandwritingStyle = TryComp<PaperHandwritingComponent>(args.Actor, out var handwriting)
                ? handwriting.Style
                : PaperHandwritingStyle.Default,
        };
        structured.Elements.Add(element);
        RefreshStructuredContent(entity, structured);
        LogStructuredEdit(args.Actor, entity, $"appended handwritten text {element.Id}: {args.Text}");
        _audio.PlayPvs(entity.Comp.Sound, entity);
        UpdateUserInterface(entity);
    }

    private string GetSignature(EntityUid actor)
    {
        var name = Name(actor);
        var job = string.Empty;
        if (_idCard.TryFindIdCard(actor, out var idCard))
        {
            if (!string.IsNullOrWhiteSpace(idCard.Comp.FullName))
                name = idCard.Comp.FullName;

            job = idCard.Comp.LocalizedJobTitle ?? string.Empty;
        }

        return string.IsNullOrWhiteSpace(job)
            ? name
            : Loc.GetString("paper-component-signature-with-job", ("name", name), ("job", job));
    }

    private void OnAppendElementsMessage(Entity<PaperComponent> entity, ref PaperAppendElementsMessage args)
    {
        var expandedElements = ExpandSignatureTokens(args.Elements);
        if (!TryValidateEdit(entity, args.Actor, PaperEditAccess.FreeText, out var access) ||
            access is not (PaperEditAccess.FreeText or PaperEditAccess.Fields or PaperEditAccess.Full) ||
            !TryNormalizeStructure(expandedElements, entity.Comp.ContentSize, out var appended))
        {
            return;
        }

        while (appended.Count > 0 &&
               appended[^1].Type is (StructuredPaperElementType.StaticText or StructuredPaperElementType.HandwrittenText) &&
               string.IsNullOrEmpty(appended[^1].Text))
        {
            appended.RemoveAt(appended.Count - 1);
        }

        if (appended.Count == 0)
            return;

        var existingCount = TryComp<StructuredPaperComponent>(entity, out var existingStructured)
            ? existingStructured.Elements.Count
            : string.IsNullOrEmpty(entity.Comp.Content) ? 0 : 1;
        if (existingCount + appended.Count > MaxStructuredElements)
            return;

        var legacyContent = existingStructured == null ? entity.Comp.Content : null;
        var existingLength = existingStructured != null
            ? GetStructuredLength(existingStructured.Elements)
            : legacyContent!.Length;
        var needsLeadingLineBreak = existingLength > 0 &&
            (existingStructured == null
                ? !legacyContent!.EndsWith('\n')
                : existingStructured.Elements.Count > 0 && !existingStructured.Elements[^1].NewLineAfter);
        var handwritingStyle = TryComp<PaperHandwritingComponent>(args.Actor, out var handwriting)
            ? handwriting.Style
            : PaperHandwritingStyle.Default;
        foreach (var element in appended)
        {
            switch (element.Type)
            {
                case StructuredPaperElementType.StaticText:
                case StructuredPaperElementType.HandwrittenText:
                    element.Type = StructuredPaperElementType.HandwrittenText;
                    break;
                case StructuredPaperElementType.SignatureField:
                    element.Type = StructuredPaperElementType.SingleLineField;
                    element.Text = string.Empty;
                    break;
                case StructuredPaperElementType.SingleLineField:
                case StructuredPaperElementType.MultilineField:
                case StructuredPaperElementType.Signature:
                    element.Text = string.Empty;
                    break;
            }

            element.HandwritingStyle = handwritingStyle;
        }

        var appendedLength = GetStructuredLength(appended);
        if (existingLength + appendedLength + (needsLeadingLineBreak ? 1 : 0) > entity.Comp.ContentSize)
            return;

        var usedIds = existingStructured?.Elements.Select(element => element.Id).ToHashSet()
            ?? new HashSet<string>();
        foreach (var element in appended)
        {
            element.Id = NewElementId(usedIds);
            element.LocId = null;
            element.PreviousText = string.Empty;
            element.PreviousHandwritingStyle = PaperHandwritingStyle.Default;
            element.Revisions.Clear();
        }

        var structured = existingStructured ?? EnsureComp<StructuredPaperComponent>(entity);
        if (existingStructured == null && !string.IsNullOrEmpty(legacyContent))
        {
            structured.Elements.Add(new StructuredPaperElement(
                NewElementId(usedIds),
                StructuredPaperElementType.StaticText,
                legacyContent,
                newLineAfter: !legacyContent.EndsWith('\n')));
        }
        else if (needsLeadingLineBreak && structured.Elements.Count > 0)
        {
            structured.Elements[^1].NewLineAfter = true;
        }

        structured.Elements.AddRange(appended);
        RefreshStructuredContent(entity, structured);
        LogStructuredEdit(args.Actor, entity, $"appended {appended.Count} document elements");
        _audio.PlayPvs(entity.Comp.Sound, entity);

        entity.Comp.Mode = PaperAction.Read;
        _editSessions.Remove((entity.Owner, args.Actor));
        if (_displayedEditAccess.TryGetValue(entity.Owner, out var displayed) && displayed.Editor == args.Actor)
            _displayedEditAccess.Remove(entity.Owner);
        UpdateUserInterface(entity);
    }

    private void OnRequestFieldEditMessage(Entity<PaperComponent> entity, ref PaperRequestFieldEditMessage args)
    {
        if (!TryGetWritableField(entity, args.Actor, args.FieldId, out _, out _))
            return;

        entity.Comp.Mode = PaperAction.Write;
        _uiSystem.OpenUi(entity.Owner, PaperUiKey.Key, args.Actor);
        UpdateUserInterface(entity);
    }

    private void OnInputStructureMessage(Entity<PaperComponent> entity, ref PaperInputStructureMessage args)
    {
        var expandedElements = ExpandSignatureTokens(args.Elements);
        if (!TryValidateEdit(entity, args.Actor, PaperEditAccess.Full, out var access) ||
            access != PaperEditAccess.Full)
        {
            _popupSystem.PopupEntity(Loc.GetString("paper-component-structure-session-expired"), entity, args.Actor);
            return;
        }

        if (!TryNormalizeStructure(expandedElements, entity.Comp.ContentSize, out var normalized) ||
            GetStructuredLength(normalized) > entity.Comp.ContentSize)
        {
            _popupSystem.PopupEntity(Loc.GetString("paper-component-structure-save-failed"), entity, args.Actor);
            return;
        }

        var structured = EnsureComp<StructuredPaperComponent>(entity);
        structured.Elements = normalized;
        RefreshStructuredContent(entity, structured);
        LogStructuredEdit(args.Actor, entity, $"replaced the document structure ({normalized.Count} elements)");
        _audio.PlayPvs(entity.Comp.Sound, entity);

        entity.Comp.Mode = PaperAction.Read;
        _editSessions.Remove((entity.Owner, args.Actor));
        if (_displayedEditAccess.TryGetValue(entity.Owner, out var displayed) && displayed.Editor == args.Actor)
            _displayedEditAccess.Remove(entity.Owner);
        UpdateUserInterface(entity);
    }

    private void OnUiClosed(Entity<PaperComponent> entity, ref BoundUIClosedEvent args)
    {
        if (!args.UiKey.Equals(PaperUiKey.Key))
            return;

        _editSessions.Remove((entity.Owner, args.Actor));
        if (!_displayedEditAccess.TryGetValue(entity.Owner, out var displayed) || displayed.Editor != args.Actor)
            return;

        entity.Comp.Mode = PaperAction.Read;
        _displayedEditAccess.Remove(entity.Owner);
        UpdateUserInterface(entity);
    }

    private void OnStructuredPaperMapInit(Entity<StructuredPaperComponent> entity, ref MapInitEvent args)
    {
        if (entity.Comp.TemplateLocId != null)
        {
            var templateLocId = entity.Comp.TemplateLocId;
            var template = Loc.GetString(templateLocId);
            if (StructuredPaperTemplateParser.TryParse(template, out var elements, out var error))
            {
                entity.Comp.Elements = elements;
            }
            else
            {
                Log.Error($"Failed to parse structured paper template '{templateLocId}' on {ToPrettyString(entity)}: {error}");
                entity.Comp.Elements = new List<StructuredPaperElement>
                {
                    new("template-error", StructuredPaperElementType.StaticText, template),
                };
            }

            // Persist and network only the resolved document structure from this point onward.
            entity.Comp.TemplateLocId = null;
        }

        var usedIds = new HashSet<string>();
        foreach (var element in entity.Comp.Elements)
        {
            if (element.LocId != null)
            {
                element.Text = Loc.GetString(element.LocId);
                element.LocId = null;
            }
            if (string.IsNullOrWhiteSpace(element.Id) ||
                element.Id.Length > MaxElementIdLength ||
                !usedIds.Add(element.Id))
            {
                element.Id = NewElementId(usedIds);
            }

            if (!string.IsNullOrWhiteSpace(element.PreviousText) && element.Revisions.Count < MaxFieldRevisions)
                element.Revisions.Add(new PaperFieldRevision(element.PreviousText, element.PreviousHandwritingStyle));

            element.PreviousText = string.Empty;
            element.PreviousHandwritingStyle = PaperHandwritingStyle.Default;
        }

        if (TryComp<PaperComponent>(entity, out var paper))
            RefreshStructuredContent((entity.Owner, paper), entity.Comp);
    }

    private bool TryValidateEdit(
        Entity<PaperComponent> entity,
        EntityUid actor,
        PaperEditAccess minimumAccess,
        out PaperEditAccess access)
    {
        if (!_editSessions.TryGetValue((entity.Owner, actor), out var session))
        {
            access = PaperEditAccess.None;
            return false;
        }

        access = session.Access;

        if (access < minimumAccess ||
            entity.Comp.EditingDisabled ||
            entity.Comp.StampedBy.Count > 0 && !session.IgnoreStamps)
            return false;

        var ev = new PaperWriteAttemptEvent(entity.Owner);
        RaiseLocalEvent(actor, ref ev);
        return !ev.Cancelled;
    }

    private bool TryBeginWriteSession(
        Entity<PaperComponent> entity,
        EntityUid user,
        EntityUid pen,
        PaperEditAccess access,
        bool ignoreStamps = false)
    {
        if (entity.Comp.StampedBy.Count > 0 && !ignoreStamps)
            return false;

        if (entity.Comp.EditingDisabled)
        {
            _popupSystem.PopupEntity(Loc.GetString("paper-tamper-proof-modified-message"), entity, user);
            return false;
        }

        var ev = new PaperWriteAttemptEvent(entity.Owner);
        RaiseLocalEvent(user, ref ev);
        if (ev.Cancelled)
        {
            if (ev.FailReason is not null)
                _popupSystem.PopupEntity(Loc.GetString(ev.FailReason), entity.Owner, user);

            return false;
        }

        var writeEvent = new PaperWriteEvent(user, entity);
        RaiseLocalEvent(pen, ref writeEvent);

        ClearEditSessions(entity.Owner);
        _editSessions[(entity.Owner, user)] = (access, ignoreStamps);
        _displayedEditAccess[entity.Owner] = (user, access);
        return true;
    }

    private static bool TryNormalizeStructure(
        IReadOnlyList<StructuredPaperElement> input,
        int contentSize,
        out List<StructuredPaperElement> normalized)
    {
        normalized = new List<StructuredPaperElement>();
        if (input.Count > MaxStructuredElements)
            return false;

        var usedIds = new HashSet<string>();
        var totalLength = 0;
        foreach (var inputElement in input)
        {
            if (!Enum.IsDefined(inputElement.Type))
                return false;

            if (!Enum.IsDefined(inputElement.HandwritingStyle) ||
                !Enum.IsDefined(inputElement.PreviousHandwritingStyle) ||
                inputElement.Revisions.Count > MaxFieldRevisions ||
                inputElement.Revisions.Any(revision => !Enum.IsDefined(revision.HandwritingStyle)))
                return false;

            if (inputElement.Type is (StructuredPaperElementType.SingleLineField or
                    StructuredPaperElementType.SignatureField or
                    StructuredPaperElementType.Signature) &&
                (ContainsLineBreak(inputElement.Text) ||
                 ContainsLineBreak(inputElement.PreviousText) ||
                 inputElement.Revisions.Any(revision => ContainsLineBreak(revision.Text))))
                return false;

            if (inputElement.MaxLength < 0 || inputElement.MaxLength > contentSize)
                return false;

            if (inputElement.Type != StructuredPaperElementType.StaticText &&
                (inputElement.Text.Length > inputElement.GetMaxLength() ||
                 inputElement.PreviousText.Length > inputElement.GetMaxLength() ||
                 inputElement.Revisions.Any(revision => revision.Text.Length > inputElement.GetMaxLength())))
                return false;

            if (inputElement.Type is (StructuredPaperElementType.StaticText or
                    StructuredPaperElementType.HandwrittenText) &&
                (!string.IsNullOrEmpty(inputElement.PreviousText) || inputElement.Revisions.Count > 0))
                return false;

            totalLength += inputElement.Text.Length + inputElement.PreviousText.Length +
                inputElement.Revisions.Sum(revision => revision.Text.Length) +
                (inputElement.NewLineAfter ? 1 : 0);
            if (totalLength > contentSize)
                return false;

            var id = inputElement.Id;
            if (string.IsNullOrWhiteSpace(id) || id.Length > MaxElementIdLength || !usedIds.Add(id))
                id = NewElementId(usedIds);

            normalized.Add(new StructuredPaperElement(
                id,
                inputElement.Type,
                inputElement.Text,
                newLineAfter: inputElement.NewLineAfter,
                maxLength: inputElement.Type is (StructuredPaperElementType.StaticText or StructuredPaperElementType.HandwrittenText)
                    ? 0
                    : inputElement.MaxLength)
            {
                PreviousText = inputElement.PreviousText,
                PreviousHandwritingStyle = inputElement.PreviousHandwritingStyle,
                HandwritingStyle = inputElement.HandwritingStyle,
                Revisions = inputElement.Revisions.Select(revision => revision.Copy()).ToList(),
            });
        }

        return true;
    }

    private static List<StructuredPaperElement> ExpandSignatureTokens(
        IReadOnlyList<StructuredPaperElement> elements)
    {
        var expanded = new List<StructuredPaperElement>();
        foreach (var source in elements)
        {
            if (source.Type is not (StructuredPaperElementType.StaticText or
                    StructuredPaperElementType.HandwrittenText) ||
                !source.Text.Contains(SignatureEditorToken, StringComparison.Ordinal))
            {
                expanded.Add(source.Copy());
                continue;
            }

            var parts = source.Text.Split(SignatureEditorToken, StringSplitOptions.None);
            var sourceIdUsed = false;
            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                {
                    var text = source.Copy();
                    text.Id = sourceIdUsed ? string.Empty : source.Id;
                    text.Text = parts[i];
                    text.NewLineAfter = false;
                    expanded.Add(text);
                    sourceIdUsed = true;
                }

                if (i < parts.Length - 1)
                {
                    expanded.Add(new StructuredPaperElement(
                        string.Empty,
                        StructuredPaperElementType.Signature,
                        string.Empty,
                        newLineAfter: false));
                }
            }

            expanded[^1].NewLineAfter = source.NewLineAfter;
        }

        return expanded;
    }

    private static string NewElementId(HashSet<string> usedIds)
    {
        string id;
        do
        {
            id = Guid.NewGuid().ToString("N");
        } while (!usedIds.Add(id));

        return id;
    }

    private static bool ContainsLineBreak(string text)
    {
        return text.Contains('\n') || text.Contains('\r');
    }

    private static int GetStructuredLength(IEnumerable<StructuredPaperElement> elements)
    {
        return elements.Sum(element => element.Text.Length + GetRevisionLength(element) +
            (element.NewLineAfter ? 1 : 0));
    }

    private static int GetRevisionLength(StructuredPaperElement element)
    {
        return element.PreviousText.Length + element.Revisions.Sum(revision => revision.Text.Length);
    }

    private static void AddLegacyRevision(StructuredPaperElement element, List<PaperFieldRevision> revisions)
    {
        if (!string.IsNullOrWhiteSpace(element.PreviousText) && revisions.Count < MaxFieldRevisions)
            revisions.Add(new PaperFieldRevision(element.PreviousText, element.PreviousHandwritingStyle));
    }

    public void RefreshStructuredContent(Entity<PaperComponent> entity, StructuredPaperComponent structured)
    {
        Dirty(entity.Owner, structured);
        SetContent(entity, string.Concat(structured.Elements.Select(element =>
            element.Text + (element.NewLineAfter ? "\n" : string.Empty))));
    }

    /// <summary>
    /// Applies the same text transformation to legacy content or every element of a structured document.
    /// </summary>
    public void TransformContent(Entity<PaperComponent> entity, Func<string, string> transform)
    {
        if (TryComp<StructuredPaperComponent>(entity, out var structured))
        {
            foreach (var element in structured.Elements)
            {
                element.Text = transform(element.Text);
                if (!string.IsNullOrEmpty(element.PreviousText))
                    element.PreviousText = transform(element.PreviousText);
                foreach (var revision in element.Revisions)
                    revision.Text = transform(revision.Text);
            }

            RefreshStructuredContent(entity, structured);
            return;
        }

        SetContent(entity, transform(entity.Comp.Content));
    }

    private void LogStructuredEdit(EntityUid actor, Entity<PaperComponent> entity, string action)
    {
        _adminLogger.Add(LogType.Chat,
            LogImpact.Low,
            $"{ToPrettyString(actor):player} has edited {ToPrettyString(entity):entity}: {action}");
    }

    private void ClearEditSessions(EntityUid paper)
    {
        foreach (var key in _editSessions.Keys.Where(key => key.Paper == paper).ToList())
            _editSessions.Remove(key);
    }
    // WL-Changes-StructuredPaper-End

    private void OnRandomPaperContentMapInit(Entity<RandomPaperContentComponent> ent, ref MapInitEvent args)
    {
        if (!_paperQuery.TryComp(ent, out var paperComp))
        {
            Log.Warning($"{ToPrettyString(ent)} has a {nameof(RandomPaperContentComponent)} but no {nameof(PaperComponent)}!");
            RemCompDeferred(ent, ent.Comp);
            return;
        }
        var dataset = ProtoMan.Index(ent.Comp.Dataset);
        // Intentionally not using the Pick overload that directly takes a LocalizedDataset,
        // because we want to get multiple attributes from the same pick.
        var pick = _random.Pick(dataset.Values);

        // Name
        _metaSystem.SetEntityName(ent, Loc.GetString(pick));
        // Description
        _metaSystem.SetEntityDescription(ent, Loc.GetString($"{pick}.desc"));
        // Content
        SetContent((ent, paperComp), Loc.GetString($"{pick}.content"));

        // Our work here is done
        RemCompDeferred(ent, ent.Comp);
    }

    private void OnPaperWrite(Entity<ActivateOnPaperOpenedComponent> entity, ref PaperWriteEvent args)
    {
        _interaction.UseInHandInteraction(args.User, entity);
    }

    /// <summary>
    ///     Accepts the name and state to be stamped onto the paper, returns true if successful.
    /// </summary>
    public bool TryStamp(Entity<PaperComponent> entity, StampDisplayInfo stampInfo, string spriteStampState)
    {
        if (!entity.Comp.StampedBy.Contains(stampInfo))
        {
            entity.Comp.StampedBy.Add(stampInfo);
            Dirty(entity);
            // WL-Changes-StructuredPaper-Start
            // A stamp applied while somebody is filling a form must close ordinary edit access immediately.
            if (_displayedEditAccess.TryGetValue(entity.Owner, out var displayed) &&
                displayed.Access != PaperEditAccess.Full)
            {
                ClearEditSessions(entity.Owner);
                _displayedEditAccess.Remove(entity.Owner);
                entity.Comp.Mode = PaperAction.Read;
            }
            // WL-Changes-StructuredPaper-End
            if (entity.Comp.StampState == null && TryComp<AppearanceComponent>(entity, out var appearance))
            {
                entity.Comp.StampState = spriteStampState;
                // Would be nice to be able to display multiple sprites on the paper
                // but most of the existing images overlap
                _appearance.SetData(entity, PaperVisuals.Stamp, entity.Comp.StampState, appearance);
            }
        }
        return true;
    }

    /// <summary>
    ///     Copy any stamp information from one piece of paper to another.
    /// </summary>
    public void CopyStamps(Entity<PaperComponent?> source, Entity<PaperComponent?> target)
    {
        if (!Resolve(source, ref source.Comp) || !Resolve(target, ref target.Comp))
            return;

        target.Comp.StampedBy = new List<StampDisplayInfo>(source.Comp.StampedBy);
        target.Comp.StampState = source.Comp.StampState;
        Dirty(target);

        if (TryComp<AppearanceComponent>(target, out var appearance))
        {
            // delete any stamps if the stamp state is null
            _appearance.SetData(target, PaperVisuals.Stamp, target.Comp.StampState ?? "", appearance);
        }
    }

    public void SetContent(EntityUid entity, string content)
    {
        if (!TryComp<PaperComponent>(entity, out var paper))
            return;
        SetContent((entity, paper), content);
    }

    public void SetContent(Entity<PaperComponent> entity, string content)
    {
        entity.Comp.Content = content;
        Dirty(entity);
        UpdateUserInterface(entity);

        if (!TryComp<AppearanceComponent>(entity, out var appearance))
            return;

        var status = string.IsNullOrWhiteSpace(content)
            ? PaperStatus.Blank
            : PaperStatus.Written;

        _appearance.SetData(entity, PaperVisuals.Status, status, appearance);
    }

    private void UpdateUserInterface(Entity<PaperComponent> entity)
    {
        // WL-Changes-StructuredPaper-Start
        List<StructuredPaperElement>? elements = null;
        if (TryComp<StructuredPaperComponent>(entity, out var structured))
            elements = structured.Elements.Select(element => element.Copy()).ToList();

        var access = PaperEditAccess.None;
        NetEntity? editor = null;
        if (_displayedEditAccess.TryGetValue(entity.Owner, out var displayed))
        {
            access = displayed.Access;
            editor = GetNetEntity(displayed.Editor);
        }

        _uiSystem.SetUiState(entity.Owner,
            PaperUiKey.Key,
            new PaperBoundUserInterfaceState(
                entity.Comp.Content,
                entity.Comp.StampedBy,
                entity.Comp.Mode,
                elements,
                access,
                editor));
        // WL-Changes-StructuredPaper-End
    }
}

/// <summary>
/// Event fired when using a pen on paper, opening the UI.
/// </summary>
[ByRefEvent]
public record struct PaperWriteEvent(EntityUid User, EntityUid Paper);

/// <summary>
/// Cancellable event for attempting to write on a piece of paper.
/// </summary>
/// <param name="paper">The paper that the writing will take place on.</param>
[ByRefEvent]
public record struct PaperWriteAttemptEvent(EntityUid Paper, string? FailReason = null, bool Cancelled = false);
