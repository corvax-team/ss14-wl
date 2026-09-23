using System.Diagnostics.CodeAnalysis;
using Content.Client.Stylesheets.Fonts;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Utility;
using Content.Client.UserInterface.Controls;
using Content.Shared.Chat;
using Robust.Client.ResourceManagement;

namespace Content.Client.UserInterface.RichText;

/// <summary>
/// Markup tag handler for <c>[textlink="LinkText"]</c> nodes. Renders a link
/// <see cref="Label"/> in rich text, covering two link types:
/// <list type="bullet">
/// <item><description>link="GuideEntryPrototypeID" — a plain link.</description></item>
/// <item><description>entity="NetEntity" — an entity link.</description></item>
/// </list>
/// Optional parameters:
/// <list type="bullet">
/// <item><description>color="HexColor" — color override.</description></item>
/// <item><description>entitynamecolor="Bool" — entity links only; opt into using the entity's name color.</description></item>
/// </list>
/// </summary>
[UsedImplicitly]
public sealed partial class TextLinkTag : IMarkupTagHandler
{
    [Dependency] private IEntityManager _entity = default!;
    [Dependency] private IUserInterfaceManager _ui = default!;
    [Dependency] private IResourceCache _cache = default!;
    private SharedChatSystem? _chat;

    public string Name => "textlink";
    public static Color DefaultLinkColor => Color.CornflowerBlue;
    private const string EntityAttributeName = "entity";
    private const string LinkAttributeName = "link";
    private const string ColorOverrideAttributeName = "color"; // DefaultLinkColor override
    private const string UseEntityNameColorAttributeName = "entitynamecolor"; // entity links only: opt into per-entity name coloring

    private delegate bool TryResolveLink(MarkupNode node, out LinkData data);
    private readonly (string AttributeName, TryResolveLink Resolver)[] _resolvers; // for parsing link to correct resolver
    /// <summary>
    /// Resolved Link Data, LinkString and LinkEntity should not be populated at the same time
    /// </summary>
    private readonly record struct LinkData(string? LinkString, NetEntity? LinkEntity, Color? Color);

    public TextLinkTag()
    {
        _resolvers =
        [
            (EntityAttributeName, TryResolveEntityLink),
            (LinkAttributeName, TryResolvePlainLink),
        ];
    }

    /// <summary>
    ///  Takes a TextLink <see cref="MarkupNode"/>, parses it and creates a TextLink <see cref="Label"/>.
    /// Fails if it cannot parse link content, or if the resolver cannot create valid <see cref="LinkData"/>.
    /// </summary>
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

        control = linkLabel;
        return true;
    }

    private static Color? ResolveColorOverride(MarkupNode node)
    {
        if (!node.Attributes.TryGetValue(ColorOverrideAttributeName, out var colorParam) ||
            !colorParam.TryGetString(out var colorStr))
        {
            return null;
        }

        return Color.TryFromHex(colorStr, out var color) ? color : null;
    }
}

/// <summary>
/// Implement on a control to receive clicks on nested [textlink link=] nodes.
/// </summary>
public interface ILinkClickHandler
{
    public void HandleClick(string link);
}

/// <summary>
/// Implement on a control to receive clicks on nested [textlink entity=] nodes.
/// </summary>
public interface IEntityLinkClickHandler
{
    public void HandleClick(NetEntity netEntity);
}
