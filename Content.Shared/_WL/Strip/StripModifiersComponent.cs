using Robust.Shared.GameStates;

namespace Content.Shared._WL.Strip.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial  class StripModifiersComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan StripTimeReduction = TimeSpan.FromSeconds(0.5f);

    [DataField, AutoNetworkedField]
    public float StripTimeMultiplier = 1f;
}
