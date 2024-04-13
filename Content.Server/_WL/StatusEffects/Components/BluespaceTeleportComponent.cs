using Robust.Shared.Map;

namespace Content.Server._WL.StatusEffects.Components;

[RegisterComponent]
public sealed partial class BluespaceTeleportComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityCoordinates Coordinates;
}
