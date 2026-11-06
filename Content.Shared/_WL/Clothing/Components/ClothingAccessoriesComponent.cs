using Robust.Shared.GameStates;

namespace Content.Shared._WL.Clothing.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ClothingAccessoriesComponent : Component
{
    [DataField]
    public string Slot = "JumpsuitAccessories";
}
