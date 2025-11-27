using JetBrains.Annotations;
using Robust.Shared.Utility;

namespace Content.Client._WL.Guidebook.RichText.InfoControls;

[UsedImplicitly]
public sealed class Warning : BaseInfoControl
{
    protected override ResPath InfoTexturePath => new("/Textures/Interface/info.svg.192dpi.png");
    protected override Color DefaultControlColor => Color.FromHex("#914416");
    protected override LocId TitleLocalizationKey => "guidebook-info-control-warning";
}
