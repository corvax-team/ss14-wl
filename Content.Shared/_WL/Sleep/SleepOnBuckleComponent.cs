using Robust.Shared.GameStates;

namespace Content.Shared._WL.Sleep;

/// <summary>
/// Allows entities buckled to this strap to sleep.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SleepOnBuckleComponent : Component
{
    /// <summary>
    /// The sleep action entity that will be granted to buckled entities.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? SleepAction;

    /// <summary>
    /// Who unbuckle entity
    /// </summary>
    [DataField]
    public EntityUid? User;
}
