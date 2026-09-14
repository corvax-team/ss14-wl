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
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private TagSystem _tag = default!;

    protected override void Effect(
        Entity<ItemComponent> entity,
        ref EntityEffectEvent<LavaPrepareFlammable> args)
    {
        var uid = entity.Owner;

        // Blacklist tags
        const string highRiskItemTag = "HighRiskItem";
        const string fireResistantTag = "FireResistant";
        if (_tag.HasAnyTag(uid, highRiskItemTag, fireResistantTag))
            return;

        if (HasComp<FlammableComponent>(uid))
            return;

        // AppearanceComponent block
        if (!HasComp<AppearanceComponent>(uid))
            EnsureComp<AppearanceComponent>(uid);

        // ReactiveComponent block
        if (!HasComp<ReactiveComponent>(uid))
        {
            var reactive = EnsureComp<ReactiveComponent>(uid);

            reactive.ReactiveGroups ??= new Dictionary<string, HashSet<ReactionMethod>>();

            if (!reactive.ReactiveGroups.ContainsKey("Extinguish"))
                reactive.ReactiveGroups["Extinguish"] = new HashSet<ReactionMethod>();

            reactive.ReactiveGroups["Extinguish"].Add(ReactionMethod.Touch);
        }

        // InjurableComponent block
        if (!HasComp<InjurableComponent>(uid))
            EnsureComp<InjurableComponent>(uid);

        // DamageableComponent block
        if (!HasComp<DamageableComponent>(uid))
        {
            EnsureComp<DamageableComponent>(uid);
            EnsureComp<DamageableComponent>(uid, out var damageable);
            _damageable.SetDamageModifierSetId((uid, damageable), new ProtoId<DamageModifierSetPrototype>("Wood"));
        }

        // FlammableComponent block
        var flammable = EnsureComp<FlammableComponent>(uid);

        flammable.AlwaysCombustible = true;
        flammable.CanExtinguish = true;

        var fireDamage = new DamageSpecifier();

        fireDamage.DamageDict.Add(
            new ProtoId<DamageTypePrototype>("Heat"),
            1
        );

        flammable.Damage = fireDamage;

        // DestructibleComponent block
        if (!HasComp<DestructibleComponent>(uid))
        {
            var destructible = EnsureComp<DestructibleComponent>(uid);
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
