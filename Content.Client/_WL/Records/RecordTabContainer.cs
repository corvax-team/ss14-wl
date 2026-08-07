using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;

namespace Content.Client._WL.Records;

/// <summary>
/// Record tabs use the accent of the selected record type without changing the neutral SS14 background.
/// </summary>
public sealed class RecordTabContainer : TabContainer
{
    public RecordTabContainer()
    {
        TabFontColorInactiveOverride = Color.FromHex("#8A8A92");
        OnTabChanged += _ => UpdateTheme();
        UpdateTheme();
    }

    public static string PaddedTitle(string title) => $"\u00A0\u00A0{title}\u00A0\u00A0";

    public void UpdateTheme()
    {
        var kind = CurrentTab switch
        {
            0 => RecordViewKind.Medical,
            1 => RecordViewKind.Security,
            _ => RecordViewKind.Employment,
        };
        var accent = RecordCardView.Accent(kind);
        TabFontColorOverride = accent;
        PanelStyleBoxOverride = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#151517"),
            BorderColor = accent.WithAlpha(0.55f),
            BorderThickness = new Thickness(1),
            ContentMarginTopOverride = 8,
        };
    }
}
