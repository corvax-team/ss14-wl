using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._WL.Records;

/// <summary>
/// A multiline record field with an explicit background and focus border.
/// </summary>
public sealed class RecordTextEdit : PanelContainer
{
    private static readonly StyleBoxFlat NormalStyle = new()
    {
        BackgroundColor = Color.FromHex("#17171B"),
        BorderColor = Color.FromHex("#45454D"),
        BorderThickness = new Thickness(1),
    };

    private static readonly StyleBoxFlat FocusedStyle = new()
    {
        BackgroundColor = Color.FromHex("#19191E"),
        BorderColor = Color.FromHex("#9B4DB5"),
        BorderThickness = new Thickness(1),
    };

    public readonly TextEdit Input;
    private readonly Label _characterCounter;
    private bool _focused;
    private int? _characterLimit;
    private string? _pendingClampedText;

    public Rope.Node TextRope
    {
        get => Input.TextRope;
        set
        {
            Input.TextRope = value;
            UpdateCharacterCounter();
        }
    }

    public event Action<TextEdit.TextEditEventArgs>? OnTextChanged;

    public RecordTextEdit()
    {
        PanelOverride = NormalStyle;
        RectClipContent = true;
        MouseFilter = MouseFilterMode.Pass;

        Input = new TextEdit
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            Margin = new Thickness(7, 6, 7, 0),
        };
        Input.OnTextChanged += HandleTextChanged;

        _characterCounter = new Label
        {
            HorizontalAlignment = HAlignment.Right,
            Margin = new Thickness(7, 0, 7, 4),
            Visible = false,
        };
        _characterCounter.StyleClasses.Add("LabelSubText");

        AddChild(new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            Children =
            {
                Input,
                _characterCounter,
            },
        });
    }

    public void SetCharacterLimit(int limit)
    {
        _characterLimit = limit;
        _characterCounter.Visible = true;
        UpdateCharacterCounter();
    }

    protected override void MouseWheel(GUIMouseWheelEventArgs args)
    {
        base.MouseWheel(args);

        // TextEdit has already consumed the wheel input by the time it bubbles up here.
        // Stop it from also scrolling the record tab behind the field.
        args.Handle();
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        ApplyPendingTextLimit();

        var focused = Input.HasKeyboardFocus();
        if (_focused == focused)
            return;

        _focused = focused;
        PanelOverride = focused ? FocusedStyle : NormalStyle;
    }

    private void HandleTextChanged(TextEdit.TextEditEventArgs args)
    {
        if (_characterLimit is { } limit && Input.TextLength > limit)
        {
            var text = Rope.Collapse(args.TextRope);
            var clampedLength = limit;
            if (clampedLength > 0 && char.IsHighSurrogate(text[clampedLength - 1]))
                clampedLength--;

            _pendingClampedText = text[..clampedLength];
            return;
        }

        _pendingClampedText = null;
        UpdateCharacterCounter();
        OnTextChanged?.Invoke(args);
    }

    private void ApplyPendingTextLimit()
    {
        if (_pendingClampedText == null)
            return;

        var text = _pendingClampedText;
        _pendingClampedText = null;
        var rope = new Rope.Leaf(text);
        var cursor = Input.CursorPosition;

        Input.TextRope = rope;
        Input.CursorPosition = cursor with { Index = Math.Min(cursor.Index, text.Length) };
        UpdateCharacterCounter();
        OnTextChanged?.Invoke(new TextEdit.TextEditEventArgs(Input, rope));
    }

    private void UpdateCharacterCounter()
    {
        if (_characterLimit is not { } limit)
            return;

        _characterCounter.Text = $"{Input.TextLength} / {limit}";
    }
}
