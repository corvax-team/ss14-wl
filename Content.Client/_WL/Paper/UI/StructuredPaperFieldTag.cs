using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Client.Resources;
using Content.Shared._WL.Paper;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.RichText;
using Robust.Client.ResourceManagement;
using Robust.Shared.Input;
using Robust.Shared.Maths;
using Robust.Shared.Utility;

namespace Content.Client._WL.Paper.UI;

public sealed partial class StructuredPaperAppendTag : IMarkupTagHandler
{
    public const string TagName = "paperappend";

    public string Name => TagName;

    public bool TryCreateControl(MarkupNode node, [NotNullWhen(true)] out Control? control)
    {
        control = new StructuredPaperAppendControl();
        return true;
    }
}

public sealed class StructuredPaperAppendControl : ContainerButton
{
    public StructuredPaperAppendControl()
    {
        StyleClasses.Add(StyleClassButton);
        StyleClasses.Add("PaperAppendButton");
        ToolTip = Loc.GetString("paper-ui-form-append-line-tooltip");
        MinSize = new Vector2(18, 18);
        Margin = new Thickness(0, 1, 0, 0);
        DefaultCursorShape = CursorShape.Hand;

        AddChild(new Label
        {
            Text = "+",
            MouseFilter = MouseFilterMode.Ignore,
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            FontOverride = IoCManager.Resolve<IResourceCache>()
                .GetFont("/Fonts/NotoSans/NotoSans-Bold.ttf", 13),
            FontColorOverride = Color.FromHex("#3d3933"),
        });
    }
}

public sealed partial class StructuredPaperFieldTag : IMarkupTagHandler
{
    public const string TagName = "paperfield";

    public string Name => TagName;

    public bool TryCreateControl(MarkupNode node, [NotNullWhen(true)] out Control? control)
    {
        control = null;

        if (!node.Value.TryGetString(out var id) || string.IsNullOrWhiteSpace(id))
            return false;

        var multiline = node.Attributes.TryGetValue("multiline", out var multilineParam) &&
            multilineParam.LongValue == 1;
        var signature = node.Attributes.TryGetValue("signature", out var signatureParam) &&
            signatureParam.LongValue == 1;
        var signatureAction = node.Attributes.TryGetValue("signature-action", out var signatureActionParam) &&
            signatureActionParam.LongValue == 1;
        var editable = node.Attributes.TryGetValue("editable", out var editableParam) &&
            editableParam.LongValue == 1;
        var selected = node.Attributes.TryGetValue("selected", out var selectedParam) &&
            selectedParam.LongValue == 1;
        var style = PaperHandwritingStyle.Default;
        if (node.Attributes.TryGetValue("style", out var styleParam) &&
            styleParam.LongValue is { } styleValue &&
            Enum.IsDefined(typeof(PaperHandwritingStyle), (byte) styleValue))
        {
            style = (PaperHandwritingStyle) styleValue;
        }

        var previousStyle = PaperHandwritingStyle.Default;
        if (node.Attributes.TryGetValue("previous-style", out var previousStyleParam) &&
            previousStyleParam.LongValue is { } previousStyleValue &&
            Enum.IsDefined(typeof(PaperHandwritingStyle), (byte) previousStyleValue))
        {
            previousStyle = (PaperHandwritingStyle) previousStyleValue;
        }

        var text = string.Empty;
        if (node.Attributes.TryGetValue("text", out var textParam) &&
            textParam.TryGetString(out var parsedText))
        {
            text = parsedText;
        }


        var previous = string.Empty;
        if (node.Attributes.TryGetValue("previous", out var previousParam) &&
            previousParam.TryGetString(out var parsedPrevious))
        {
            previous = parsedPrevious;
        }

        var placeholder = string.Empty;
        if (node.Attributes.TryGetValue("placeholder", out var placeholderParam) &&
            placeholderParam.TryGetString(out var parsedPlaceholder))
        {
            placeholder = parsedPlaceholder;
        }


        var revisions = string.IsNullOrWhiteSpace(previous)
            ? Array.Empty<PaperFieldRevision>()
            : [new PaperFieldRevision(previous, previousStyle)];
        var field = new StructuredPaperFieldControl(
            id,
            text,
            revisions,
            placeholder,
            multiline,
            signature,
            editable,
            selected,
            style,
            signatureAction);
        control = field;
        return true;
    }
}

public sealed class StructuredPaperFieldControl : ContainerButton
{
    private static readonly Color PaperTextColor = new(25, 25, 25);
    private readonly bool _multiline;
    private readonly bool _signature;
    private readonly WrapContainer _content;
    private readonly TextureButton? _signatureButton;
    private PanelContainer? _currentPanel;
    private bool _editable;
    private bool _empty;
    private PaperHandwritingStyle _style;
    private string _text = string.Empty;
    private List<PaperFieldRevision> _revisions = new();
    private string _placeholder = string.Empty;
    private bool _hovered;
    private bool _selected;
    private bool _styleInitialized;
    private bool _showSignatureAction;
    private static readonly Type[] HandwritingTags =
    [
        typeof(ColorTag),
        typeof(FontTag),
        typeof(ItalicTag),
    ];

