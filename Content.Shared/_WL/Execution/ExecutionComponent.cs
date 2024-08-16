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
    public float DoAfterDurationIfVictumResist = 10f;

    [DataField, AutoNetworkedField]
    public float DamageModifier = 9f;

    // Not networked because this is transient inside of a tick.
    /// <summary>
    /// True if it is currently executing for handlers.
    /// </summary>
    [DataField]
    public bool Executing = false;

    [DataField]
    public string InternalMeleeExecutionMessage = "execution-popup-melee-initial-internal";
    [DataField]
    public string ExternalMeleeExecutionMessage = "execution-popup-melee-initial-external";
    //[DataField]
    //public string CompleteInternalMeleeExecutionMessage = "execution-popup-melee-complete-internal";
    //[DataField]
    //public string CompleteExternalMeleeExecutionMessage = "execution-popup-melee-complete-external";
    [DataField]
    public string InternalGunExecutionMessage = "execution-popup-gun-initial-internal";
    [DataField]
    public string tExternalGunExecutionMessage = "execution-popup-gun-initial-external";
    //[DataField]
    //public string CompleteInternalGunExecutionMessage = "execution-popup-gun-complete-internal";
    //[DataField]
    //public string CompleteExternalGunExecutionMessage = "execution-popup-gun-complete-external";
    [DataField]
    public string InternalGunSuicideMessage = "suicide-popup-gun-initial-internal";
    [DataField]
    public string ExternalGunSuicideMessage = "suicide-popup-gun-initial-external";
    //[DataField]
    //public string  = "suicide-popup-gun-complete-internal";
    //[DataField]
    //public string  = "suicide-popup-gun-complete-external";
    [DataField]
    public string InternalMeleeSuicideMessage = "suicide-popup-melee-initial-internal";
    [DataField]
    public string ExternalMeleeSuicideMessage = "suicide-popup-melee-initial-external";
    //[DataField]
    //public string  = "suicide-popup-melee-complete-internal";
    //[DataField]
    //public string  = "suicide-popup-melee-complete-external";

}
