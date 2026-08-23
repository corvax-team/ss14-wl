using JetBrains.Annotations;
using Content.Shared.Hands.EntitySystems;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Content.Shared.Paper;
using Content.Shared._WL.Paper; // WL-Changes: Structured paper forms
using Content.Shared.Tag;
using static Content.Shared.Paper.PaperComponent;

namespace Content.Client.Paper.UI;

[UsedImplicitly]
public sealed class PaperBoundUserInterface : BoundUserInterface
{
    private static readonly ProtoId<TagPrototype> WriteTag = "Write";

    [ViewVariables]
    private PaperWindow? _window;

    public PaperBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        // WL-Changes-StructuredPaper-Start
        // BoundUserInterface applies its first state after Open(). Create the paper window here, but only show it
        // after Populate() so rich text and form fields are measured together instead of flashing an empty form.
        _window = this.CreateDisposableControl<PaperWindow>();
        _window.OnClose += Close;
        EntMan.System<UserInterfaceSystem>().RegisterControl(this, _window);
        // WL-Changes-StructuredPaper-End
        _window.OnSaved += InputOnTextEntered;
        // WL-Changes-StructuredPaper-Start
        _window.OnFieldSaved += InputOnFieldEntered;
        _window.OnFieldEditRequested += InputOnFieldEditRequested;
        _window.OnSignatureRequested += InputOnSignatureRequested;
        _window.OnTextAppended += InputOnTextAppended;
        _window.OnElementsAppended += InputOnElementsAppended;
        _window.OnStructureSaved += InputOnStructureEntered;
        _window.CanBeginFieldEdit = HasActivePen;
        // WL-Changes-StructuredPaper-End

        if (EntMan.TryGetComponent<PaperComponent>(Owner, out var paper))
        {
            _window.MaxInputLength = paper.ContentSize;
        }
        if (EntMan.TryGetComponent<PaperVisualsComponent>(Owner, out var visuals))
        {
            _window.InitVisuals(Owner, visuals);
        }
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (_window == null)
            return;

        var paperState = (PaperBoundUserInterfaceState) state;
        _window.HandwritingStyle = PlayerManager.LocalEntity is { } localPlayer &&
            EntMan.TryGetComponent<PaperHandwritingComponent>(localPlayer, out var handwriting)
                ? handwriting.Style
                : PaperHandwritingStyle.Default;
        var isEditor = paperState.Editor != null &&
            PlayerManager.LocalEntity is { } player &&
            paperState.Editor == EntMan.GetNetEntity(player);
        _window.Populate(paperState, isEditor);
        // WL-Changes-StructuredPaper-Start
        if (_window.IsOpen)
            return;

        _window.PrepareForFirstReveal();
        var uiSystem = EntMan.System<UserInterfaceSystem>();
        if (uiSystem.TryGetPosition(Owner, UiKey, out var position))
            _window.Open(position);
        else
            _window.OpenCentered();
        // WL-Changes-StructuredPaper-End
    }

    private void InputOnTextEntered(string text)
    {
        SendMessage(new PaperInputTextMessage(text));

        if (_window != null)
        {
            _window.Input.TextRope = Rope.Leaf.Empty;
            _window.Input.CursorPosition = new TextEdit.CursorPos(0, TextEdit.LineBreakBias.Top);
        }
    }

    // WL-Changes-StructuredPaper-Start
    private void InputOnFieldEntered(string fieldId, string text)
    {
        SendMessage(new PaperInputFieldMessage(fieldId, text));
    }

    private void InputOnFieldEditRequested(string fieldId)
    {
        SendMessage(new PaperRequestFieldEditMessage(fieldId));
    }

    private void InputOnSignatureRequested(string fieldId)
    {
        SendMessage(new PaperSignFieldMessage(fieldId));
    }

    private bool HasActivePen()
    {
        if (PlayerManager.LocalEntity is not { } player ||
            !EntMan.System<SharedHandsSystem>().TryGetActiveItem(player, out var held))
        {
            return false;
        }

        return EntMan.System<TagSystem>().HasTag(held.Value, WriteTag);
    }

    private void InputOnTextAppended(string text)
    {
        SendMessage(new PaperAppendTextMessage(text));
    }

    private void InputOnElementsAppended(List<StructuredPaperElement> elements)
    {
        SendMessage(new PaperAppendElementsMessage(elements));
    }

    private void InputOnStructureEntered(List<StructuredPaperElement> elements)
    {
        SendMessage(new PaperInputStructureMessage(elements));
    }
    // WL-Changes-StructuredPaper-End
}
