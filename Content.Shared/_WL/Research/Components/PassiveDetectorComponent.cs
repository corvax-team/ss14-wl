using Content.Shared._WL.Research.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PassiveDetectorComponent : Component
{
    [DataField]
    public float Range = 10f;

    [DataField, AutoNetworkedField]
    public bool Active = true;
}
