using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
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
    private const float DefaultPopupMaxWidth = 600;
    private const float CompactPopupMaxWidth = 430;

    private bool _popupClosedOnButton;
    private bool _closingFromToggle;
    private bool _addingSectionHeader;
    private string? _pendingSectionHeaderLabel;
    private int _nextSectionHeaderId = -1;
    private bool _compactItems;
    private readonly Dictionary<int, Texture> _icons = new();
    private readonly Dictionary<int, string> _labels = new();
    private readonly TextureRect _selectedIcon;
    private readonly BoxContainer? _selectedContent;
    private readonly Label? _defaultSelectedLabel;
    private EllipsisLabel? _compactSelectedLabel;
    private Texture? _pendingIcon;
    private string? _compactToolTip;

    public bool CompactItems
    {
        get => _compactItems;
        set
        {
            if (_compactItems == value)
                return;

            if (!value)
                ClearCompactToolTip();

            _compactItems = value;
            OptionsScroll.MaxWidth = value ? CompactPopupMaxWidth : DefaultPopupMaxWidth;
            OptionsScroll.HScrollEnabled = !value;
            UpdateSelectedLabelMode();

            if (value)
                SetCompactToolTip(_labels.GetValueOrDefault(SelectedId));
        }
    }

    public RecordOptionButton()
    {
        OptionsScroll.MaxHeight = PopupItemHeight;
        OptionsScroll.MaxWidth = DefaultPopupMaxWidth;
        _selectedIcon = new TextureRect
        {
            MinSize = new Vector2(16, 16),
            Margin = new Thickness(4, 0, 5, 0),
            VerticalAlignment = VAlignment.Center,
            Visible = false,
        };

        if (Children.FirstOrDefault() is BoxContainer selectedContent)
        {
            _selectedContent = selectedContent;
            if (selectedContent.Children.OfType<Label>().FirstOrDefault() is { } selectedLabel)
            {
                _defaultSelectedLabel = selectedLabel;
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
        OnItemSelected += args => UpdateSelectedItem(args.Id);
    }

    public new void AddItem(string label, int? id = null)
    {
        var itemId = id ?? ItemCount;
        base.AddItem(label, itemId);
        _labels[itemId] = label;
        UpdatePopupHeight();

        if (ItemCount == 1)
            UpdateSelectedItem(itemId);
    }

    public new void AddItem(Texture icon, string label, int? id = null)
    {
        var itemId = id ?? ItemCount;
        _pendingIcon = icon;
        base.AddItem(label, itemId);
        _pendingIcon = null;
        _icons[itemId] = icon;
        _labels[itemId] = label;
        UpdatePopupHeight();

        if (ItemCount == 1)
            UpdateSelectedItem(itemId);
    }

    public void AddSectionHeader(string label, string filterText)
    {
        _addingSectionHeader = true;
        _pendingSectionHeaderLabel = label;
        base.AddItem(filterText, _nextSectionHeaderId--);
        _pendingSectionHeaderLabel = null;
        _addingSectionHeader = false;
        SetItemDisabled(ItemCount - 1, true);
        UpdatePopupHeight();
    }

    public new void SelectId(int id)
    {
        base.SelectId(id);
        UpdateSelectedItem(id);
    }

    public new void Clear()
    {
        base.Clear();
        _icons.Clear();
        _labels.Clear();
        _nextSectionHeaderId = -1;
        _selectedIcon.Texture = null;
        _selectedIcon.Visible = false;
        if (_compactSelectedLabel != null)
            _compactSelectedLabel.FullText = string.Empty;
        ClearCompactToolTip();
        UpdatePopupHeight();
    }

    public override void ButtonOverride(Button button)
    {
        if (!_compactItems && !_addingSectionHeader)
        {
            ConfigureDefaultItem(button);
            return;
        }

        var fullText = _pendingSectionHeaderLabel ?? button.Text ?? string.Empty;
        button.HorizontalExpand = true;
        button.ToolTip = _addingSectionHeader ? null : fullText;
        button.RemoveChild(button.Label);

        var visibleLabel = new EllipsisLabel
        {
            FullText = fullText,
            HorizontalExpand = true,
            Align = Label.AlignMode.Left,
            Margin = new Thickness(6, 0),
            FontColorOverride = _addingSectionHeader ? Color.FromHex("#B8B8B8") : null,
        };

        if (_addingSectionHeader)
        {
            button.MinHeight = 26;
            button.StyleBoxOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#1B1B1E"),
                BorderColor = Color.FromHex("#4A4A4F"),
                BorderThickness = new Thickness(0, 0, 0, 1),
                ContentMarginLeftOverride = 4,
            };
            button.AddChild(visibleLabel);
            return;
        }

        if (_pendingIcon == null)
        {
            button.AddChild(visibleLabel);
            return;
        }

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
                visibleLabel,
            },
        });
    }

    private void ConfigureDefaultItem(Button button)
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

    private void UpdateSelectedLabelMode()
    {
        if (_selectedContent == null || _defaultSelectedLabel == null)
            return;

        _defaultSelectedLabel.Visible = !_compactItems;
        if (!_compactItems)
        {
            if (_compactSelectedLabel != null)
                _compactSelectedLabel.Visible = false;
            return;
        }

        _compactSelectedLabel ??= new EllipsisLabel
        {
            HorizontalExpand = true,
            Align = Label.AlignMode.Left,
        };
        if (_compactSelectedLabel.Parent == null)
        {
            _selectedContent.AddChild(_compactSelectedLabel);
            _compactSelectedLabel.SetPositionInParent(1);
        }

        _compactSelectedLabel.Visible = true;
        _compactSelectedLabel.FullText = _labels.GetValueOrDefault(SelectedId) ?? string.Empty;
    }

    private sealed class EllipsisLabel : Label
    {
        private const string Ellipsis = "...";
        private string _fullText = string.Empty;

        public string FullText
        {
            get => _fullText;
            set
            {
                _fullText = value;
                Text = value;
                UpdateVisibleText();
            }
        }

        public EllipsisLabel()
        {
            ClipText = true;
        }

        protected override void Resized()
        {
            base.Resized();
            UpdateVisibleText();
        }

        protected override void StylePropertiesChanged()
        {
            base.StylePropertiesChanged();
            UpdateVisibleText();
        }

        protected override void UIScaleChanged()
        {
            base.UIScaleChanged();
            UpdateVisibleText();
        }

        private void UpdateVisibleText()
        {
            if (PixelSize.X <= 0)
                return;

            var font = GetActualFont();
            if (MeasureText(font, _fullText) <= PixelSize.X)
            {
                Text = _fullText;
                return;
            }

            var availableWidth = PixelSize.X - MeasureText(font, Ellipsis);
            var width = 0;
            var builder = new StringBuilder();
            foreach (var rune in _fullText.EnumerateRunes())
            {
                if (!font.TryGetCharMetrics(rune, UIScale, out var metrics))
                    break;

                if (width + metrics.Advance > availableWidth)
                    break;

                builder.Append(rune.ToString());
                width += metrics.Advance;
            }

            Text = builder.ToString().TrimEnd() + Ellipsis;
        }

        private int MeasureText(Font font, string text)
        {
            var width = 0;
            foreach (var rune in text.EnumerateRunes())
            {
                if (font.TryGetCharMetrics(rune, UIScale, out var metrics))
                    width += metrics.Advance;
            }

            return width;
        }

        private Font GetActualFont()
        {
            if (FontOverride != null)
                return FontOverride;

            return TryGetStyleProperty<Font>(StylePropertyFont, out var font)
                ? font
                : UserInterfaceManager.ThemeDefaults.LabelFont;
        }
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

    private void UpdateSelectedItem(int id)
    {
        UpdateSelectedIcon(id);
        var label = _labels.GetValueOrDefault(id);
        if (_compactSelectedLabel != null)
            _compactSelectedLabel.FullText = label ?? string.Empty;
        if (_compactItems)
            SetCompactToolTip(label);
    }

    private void SetCompactToolTip(string? value)
    {
        if (ToolTip == _compactToolTip)
            ToolTip = value;

        _compactToolTip = value;
    }

    private void ClearCompactToolTip()
    {
        if (ToolTip == _compactToolTip)
            ToolTip = null;

        _compactToolTip = null;
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
