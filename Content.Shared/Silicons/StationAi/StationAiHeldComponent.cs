using Robust.Shared.GameStates;

namespace Content.Shared.Silicons.StationAi;

/// <summary>
/// Indicates this entity is currently held inside of a station AI core.
/// </summary>
[RegisterComponent, NetworkedComponent]
// WL-Changes-start
public sealed partial class StationAiHeldComponent : Component
{
    [DataField]
    public EntityUid? CurrentConnectedEntity;
}
// WL-Changes-end
