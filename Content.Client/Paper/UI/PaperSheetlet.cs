using Content.Client.Resources;
using Content.Client.Stylesheets;
using Content.Client.Stylesheets.SheetletConfigs;
using Content.Client.Stylesheets.Stylesheets;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Paper.UI;

[CommonSheetlet]
public sealed class PaperSheetlet : Sheetlet<NanotrasenStylesheet>
{
    public override StyleRule[] GetRules(NanotrasenStylesheet sheet, object config)
    {
        var windowCfg = (IWindowConfig)sheet;

        var paperBackground = ResCache.GetTexture("/Textures/Interface/Paper/paper_background_default.svg.96dpi.png")
            .IntoPatch(StyleBox.Margin.All, 16);
        var paperBox = new StyleBoxTexture
            { Texture = sheet.GetTexture(windowCfg.TransparentWindowBackgroundBorderedPath) };
        paperBox.SetPatchMargin(StyleBox.Margin.All, 2);

        var borderedTransparentTex = ResCache.GetTexture("/Textures/Interface/Nano/transparent_window_background_bordered.png");
        var borderedTransparentBackground = new StyleBoxTexture
        {
            Texture = borderedTransparentTex,
        };
        borderedTransparentBackground.SetPatchMargin(StyleBox.Margin.All, 2);
        var paperEditorBackground = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#eeeae1"),
            BorderColor = Color.FromHex("#afa99d80"),
            BorderThickness = new Thickness(1),
        };
        var paperAppendButton = new StyleBoxFlat
        {
            BackgroundColor = Color.Transparent,
            BorderThickness = new Thickness(0),
            ContentMarginLeftOverride = 3,
            ContentMarginRightOverride = 3,
            ContentMarginTopOverride = 1,
            ContentMarginBottomOverride = 1,
        };
        var paperAppendButtonHovered = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#8f806c12"),
            BorderThickness = new Thickness(0),
            ContentMarginLeftOverride = 3,
            ContentMarginRightOverride = 3,
            ContentMarginTopOverride = 1,
            ContentMarginBottomOverride = 1,
        };
        var paperAppendButtonPressed = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#8f806c20"),
            BorderThickness = new Thickness(0),
            ContentMarginLeftOverride = 3,
            ContentMarginRightOverride = 3,
            ContentMarginTopOverride = 1,
            ContentMarginBottomOverride = 1,
        };
        var paperLineEdit = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#eeeae1"),
            BorderColor = Color.FromHex("#afa99d70"),
            BorderThickness = new Thickness(0, 0, 0, 1),
            ContentMarginLeftOverride = 4,
            ContentMarginRightOverride = 4,
            ContentMarginTopOverride = 2,
            ContentMarginBottomOverride = 2,
        };
        var handwritingDefault = ResCache.GetFont("/Fonts/NotoSans/NotoSans-Regular.ttf", 12);
        var handwritingNeat = ResCache.GetFont("/Fonts/_WL/Handwriting/BadScript/BadScript-Regular.ttf", 14);
        var handwritingQuick = ResCache.GetFont("/Fonts/_WL/Handwriting/Caveat/Caveat.ttf", 16);
        var handwritingFormal = ResCache.GetFont("/Fonts/_WL/Handwriting/MarckScript/MarckScript-Regular.ttf", 15);
        var handwritingHeavy = ResCache.GetFont("/Fonts/_WL/Handwriting/Pangolin/Pangolin-Regular.ttf", 13);
        var handwritingMessy = ResCache.GetFont("/Fonts/_WL/Handwriting/Neucha/Neucha.ttf", 15);

        return
        [
            E<PanelContainer>().Identifier("PaperContainer").Panel(paperBox),
            E<PanelContainer>()
                .Identifier("PaperDefaultBorder")
                .Prop(PanelContainer.StylePropertyPanel, paperBackground),
            E<PanelContainer>()
                .Identifier("PaperEditBackground")
                .Prop(PanelContainer.StylePropertyPanel, borderedTransparentBackground),
            E<PanelContainer>()
                .Identifier("PaperEditorBackground")
                .Prop(PanelContainer.StylePropertyPanel, paperEditorBackground),
            E<TextEdit>()
                .Class("PaperTextEdit")
                .Prop("font-color", Color.FromHex("#24211e"))
                .Prop(TextEdit.StylePropertyCursorColor, Color.FromHex("#3b3630"))
                .Prop(TextEdit.StylePropertySelectionColor, Color.FromHex("#9f8f7048")),
            E<TextEdit>()
                .Class("PaperHandwritingDefault")
                .Prop("font", handwritingDefault)
                .Prop("font-color", Color.FromHex("#191919")),
            E<TextEdit>()
                .Class("PaperHandwritingNeat")
                .Prop("font", handwritingNeat)
                .Prop("font-color", Color.FromHex("#27231f")),
            E<TextEdit>()
                .Class("PaperHandwritingQuick")
                .Prop("font", handwritingQuick)
                .Prop("font-color", Color.FromHex("#46392b")),
            E<TextEdit>()
                .Class("PaperHandwritingFormal")
                .Prop("font", handwritingFormal)
                .Prop("font-color", Color.FromHex("#20242d")),
            E<TextEdit>()
                .Class("PaperHandwritingHeavy")
                .Prop("font", handwritingHeavy)
                .Prop("font-color", Color.FromHex("#1d1b19")),
            E<TextEdit>()
                .Class("PaperHandwritingMessy")
                .Prop("font", handwritingMessy)
                .Prop("font-color", Color.FromHex("#513427")),
            E<Button>()
                .Class(ContainerButton.StyleClassButton)
                .Class("PaperAppendButton")
                .Pseudo(ContainerButton.StylePseudoClassNormal)
                .Prop(ContainerButton.StylePropertyStyleBox, paperAppendButton),
            E<Button>()
                .Class(ContainerButton.StyleClassButton)
                .Class("PaperAppendButton")
                .Pseudo(ContainerButton.StylePseudoClassHover)
                .Prop(ContainerButton.StylePropertyStyleBox, paperAppendButtonHovered),
            E<Button>()
                .Class(ContainerButton.StyleClassButton)
                .Class("PaperAppendButton")
                .Pseudo(ContainerButton.StylePseudoClassPressed)
                .Prop(ContainerButton.StylePropertyStyleBox, paperAppendButtonPressed),
            E<Button>()
                .Class(ContainerButton.StyleClassButton)
                .Class("PaperAppendButton")
                .Pseudo(ContainerButton.StylePseudoClassDisabled)
                .Prop(ContainerButton.StylePropertyStyleBox, paperAppendButton),
            E<Button>()
                .Class(ContainerButton.StyleClassButton)
                .Class("PaperAppendButton")
                .ParentOf(E<Label>())
                .Prop(Label.StylePropertyFontColor, Color.FromHex("#3d3933")),
            E<LineEdit>()
                .Class("PaperLineEdit")
                .Prop(LineEdit.StylePropertyStyleBox, paperLineEdit)
                .Prop("font-color", Color.FromHex("#24211e")),
            E<LineEdit>()
                .Class("PaperLineEdit")
                .Pseudo(LineEdit.StylePseudoClassPlaceholder)
                .Prop("font-color", Color.FromHex("#756e6280")),
        ];
    }
}
