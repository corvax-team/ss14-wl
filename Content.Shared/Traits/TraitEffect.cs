using Content.Shared.Preferences;
using JetBrains.Annotations;
using Robust.Shared.Player;
using Robust.Shared.Serialization;

namespace Content.Shared.Traits
{
    [ImplicitDataDefinitionForInheritors]
    [MeansImplicitUse]
    [Serializable, NetSerializable]
    public abstract partial class TraitEffect
    {
        public abstract void Effect(TraitEffectArgs effectArgs);
    }

    public readonly record struct TraitEffectArgs(
        ICommonSession PlayerSession,
        string? JobId,
        EntityUid Station,
        HumanoidCharacterProfile Profile,
        bool LateJoin,
        // ММ, хрень
        HandledEntityEventArgs? BeforeSpawnEvent = null,
        EntityEventArgs? AfterSpawnEvent = null);
}
