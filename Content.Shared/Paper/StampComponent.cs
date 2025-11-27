using Robust.Shared.Serialization;
using Robust.Shared.Audio;
using Robust.Shared.Utility;
using System.Numerics;

namespace Content.Shared.Paper;

/// <summary>
///     Set of required information to draw a stamp in UIs, where
///     representing the state of the stamp at the point in time
///     when it was applied to a paper. These fields mirror the
///     equivalent in the component.
/// </summary>
[DataDefinition, Serializable, NetSerializable]
public partial struct StampDisplayInfo
{
    [DataField]
    public string StampedName;

    [DataField]
    public Color StampedColor;

    // WL-Changes-start
    [DataField]
    public SpriteSpecifier StampedTexture = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Paper/paper_stamp_border.svg.96dpi.png"));

    [DataField]
    public bool StampedTextureIsBorder = true;

    [DataField]
    public Vector2? OverrideTextureSize = null;
    // WL-Changes-end
};

[RegisterComponent]
public sealed partial class StampComponent : Component
{
    /// <summary>
    ///     The loc string name that will be stamped to the piece of paper on examine.
    /// </summary>
    [DataField]
    public string StampedName { get; set; } = "stamp-component-stamped-name-default";

    /// <summary>
    ///     The sprite state of the stamp to display on the paper from paper Sprite path.
    /// </summary>
    [DataField]
    public string StampState { get; set; } = "paper_stamp-generic";

    /// <summary>
    /// The color of the ink used by the stamp in UIs
    /// </summary>
    [DataField]
    public Color StampedColor = Color.FromHex("#BB3232"); // StyleNano.DangerousRedFore

    /// <summary>
    /// The sound when stamp stamped
    /// </summary>
    [DataField]
    public SoundSpecifier? Sound = null;

    // WL-Changes-start
    /// <summary>
    /// The texture to use when rendering this stamp on paper.
    /// </summary>
    [DataField]
    public SpriteSpecifier StampTexture = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Paper/paper_stamp_border.svg.96dpi.png"));

    /// <summary>
    /// Whether the stamp texture should be used as a border around text, or contains the text itself
    /// </summary>
    [DataField]
    public bool IsBorderTexture = true;

    [DataField]
    public Vector2? OverrideTextureSize = null;
    // WL-Changes-end
}
