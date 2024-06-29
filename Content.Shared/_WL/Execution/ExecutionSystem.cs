using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.ActionBlocker;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Physics.Components;
using Robust.Shared.Map;
using Content.Shared.Projectiles;
using System.Threading;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Damage.Components;
using Content.Shared.Weapons.Ranged;

namespace Content.Shared._WL.Execution;

/// <summary>
///     Verb for violently murdering cuffed creatures.
/// </summary>
public sealed class ExecutionSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlockerSystem = default!;
    [Dependency] private readonly SharedGunSystem _gunSystem = default!;
    [Dependency] private readonly SharedCombatModeSystem _combatSystem = default!;
    [Dependency] private readonly SharedMeleeWeaponSystem _meleeSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecutionComponent, GetVerbsEvent<UtilityVerb>>(OnGetInteractionsVerbs);
        SubscribeLocalEvent<ExecutionComponent, ExecutionDoAfterEvent>(OnExecutionDoAfter);
        SubscribeLocalEvent<ExecutionComponent, GetMeleeDamageEvent>(OnGetMeleeDamage);
        SubscribeLocalEvent<ExecutionComponent, AmmoShotEvent>(OnAmmoShot);

    }

    private void OnGetInteractionsVerbs(EntityUid uid, ExecutionComponent comp, GetVerbsEvent<UtilityVerb> args)
    {
        if (args.Hands == null || args.Using == null || !args.CanAccess || !args.CanInteract)
            return;

        var attacker = args.User;
        var weapon = args.Using.Value;
        var victim = args.Target;

        if (!CanExecuteWithAny(victim, attacker))
            return;

        UtilityVerb verb = new()
        {
            Act = () => TryStartExecutionDoAfter(weapon, victim, attacker, comp),
            Impact = LogImpact.High,
            Text = Loc.GetString("execution-verb-name"),
            Message = Loc.GetString("execution-verb-message"),
        };

        args.Verbs.Add(verb);
    }

    private void TryStartExecutionDoAfter(EntityUid weapon, EntityUid victim, EntityUid attacker, ExecutionComponent comp)
    {
        if (!CanExecuteWithAny(victim, attacker))
            return;

        // TODO: This should just be on the weapons as a single execution message.
        var defaultExecutionInternal = comp.DefaultInternalMeleeExecutionMessage;
        var defaultExecutionExternal = comp.DefaultExternalMeleeExecutionMessage;

        if (HasComp<GunComponent>(weapon))
        {
            defaultExecutionExternal = comp.DefaultInternalGunExecutionMessage;
            defaultExecutionInternal = comp.DefaultExternalGunExecutionMessage;
        }

        var internalMsg = defaultExecutionInternal;
        var externalMsg = defaultExecutionExternal;
        ShowExecutionInternalPopup(internalMsg, attacker, victim, weapon);
        ShowExecutionExternalPopup(externalMsg, attacker, victim, weapon);

        var doAfter =
            new DoAfterArgs(EntityManager, attacker, comp.DoAfterDuration, new ExecutionDoAfterEvent(), weapon, target: victim, used: weapon)
            {
                BreakOnMove = true,
                BreakOnHandChange = true,
                BreakOnDamage = true,
                NeedHand = true
            };

        _doAfterSystem.TryStartDoAfter(doAfter);

    }

    private bool CanExecuteWithAny(EntityUid victim, EntityUid attacker)
    {
        // Use suicide.
        if (victim == attacker)
            return true;

        // No point executing someone if they can't take damage
        if (!TryComp<DamageableComponent>(victim, out _))
            return false;

        // You can't execute something that cannot die
        if (!TryComp<MobStateComponent>(victim, out var mobState))
            return false;

        // You're not allowed to execute dead people (no fun allowed)
        if (_mobStateSystem.IsDead(victim, mobState))
            return false;

        // You must be able to attack people to execute
        if (!_actionBlockerSystem.CanAttack(attacker, victim))
            return false;

        // The victim must be incapacitated to be executed
        if (victim != attacker && _actionBlockerSystem.CanInteract(victim, null))
            return false;

        if (Transform(attacker).Coordinates.InRange(_entityManager, _transformSystem, Transform(victim).Coordinates, 0.1f))
            return false;

        // All checks passed
        return true;
    }

    private void OnExecutionDoAfter(EntityUid uid, ExecutionComponent component, ExecutionDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Used == null || args.Target == null)
            return;

        var attacker = args.User;
        var victim = args.Target.Value;
        var weapon = args.Used.Value;

        if (!CanExecuteWithAny(victim, attacker))
            return;

        // This is needed so the melee system does not stop it.
        var prev = _combatSystem.IsInCombatMode(attacker);
        _combatSystem.SetInCombatMode(attacker, true);
        component.Executing = true;
        string? internalMsg = null;
        string? externalMsg = null;

        if (TryComp(uid, out MeleeWeaponComponent? melee))
        {
            _meleeSystem.AttemptLightAttack(attacker, weapon, melee, victim);
            internalMsg = component.DefaultCompleteInternalMeleeExecutionMessage;
            externalMsg = component.DefaultCompleteExternalMeleeExecutionMessage;
        }
        // TODO: Fcking shit code by GunSystem and HitscanPrototype
        else if (TryComp(uid, out HitscanBatteryAmmoProviderComponent? hitscanBatteryAmmo) &&
                 hitscanBatteryAmmo.Shots != 0 &&
                 TryComp(uid, out GunComponent? laserGun))
        {
            DamageSpecifier damageSpecifier = new DamageSpecifier()
            {
                DamageDict = new Dictionary<string, FixedPoint.FixedPoint2>()
                {
                    { "Heat", component.DamageModifier * 10f }
                }
            };

            if (attacker == victim)
            {
                _gunSystem.AttemptShoot(uid, laserGun);
                _damageable.TryChangeDamage(victim, damageSpecifier, origin: attacker);
            }
            else  //This number is set, because two Vectors with x0 y0 make Vector(NaN, Nan) direction, and this pass NaN value check... ¯\_(ツ)_/¯
            {
                _gunSystem.AttemptShoot(attacker, uid, laserGun, new EntityCoordinates(victim, 0.01984f, -0.00451f));
                _damageable.TryChangeDamage(victim, damageSpecifier, origin: attacker);
            }

            args.Handled = true;
        }
        else if (TryComp(uid, out GunComponent? gun))
        {
            var clumsyShot = false;

            // TODO: This should just be an event or something instead to get this.
            // TODO: Handle clumsy.
            // TODO: Make check on open rifleBolt, empty mag, empty revolver and etc

            if (clumsyShot)
            {
                internalMsg = "execution-popup-gun-empty";
                externalMsg = "execution-popup-gun-empty";
            }
            else
            {
                internalMsg = component.DefaultCompleteInternalGunExecutionMessage;
                externalMsg = component.DefaultCompleteExternalGunExecutionMessage;
            }

            if(attacker == victim)
            {
                _gunSystem.AttemptShoot(uid, gun);
            }
            else
            {
                //This number is set, because two Vectors with x0 y0 make Vector(NaN, Nan) direction, and this pass NaN value check... ¯\_(ツ)_/¯
                _gunSystem.AttemptShoot(attacker, uid, gun, new EntityCoordinates(victim, 0.01984f, -0.00451f));
            }
            args.Handled = true;
        }

        _combatSystem.SetInCombatMode(attacker, prev);
        component.Executing = false;
        args.Handled = true;

        if (internalMsg != null && externalMsg != null)
           {
        //     ShowExecutionInternalPopup(internalMsg, attacker, victim, uid);
        //     ShowExecutionExternalPopup(externalMsg, attacker, victim, uid);
           }
    }

    private void OnGetMeleeDamage(EntityUid uid, ExecutionComponent comp, ref GetMeleeDamageEvent args)
    {
        if (!TryComp<MeleeWeaponComponent>(uid, out var melee) ||
            !TryComp<ExecutionComponent>(uid, out var execComp) ||
            !execComp.Executing)
        {
            return;
        }

        args.Damage *= execComp.DamageModifier;
        comp.Executing = false;

    }

    private void OnAmmoShot(EntityUid uid, ExecutionComponent comp, ref AmmoShotEvent args)
    {
        if (!comp.Executing || args.FiredProjectiles.Count == 0)
        {
            return;
        }

        float staminaDamage = 0;
        if (TryComp(args.FiredProjectiles[0], out StaminaDamageOnCollideComponent? staminaDamageOnCollide))
        {
            staminaDamage = staminaDamageOnCollide.Damage * comp.DamageModifier;
            staminaDamageOnCollide.Damage *= staminaDamage;
        }

        if (TryComp(args.FiredProjectiles[0], out ProjectileComponent? projectile))
        {
            if(projectile.Damage.GetTotal() * comp.DamageModifier > staminaDamage)
                projectile.Damage *= comp.DamageModifier;

        }
        comp.Executing = false;
    }

    private void ShowExecutionInternalPopup(string locString,
        EntityUid attacker, EntityUid victim, EntityUid weapon, bool predict = true)
    {
        if (predict)
        {
            _popupSystem.PopupClient(
                Loc.GetString(locString, ("attacker", attacker), ("victim", victim), ("weapon", weapon)),
                attacker,
                attacker,
                PopupType.Medium
            );
        }
        else
        {
            _popupSystem.PopupEntity(
                Loc.GetString(locString, ("attacker", attacker), ("victim", victim), ("weapon", weapon)),
                attacker,
                Filter.Entities(attacker),
                true,
                PopupType.Medium
            );
        }

    }

    private void ShowExecutionExternalPopup(string locString, EntityUid attacker, EntityUid victim, EntityUid weapon)
    {
        _popupSystem.PopupEntity(
            Loc.GetString(locString, ("attacker", attacker), ("victim", victim), ("weapon", weapon)),
            attacker,
            Filter.PvsExcept(attacker),
            true,
            PopupType.MediumCaution
            );
    }
}
