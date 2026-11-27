using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Shared._Ship14.CorticalBorer.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CorticalBorerHostComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Borer;

    [DataField, AutoNetworkedField]
    public EntityUid? TrappedMind;

    [DataField]
    public EntityUid? ControllingMind;

    [DataField]
    public EntityUid? OriginalMind;

    [ViewVariables]
    public ContainerSlot BorerContainer = default!;

    [ViewVariables]
    public float NutritionDecayAccumulator;
}
