using JetBrains.Annotations;

namespace Content.Shared._WL.PulseDemon;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class ElectromagneticTamperAction
{
    /// <summary>
    /// Performs an action after applying the Electromagnetic Tamper Action, depending on the target.
    /// see PulseDemonSystem.Abilities.OnElectromagneticTamperAction.
    /// </summary>
    public abstract bool Action(ElectromagneticTamperActionArgs args);
}

public readonly record struct ElectromagneticTamperActionArgs(EntityUid DemonUid, EntityUid TargetUid, IEntityManager EntityManager);
