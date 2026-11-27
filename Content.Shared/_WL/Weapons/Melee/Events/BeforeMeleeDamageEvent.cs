using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Shared._WL.Weapons.Melee.Events;

/// <summary>
/// Raised on the target after melee damage modifiers are calculated and before damage is applied.
/// A subscriber may set <see cref="Deferred"/>: the melee system then applies nothing now and the
/// subscriber becomes responsible for later calling
/// <c>SharedMeleeWeaponSystem.ApplyDeferredMeleeHit</c> with the final damage.
/// </summary>
[ByRefEvent]
public record struct BeforeMeleeDamageEvent(
    EntityUid Target,
    EntityUid User,
    EntityUid Weapon,
    DamageSpecifier Damage,
    bool ResistanceBypass,
    SoundSpecifier? HitSoundOverride,
    bool Heavy)
{
    public bool Deferred;
}
