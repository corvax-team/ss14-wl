using Content.Client.Stylesheets.Palette;

namespace Content.Client.Stylesheets.Stylesheets;

public partial class NanotrasenStylesheet
{
    //WL-Change-start
    public override ColorPalette PrimaryPalette => Palettes.WL2;
    public override ColorPalette SecondaryPalette => Palettes.WL2;
    public override ColorPalette PositivePalette => Palettes.WL1;
    public override ColorPalette NegativePalette => Palettes.WL1;
    public override ColorPalette HighlightPalette => Palettes.WL1;
    //WL-Change-end
}
