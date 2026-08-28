using Robust.Shared.GameStates;

namespace Content.Shared._WL.Clothing.Components;

/// <summary>
/// Allows neckwear to be drawn either below or above outer clothing.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AdjustableNeckwearLayerComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool AboveOuterClothing;
}
