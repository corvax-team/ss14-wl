using Content.Shared.Damage;
using Robust.Shared.GameObjects;

namespace Content.Shared._WL.Weapons.Melee.Events;

/// <summary>
/// Raised on the target after melee damage modifiers are calculated and before damage is applied.
/// </summary>
[ByRefEvent]
public record struct BeforeMeleeDamageEvent(EntityUid Target, EntityUid User, EntityUid Weapon, DamageSpecifier Damage);
