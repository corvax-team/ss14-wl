using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Input;
using Robust.Shared.Utility;
using Content.Client.UserInterface.ControlExtensions;

namespace Content.Client.Guidebook.RichText;

[UsedImplicitly]
public sealed class TextLinkTag : IMarkupTagHandler
{
    public static Color LinkColor => Color.CornflowerBlue;

    public string Name => "textlink";

    /// <inheritdoc/>
    public bool TryCreateControl(MarkupNode node, [NotNullWhen(true)] out Control? control)
    {
        // WL-Changes-start
        control = null;

        if (!node.Value.TryGetString(out var text))
            return false;

        var label = new RichTextLabel()
        {
            MouseFilter = Control.MouseFilterMode.Stop,
            DefaultCursorShape = Control.CursorShape.Hand,
            Margin = new Thickness(1),
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        label.SetMessage(text, defaultColor: LinkColor);

        label.OnMouseEntered += _ => label.SetMessage(text, defaultColor: Color.LightSkyBlue);
        label.OnMouseExited += _ => label.SetMessage(text, defaultColor: Color.CornflowerBlue);

        if (node.Attributes.TryGetValue("tip", out var tipParameter) &&
            tipParameter.TryGetString(out var tipText))
            label.ToolTip = tipText;

        if (node.Attributes.TryGetValue("link", out var linkParameter) &&
            linkParameter.TryGetString(out var link))
            label.OnKeyBindDown += args => OnKeybindDown(args, link, label);
        // WL-Changes-end

        control = label;
        return true;
    }

    private void OnKeybindDown(GUIBoundKeyEventArgs args, string link, Control? control)
    {
        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        if (control == null)
            return;

        if (control.TryGetParentHandler<ILinkClickHandler>(out var handler))
            handler.HandleClick(link);
        else
            Logger.Warning("Warning! No valid ILinkClickHandler found.");
    }
}

public interface ILinkClickHandler
{
    public void HandleClick(string link);
}
