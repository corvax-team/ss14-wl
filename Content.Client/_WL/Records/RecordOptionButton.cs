using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;

namespace Content.Client._WL.Records;

/// <summary>
/// Keeps record selectors compact and their popup entries readable.
/// </summary>
public sealed class RecordOptionButton : OptionButton
{
    private const float PopupItemHeight = 30;
    private const float PopupMaxHeight = 300;

    private bool _popupClosedOnButton;
    private bool _closingFromToggle;
    private readonly Dictionary<int, Texture> _icons = new();
    private readonly TextureRect _selectedIcon;
    private Texture? _pendingIcon;

    public RecordOptionButton()
    {
        OptionsScroll.MaxHeight = PopupItemHeight;
        OptionsScroll.MaxWidth = 600;
        _selectedIcon = new TextureRect
        {
            MinSize = new Vector2(16, 16),
            Margin = new Thickness(4, 0, 5, 0),
            VerticalAlignment = VAlignment.Center,
            Visible = false,
        };

        if (Children.FirstOrDefault() is BoxContainer selectedContent)
        {
            if (selectedContent.Children.OfType<Label>().FirstOrDefault() is { } selectedLabel)
            {
                selectedLabel.Align = Label.AlignMode.Left;
                selectedLabel.ClipText = true;
            }

            selectedContent.AddChild(_selectedIcon);
            _selectedIcon.SetPositionInParent(0);
        }

        if (OptionsScroll.Parent is Popup popup)
        {
            popup.OnPopupHide += () =>
            {
                if (_closingFromToggle)
                    return;

                var pointer = UserInterfaceManager.MousePositionScaled.Position - GlobalPosition;
                _popupClosedOnButton = HasPoint(pointer);
            };
        }

        OnPressed += _ => CloseReopenedPopup();
        OnItemSelected += args => UpdateSelectedIcon(args.Id);
    }

    public new void AddItem(string label, int? id = null)
    {
        base.AddItem(label, id);
        UpdatePopupHeight();
    }

    public new void AddItem(Texture icon, string label, int? id = null)
    {
        var itemId = id ?? ItemCount;
        _pendingIcon = icon;
        base.AddItem(label, itemId);
        _pendingIcon = null;
        _icons[itemId] = icon;
        UpdatePopupHeight();

        if (ItemCount == 1)
            UpdateSelectedIcon(itemId);
    }

    public new void SelectId(int id)
    {
        base.SelectId(id);
        UpdateSelectedIcon(id);
    }

    public new void Clear()
    {
        base.Clear();
        _icons.Clear();
        _selectedIcon.Texture = null;
        _selectedIcon.Visible = false;
        UpdatePopupHeight();
    }

    public override void ButtonOverride(Button button)
    {
        button.HorizontalExpand = true;
        button.TextAlign = Label.AlignMode.Left;

        if (_pendingIcon == null)
            return;

        button.RemoveChild(button.Label);
        button.AddChild(new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            Margin = new Thickness(5, 0),
            SeparationOverride = 6,
            Children =
            {
                new TextureRect
                {
                    Texture = _pendingIcon,
                    MinSize = new Vector2(16, 16),
                    VerticalAlignment = VAlignment.Center,
                },
                button.Label,
            },
        });
    }

    private void UpdateSelectedIcon(int id)
    {
        if (!_icons.TryGetValue(id, out var icon))
        {
            _selectedIcon.Texture = null;
            _selectedIcon.Visible = false;
            return;
        }

        _selectedIcon.Texture = icon;
        _selectedIcon.Visible = true;
    }

    private void UpdatePopupHeight()
    {
        OptionsScroll.MaxHeight = Math.Min(PopupMaxHeight, Math.Max(1, ItemCount) * PopupItemHeight);
    }

    private void CloseReopenedPopup()
    {
        if (!_popupClosedOnButton || OptionsScroll.Parent is not Popup popup)
            return;

        _popupClosedOnButton = false;
        _closingFromToggle = true;
        popup.Close();
        _closingFromToggle = false;
    }
}
