using System.Numerics;
using Content.Shared._Ship14.CorticalBorer;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Client._Ship14.CorticalBorer;

public sealed class CorticalBorerWindow : DefaultWindow
{
    public event Action<string>? MessageSubmitted;

    private readonly BoxContainer _options;

    public CorticalBorerWindow()
    {
        Title = Loc.GetString("cortical-borer-window-title");
        MinSize = new Vector2(320, 160);

        var root = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            Margin = new Thickness(8),
        };
        Contents.AddChild(root);

        _options = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            VerticalExpand = true,
        };
        root.AddChild(_options);
    }

    public void UpdateState(CorticalBorerBuiState state)
    {
        _options.RemoveAllChildren();

        if (state.Mode == CorticalBorerMenuMode.Converse)
            AddConverse();
    }

    private void AddConverse()
    {
        var input = new LineEdit
        {
            PlaceHolder = Loc.GetString("cortical-borer-window-message-placeholder"),
            HorizontalExpand = true,
            SetHeight = 32,
        };
        input.OnTextEntered += args => MessageSubmitted?.Invoke(args.Text);
        _options.AddChild(input);

        var send = new Button { Text = Loc.GetString("cortical-borer-window-send") };
        send.OnPressed += _ => MessageSubmitted?.Invoke(input.Text);
        _options.AddChild(send);
    }
}
