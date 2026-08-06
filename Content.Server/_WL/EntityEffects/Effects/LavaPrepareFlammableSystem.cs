using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds;
using Content.Server.Destructible.Thresholds.Behaviors;
using Content.Shared._WL.EntityEffects.Effects;
using Content.Shared.Atmos.Components;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Destructible;
using Content.Shared.Destructible.Thresholds;
using Content.Shared.Destructible.Thresholds.Triggers;
using Content.Shared.EntityEffects;
using Content.Shared.Item;
using Robust.Shared.Prototypes;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Tag;

namespace Content.Server._WL.EntityEffects.Effects;

public sealed partial class LavaPrepareFlammableEntityEffectSystem
    : EntityEffectSystem<ItemComponent, LavaPrepareFlammable>
{
    [Dependency] private IEntityManager _entMan = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private TagSystem _tag = default!;

    protected override void Effect(
        Entity<ItemComponent> entity,
        ref EntityEffectEvent<LavaPrepareFlammable> args)
    {
        var uid = entity.Owner;

        // Blacklist tags
        if (_tag.HasAnyTag(uid, "HighRiskItem", "FireResistant"))
            return;

        if (_entMan.HasComponent<FlammableComponent>(uid))
            return;

        // AppearanceComponent block
        if (!_entMan.HasComponent<AppearanceComponent>(uid))
            _entMan.EnsureComponent<AppearanceComponent>(uid);

        // ReactiveComponent block
        if (!_entMan.HasComponent<ReactiveComponent>(uid))
        {
            var reactive = _entMan.EnsureComponent<ReactiveComponent>(uid);

            reactive.ReactiveGroups ??= new Dictionary<string, HashSet<ReactionMethod>>();

            if (!reactive.ReactiveGroups.ContainsKey("Extinguish"))
                reactive.ReactiveGroups["Extinguish"] = new HashSet<ReactionMethod>();

            reactive.ReactiveGroups["Extinguish"].Add(ReactionMethod.Touch);
        }

        // InjurableComponent block
        if (!_entMan.HasComponent<InjurableComponent>(uid))
            _entMan.EnsureComponent<InjurableComponent>(uid);

        // DamageableComponent block
        if (!_entMan.HasComponent<DamageableComponent>(uid))
        {
            _entMan.EnsureComponent<DamageableComponent>(uid);
            _entMan.EnsureComponent<DamageableComponent>(uid, out var damageable);
            _damageable.SetDamageModifierSetId((uid, damageable), new ProtoId<DamageModifierSetPrototype>("Wood"));
        }

        // FlammableComponent block
        var flammable = _entMan.EnsureComponent<FlammableComponent>(uid);

        flammable.AlwaysCombustible = true;
        flammable.CanExtinguish = true;

        var fireDamage = new DamageSpecifier();

        fireDamage.DamageDict.Add(
            new ProtoId<DamageTypePrototype>("Heat"),
            1
        );

        flammable.Damage = fireDamage;

        // DestructibleComponent block
        if (!_entMan.HasComponent<DestructibleComponent>(uid))
        {
            var destructible = _entMan.EnsureComponent<DestructibleComponent>(uid);
            destructible.Thresholds ??= new List<DamageThreshold>();
            destructible.Thresholds.Add(new DamageThreshold
            {
                Trigger = new DamageTrigger { Damage = 40 },
                Behaviors = new List<IThresholdBehavior>
                {
                    new SpawnEntitiesBehavior
                    {
                        Spawn = new Dictionary<EntProtoId, MinMax>
                        {
                            ["Ash"] = new MinMax(1, 1)
                        }
                    },
                    new DoActsBehavior
                    {
                        Acts = ThresholdActs.Destruction
                    }
                }
            });
        }
    }
}
