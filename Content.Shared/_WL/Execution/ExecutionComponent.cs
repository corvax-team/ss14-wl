using Robust.Shared.GameStates;

namespace Content.Shared._WL.Execution;

/// <summary>
/// Added to entities that can be used to execute another target.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ExecutionComponent : Component
{
    /// <summary>
    /// How long the execution duration lasts.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DoAfterDuration = 5f;

    [DataField, AutoNetworkedField]
    public float DamageModifier = 9f;

    // Not networked because this is transient inside of a tick.
    /// <summary>
    /// True if it is currently executing for handlers.
    /// </summary>
    [DataField]
    public bool Executing = false;

    [DataField]
    public string DefaultInternalMeleeExecutionMessage = "execution-popup-melee-initial-internal";
    [DataField]
    public string DefaultExternalMeleeExecutionMessage = "execution-popup-melee-initial-external";
    [DataField]
    public string DefaultCompleteInternalMeleeExecutionMessage = "execution-popup-melee-complete-internal";
    [DataField]
    public string DefaultCompleteExternalMeleeExecutionMessage = "execution-popup-melee-complete-external";
    [DataField]
    public string DefaultInternalGunExecutionMessage = "execution-popup-gun-initial-internal";
    [DataField]
    public string DefaultExternalGunExecutionMessage = "execution-popup-gun-initial-external";
    [DataField]
    public string DefaultCompleteInternalGunExecutionMessage = "execution-popup-gun-complete-internal";
    [DataField]
    public string DefaultCompleteExternalGunExecutionMessage = "execution-popup-gun-complete-external";
}