    public string FieldId { get; }
    public event Action? OnSignatureRequested;

    public StructuredPaperFieldControl(
        string fieldId,
        string text,
        IReadOnlyList<PaperFieldRevision> revisions,
        string placeholder,
        bool multiline,
        bool signature,
        bool editable,
        bool selected,
        PaperHandwritingStyle style,
        bool showSignatureAction)
    {
        FieldId = fieldId;
        _multiline = multiline;
        _signature = signature;
        _editable = editable;
        _empty = string.IsNullOrWhiteSpace(text);
        _selected = selected;
        _style = style;
        _showSignatureAction = showSignatureAction;

        Disabled = false;
        MouseFilter = MouseFilterMode.Stop;
        DefaultCursorShape = CursorShape.Hand;
        MinWidth = multiline ? 230 : signature ? 110 : 86;
        MinHeight = multiline && _empty ? 42 : 18;
        VerticalAlignment = multiline ? VAlignment.Top : VAlignment.Bottom;
        Margin = multiline
            ? _empty ? new Thickness(0, 1, 0, 3) : new Thickness(0, 0, 0, 1)
            : new Thickness(1, 0);

        _content = new WrapContainer
        {
            LayoutAxis = Axis.Horizontal,
            SeparationOverride = 3,
            CrossSeparationOverride = 1,
            MouseFilter = MouseFilterMode.Ignore,
            HorizontalExpand = multiline,
        };

        if (signature)
        {
            var row = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                SeparationOverride = 1,
                MouseFilter = MouseFilterMode.Ignore,
            };
            row.AddChild(_content);
            _signatureButton = new TextureButton
            {
                TextureNormal = IoCManager.Resolve<IResourceCache>()
                    .GetTexture("/Textures/Interface/character.svg.192dpi.png"),
                ToolTip = Loc.GetString("paper-ui-form-sign-as-character"),
                MinSize = new Vector2(17, 17),
                MaxSize = new Vector2(17, 17),
                Margin = new Thickness(1, 0, 0, 0),
                ModulateSelfOverride = Color.FromHex("#5d554b"),
                DefaultCursorShape = CursorShape.Hand,
            };
            _signatureButton.OnPressed += _ => OnSignatureRequested?.Invoke();
            row.AddChild(_signatureButton);
            AddChild(row);
        }
        else
        {
            AddChild(_content);
        }

        UpdateContent(text, revisions, placeholder, editable, selected, style, showSignatureAction);

