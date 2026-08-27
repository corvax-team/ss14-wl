using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.Stylesheets;
using Robust.Shared.Input;
using Robust.Shared.Timing;
using System.Linq;
using System.Numerics;
using static Robust.Client.UserInterface.StylesheetHelpers;

namespace Content.Client._WL.UserInterface.Systems.Computers;

public sealed partial class CmdLineEdit : HistoryLineEdit
{
    [Dependency] private IResourceCache _resourceCache = default!;

    public const string StyleClassCmdLineEdit = "cmdLineEdit";

    public event Action<CmdLineCommandEntry>? OnCommandEntered;

    public string ArgumentPrefix { get; set; } = "--";
    public Font? OverrideFont
    {
        get => _overrideFont;
        set
        {
            _overrideFont = value;
            UpdateStylesheet();
        }
    }

    private Font? _overrideFont = null;
    public CmdLineEdit()
    {
        IoCManager.InjectDependencies(this);

        DefaultCursorShape = CursorShape.Arrow;

        OnTextChanged += args =>
        {
            if (!Editable)
                return;

            var le = args.Control;
            var contentWidth = le.GetOffsetAtIndex(le.Text.Length);
            var desiredWidth = contentWidth + 2;
            le.SetSize = new Vector2(desiredWidth, MeasureOverride(Vector2.Zero).Y);
            le.Parent?.InvalidateMeasure();

            args.Control.CursorPosition = args.Control.Text.Length;
            args.Control.SelectionStart = args.Control.Text.Length;
        };

        var style = new DefaultStylesheet(_resourceCache, UserInterfaceManager).Stylesheet;

        StyleClasses.Add(StyleClassCmdLineEdit);

        StyleBoxOverride = new StyleBoxFlat(Color.Transparent);
        OnTextEntered += args =>
        {
            if (!Editable)
                return;

            var raw = args.Text.Trim();
            if (string.IsNullOrEmpty(raw))
                return;

            var parts = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var commandName = parts[0];
            var positional = new List<string>();
            var flags = new Dictionary<string, List<string>>();

            string? currentFlag = null;

            for (var i = 1; i < parts.Length; i++)
            {
                var token = parts[i];

                if (token.StartsWith(ArgumentPrefix))
                {
                    currentFlag = token[ArgumentPrefix.Length..];
                    if (!flags.ContainsKey(currentFlag))
                        flags[currentFlag] = [];
                }
                else
                {
                    if (currentFlag != null)
                        flags[currentFlag].Add(token);
                    else
                        positional.Add(token);
                }
            }

            OnCommandEntered?.Invoke(
                new CmdLineCommandEntry(
                    raw,
                    commandName,
                    positional,
                    flags
                ));
        };
    }

    private void UpdateStylesheet()
    {
        var font = OverrideFont ?? UserInterfaceManager.ThemeDefaults.DefaultFont;

        var rule1 = Element<LineEdit>().Class(StyleClassCmdLineEdit)
            .Prop("font", font);

        var rule2 = Element<LineEdit>().Class(StyleClassCmdLineEdit)
            .Prop("cursor-color", Color.Transparent);

        Stylesheet = new Stylesheet([rule1, rule2]);
    }

    public void SetInputEnabled(bool value)
    {
        Editable = value;
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        var fn = args.Function;

        if (fn == EngineKeyFunctions.TextCursorLeft ||
                fn == EngineKeyFunctions.TextCursorRight ||
                fn == EngineKeyFunctions.TextCursorWordLeft ||
                fn == EngineKeyFunctions.TextCursorWordRight ||
                fn == EngineKeyFunctions.TextCursorBegin ||
                fn == EngineKeyFunctions.TextCursorEnd ||
                fn == EngineKeyFunctions.TextCursorSelectLeft ||
                fn == EngineKeyFunctions.TextCursorSelectRight ||
                fn == EngineKeyFunctions.TextCursorSelectWordLeft ||
                fn == EngineKeyFunctions.TextCursorSelectWordRight ||
                fn == EngineKeyFunctions.TextCursorSelectBegin ||
                fn == EngineKeyFunctions.TextCursorSelectEnd ||
                fn == EngineKeyFunctions.UIClick ||
                fn == EngineKeyFunctions.TextCursorSelect)
        {
            args.Handle();
            return;
        }

        base.KeyBindDown(args);
    }

    protected override void MouseMove(GUIMouseMoveEventArgs args)
    {
        // skip
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        CursorPosition = Text.Length;
        SelectionStart = Text.Length;
    }

    public readonly record struct CmdLineCommandEntry(
        string RawText,
        string CommandName,
        List<string> Positional,
        Dictionary<string, List<string>> Flags
    );
}
