using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SwimmableMapComponent : Component
{
    [DataField, AutoNetworkedField]
    public float WaterResistance = 4f;
}
