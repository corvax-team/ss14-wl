using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SwimmerComponent : Component
{
    [DataField, AutoNetworkedField]
    public float SwimSpeedModifier = 1f;

    [DataField, AutoNetworkedField]
    public float SwimAccelerationModifier = 0.5f;
}
