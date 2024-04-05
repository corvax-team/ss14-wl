using JetBrains.Annotations;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._WL.PulseDemon;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class ElectromagneticTamperAction
{
    /// <summary>
    /// Performs an action after applying the Electromagnetic Tamper Action, depending on the target.
    /// <see cref="PulseDemon.Systems.PulseDemonSystem.OnElectromagneticTamper">
    /// </summary>
    public abstract bool Action(ElectromagneticTamperActionArgs args);
}

public readonly record struct ElectromagneticTamperActionArgs(EntityUid DemonUid, EntityUid TargetUid, IEntityManager EntityManager, IRobustRandom RobustRandom, IGameTiming GameTiming);
