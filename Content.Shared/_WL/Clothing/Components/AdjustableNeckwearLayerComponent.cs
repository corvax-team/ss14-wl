using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Clothing.Components;

/// <summary>
/// Allows neckwear to be drawn either below or above outer clothing.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class AdjustableNeckwearLayerComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool AboveOuterClothing;

    [DataField]
    public string InsertionSlot = "outerClothing";

    [DataField]
    public string EquippedSlot = "neck";

    [DataField]
    public SpriteSpecifier VerbIcon = new SpriteSpecifier.Texture(
        new ResPath("/Textures/Interface/VerbIcons/outfit.svg.192dpi.png"));
}
