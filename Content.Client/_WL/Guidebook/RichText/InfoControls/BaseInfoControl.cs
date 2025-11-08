using Content.Client.Administration.UI.CustomControls;
using Content.Client.Guidebook.Richtext;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Content.Client._WL.Guidebook.RichText.InfoControls;

[UsedImplicitly]
public abstract class BaseInfoControl : PanelContainer, IDocumentTag
{
    protected abstract ResPath InfoTexturePath { get; }
    protected abstract Color DefaultControlColor { get; }
    protected abstract LocId TitleLocalizationKey { get; }
    protected virtual float TextureInterpolateBetweenFactor => 0.2f;
    protected virtual Vector2 TextureScale => new(0.5f);

    private PanelContainer? _rightFooterPanel;
    private Control? _mainBox;
    private bool _suppressChildAdded;

    public virtual bool TryParseTag(Dictionary<string, string> args, [NotNullWhen(true)] out Control? control)
    {
        HorizontalExpand = true;

        _suppressChildAdded = true;

        var controlColor = DefaultControlColor;
        if (args.TryGetValue("Color", out var color))
            controlColor = Color.FromHex(color);

        _mainBox = new BoxContainer()
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            Margin = new Thickness(10, 10, 10, 15),
        };

        var leftSeparator = new VSeparator(controlColor);
        leftSeparator.MinSize = new Vector2(4, 5);

        var rightContentBox = new BoxContainer()
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true
        };

        var rightContentTitlePanel = new PanelContainer()
        {
            HorizontalExpand = true,
        };

        var rightContentTitleBackground = new PanelContainer()
        {
            HorizontalExpand = true,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = controlColor.WithAlpha(0.5f),
            }
        };

        var rightContentTitleBox = new BoxContainer()
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            Margin = new Thickness(5, 0, 0, 0)
        };

        var infoTexture = new TextureButton
        {
            Modulate = Color.InterpolateBetween(controlColor, Color.White, TextureInterpolateBetweenFactor),
            HorizontalAlignment = HAlignment.Left,
            VerticalAlignment = VAlignment.Center,
            TexturePath = InfoTexturePath.CanonPath,
            Scale = TextureScale,
            Margin = new Thickness(0, 3)
        };

        var rightContentTitleLabel = new RichTextLabel()
        {
            Text = Loc.GetString(TitleLocalizationKey),
            HorizontalAlignment = HAlignment.Left,
            VerticalAlignment = VAlignment.Center,
            Margin = new Thickness(6, 0)
        };

        _rightFooterPanel = new PanelContainer()
        {
            HorizontalExpand = true,
            Margin = new Thickness(5, 1, 1, 1)
        };

        rightContentTitleBox.AddChild(infoTexture);
        rightContentTitleBox.AddChild(rightContentTitleLabel);

        rightContentTitlePanel.AddChild(rightContentTitleBackground);
        rightContentTitlePanel.AddChild(rightContentTitleBox);

        rightContentBox.AddChild(rightContentTitlePanel);
        rightContentBox.AddChild(_rightFooterPanel);

        _mainBox.AddChild(leftSeparator);
        _mainBox.AddChild(rightContentBox);

        AddChild(_mainBox);

        _suppressChildAdded = false;

        control = this;
        return true;
    }

    protected override void ChildAdded(Control newChild)
    {
        base.ChildAdded(newChild);

        if (_suppressChildAdded)
            return;

        if (newChild == _mainBox)
            return;

        if (_rightFooterPanel == null)
            return;

        newChild.Orphan();
        _rightFooterPanel.AddChild(newChild);
    }
}