        OnMouseEntered += _ =>
        {
            _hovered = true;
            UpdateStyle();
        };
        OnMouseExited += _ =>
        {
            _hovered = false;
            UpdateStyle();
        };
    }

    public void SetSelected(bool selected)
    {
        if (_selected == selected && _styleInitialized)
            return;

        _selected = selected;
        UpdateStyle();
        _styleInitialized = true;
    }

    public void SetDocumentWidth(float width)
    {
        if (!_multiline || width <= 0)
            return;

        var fieldWidth = Math.Max(96, width);
        if (MathHelper.CloseTo(MinWidth, fieldWidth))
            return;

        MinWidth = fieldWidth;
        if (_currentPanel != null)
            _currentPanel.MinWidth = fieldWidth;
    }

    public void UpdateContent(
        string text,
        IReadOnlyList<PaperFieldRevision> revisions,
        string placeholder,
        bool editable,
        bool selected,
        PaperHandwritingStyle style,
        bool showSignatureAction)
    {
        var visualStyleChanged = !_styleInitialized ||
            _editable != editable ||
            _empty != string.IsNullOrWhiteSpace(text) ||
            _selected != selected;
        var contentChanged = _text != text ||
            !RevisionsEqual(_revisions, revisions) ||
            _placeholder != placeholder ||
            _style != style;

        _text = text;
        _revisions = revisions.Select(revision => revision.Copy()).ToList();
        _placeholder = placeholder;
        _editable = editable;
        _empty = string.IsNullOrWhiteSpace(text);
        _selected = selected;
        _style = style;
        _showSignatureAction = showSignatureAction;
        if (_signatureButton != null)
            _signatureButton.Visible = _signature && _empty && _showSignatureAction;
        MinHeight = _multiline && _empty ? 42 : 18;
        Margin = _multiline
            ? _empty ? new Thickness(0, 1, 0, 3) : new Thickness(0, 0, 0, 1)
            : new Thickness(1, 0);

        if (contentChanged)
        {
            _content.RemoveAllChildren();
            foreach (var revision in _revisions)
            {
                if (!string.IsNullOrWhiteSpace(revision.Text))
                    _content.AddChild(CreateWrittenLabel(revision.Text, revision.HandwritingStyle, true));
            }

            _currentPanel = new PanelContainer
            {
                MouseFilter = MouseFilterMode.Ignore,
                HorizontalExpand = _multiline,
                MinWidth = MinWidth,
            };
            _currentPanel.AddChild(CreateCurrentLabel(text, placeholder));
            _content.AddChild(_currentPanel);
        }

        if (visualStyleChanged)
        {
            UpdateStyle();
            _styleInitialized = true;
        }
    }

    private static bool RevisionsEqual(
        IReadOnlyList<PaperFieldRevision> left,
        IReadOnlyList<PaperFieldRevision> right)
    {
        if (left.Count != right.Count)
            return false;

        for (var i = 0; i < left.Count; i++)
        {
            if (left[i].Text != right[i].Text || left[i].HandwritingStyle != right[i].HandwritingStyle)
                return false;
        }

        return true;
    }

    private RichTextLabel CreateCurrentLabel(string text, string placeholder)
    {
        var label = new RichTextLabel
        {
            MouseFilter = MouseFilterMode.Ignore,
            HorizontalExpand = true,
            VerticalAlignment = _multiline ? VAlignment.Top : VAlignment.Center,
            HorizontalAlignment = _empty && !_multiline ? HAlignment.Center : HAlignment.Left,
            StyleClasses = { "PaperWrittenText" },
        };
        var message = new FormattedMessage();
        if (_empty)
        {
            message.AddText(placeholder);
        }
        else
        {
            AddHandwriting(message, text, _style);
        }

        label.SetMessage(message, HandwritingTags, PaperTextColor);
        label.ModulateSelfOverride = _empty
            ? Color.FromHex("#756e6270")
            : null;
        return label;
    }

    private static RichTextLabel CreateWrittenLabel(
        string text,
        PaperHandwritingStyle style,
        bool struck)
    {
        RichTextLabel label = struck ? new StruckPaperTextLabel() : new RichTextLabel();
        label.MouseFilter = MouseFilterMode.Ignore;
        label.HorizontalExpand = false;
        label.MaxWidth = 430;
        label.StyleClasses.Add("PaperWrittenText");

        var message = new FormattedMessage();
        AddHandwriting(message, text, style);
        label.SetMessage(message, HandwritingTags, PaperTextColor);
        label.ModulateSelfOverride = Color.FromHex("#574f4690");
        return label;
    }

    public static void AddHandwriting(
        FormattedMessage message,
        string text,
        PaperHandwritingStyle style,
        bool markup = false)
    {
        var (fontId, size, color) = GetHandwritingPresentation(style);
        message.PushTag(new MarkupNode("font", new MarkupParameter(fontId), new Dictionary<string, MarkupParameter>
        {
            ["size"] = new(size),
        }));
        message.PushColor(color);
        if (markup)
            message.AddMarkupPermissive(text);
        else
            message.AddText(text);
        message.Pop();
        message.Pop();
    }

    private static (string FontId, long Size, Color Color) GetHandwritingPresentation(PaperHandwritingStyle style)
    {
        return style switch
        {
            PaperHandwritingStyle.Neat => ("HandwritingNeat", 14, Color.FromHex("#27231f")),
            PaperHandwritingStyle.Quick => ("HandwritingQuick", 16, Color.FromHex("#46392b")),
            PaperHandwritingStyle.Formal => ("HandwritingFormal", 15, Color.FromHex("#20242d")),
            PaperHandwritingStyle.Heavy => ("HandwritingHeavy", 13, Color.FromHex("#1d1b19")),
            PaperHandwritingStyle.Messy => ("HandwritingMessy", 15, Color.FromHex("#513427")),
            _ => ("Default", 12, PaperTextColor),
        };
    }

    private void UpdateStyle()
    {
        StyleBoxOverride = null;
        if (_currentPanel == null)
            return;

        _currentPanel.PanelOverride = new StyleBoxFlat
        {
            BackgroundColor = GetBackgroundColor(),
            BorderColor = GetBorderColor(),
            BorderThickness = new Thickness(0, 0, 0, _empty ? 1 : 0),
            ContentMarginLeftOverride = 3,
            ContentMarginRightOverride = 3,
            ContentMarginTopOverride = 0,
            ContentMarginBottomOverride = _empty ? 1 : 0,
        };
    }

    private Color GetBackgroundColor()
    {
        if (!_editable)
            return Color.Transparent;

        if (_selected)
            return Color.FromHex("#cfc7b73a");

        if (_hovered)
            return Color.FromHex("#d8d1c42c");

        return _empty
            ? Color.FromHex("#ded8cd20")
            : Color.Transparent;
    }

    private Color GetBorderColor()
    {
        if (_selected)
            return Color.FromHex("#75634882");

        if (_hovered)
            return Color.FromHex("#8f806c60");

        return _empty
            ? Color.FromHex("#8f806c48")
            : Color.FromHex("#8f806c24");
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);

        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        SetSelected(true);
    }
}

public sealed class StruckPaperTextLabel : RichTextLabel
{
    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        const float lineHeight = 16f;
        for (var y = lineHeight * 0.64f; y < Size.Y; y += lineHeight)
            handle.DrawLine(new Vector2(0, y), new Vector2(Size.X, y), Color.FromHex("#66574c90"));
    }
}
