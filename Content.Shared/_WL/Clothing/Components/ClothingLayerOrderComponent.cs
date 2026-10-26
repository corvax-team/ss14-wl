using Robust.Shared.GameStates;

namespace Content.Shared._WL.Clothing.Components;

/// <summary>
/// Draws equipped clothing above all visual layers belonging to the listed inventory slots.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ClothingLayerOrderComponent : Component
{
    [DataField]
    public List<string> Above = new();
}
