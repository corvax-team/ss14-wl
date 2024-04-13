using JetBrains.Annotations;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Slimes;

/// <summary>
/// The commands that slime executes. Check SlimeSystem.
/// </summary>
[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
[Serializable, NetSerializable]
public abstract partial class SlimeCommand
{
    public abstract bool Command(SlimeCommandArgs args);
}
public readonly record struct SlimeCommandArgs(EntityUid Slime, EntityUid Source, string[] Keywords, string? ChatMessage, IEntityManager EntityManager);
