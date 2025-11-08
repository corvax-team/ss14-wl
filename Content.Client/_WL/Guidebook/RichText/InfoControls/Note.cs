using JetBrains.Annotations;
using Robust.Shared.Utility;
using System.Numerics;

namespace Content.Client._WL.Guidebook.RichText.InfoControls;

[UsedImplicitly]
public sealed class Note : BaseInfoControl
{
    protected override ResPath InfoTexturePath => new("/Textures/Interface/Nano/help.png");

    protected override Color DefaultControlColor => Color.FromHex("#154884");

    protected override LocId TitleLocalizationKey => "guidebook-info-control-note";

    protected override Vector2 TextureScale => new(1.1f, 1.1f);
}
