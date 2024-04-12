using Content.Server.Emp;
using Content.Server.Explosion.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Shared.DoAfter;
using Content.Shared.Maps;
using Content.Shared._WL.PulseDemon;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using System.Linq;
using Content.Server.Power.NodeGroups;
using Content.Shared.Emag.Components;
using Content.Shared.Access.Components;
using Content.Server._WL.PulseDemon.Components;

namespace Content.Server._WL.PulseDemon.Systems;

public sealed partial class PulseDemonSystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly SharedPointLightSystem _light = default!;


    private const string PulseDemonExplosionType = "ElectricExplosion";

    private const string PulseDemonExplosiveParticlePrototype = "EffectSparks4";

    private const float PulseDemonDoAfterDistanceThresholdRange = 50000f;

    private void InitializeAbilities()
    {
        SubscribeLocalEvent<PulseDemonComponent, PulseDemonAbsorptionActionEvent>(OnAbsorption);
        SubscribeLocalEvent<PulseDemonComponent, PulseDemonAbsorptionDoAfterEvent>(OnAbsorptionDoAfter);

        SubscribeLocalEvent<PulseDemonComponent, PulseDemonApcHijackActionEvent>(OnApcHijack);
        SubscribeLocalEvent<PulseDemonComponent, PulseDemonApcHijackDoAfterEvent>(OnApcHijackDoAfter);

        SubscribeLocalEvent<PulseDemonComponent, PulseDemonHideActionEvent>(OnHide);
        SubscribeLocalEvent<PulseDemonComponent, PulseDemonHideDoAfterEvent>(OnHideDoAfter);

        SubscribeLocalEvent<PulseDemonComponent, PulseDemonSelfSustainingActionEvent>(OnSelfSustaining);
        SubscribeLocalEvent<PulseDemonComponent, PulseDemonSelfSustainingDoAfterEvent>(OnSelfSustainingDoAfter);

        SubscribeLocalEvent<PulseDemonComponent, PulseDemonOverloadMachineActionEvent>(OnOverload);
        SubscribeLocalEvent<ExplosiveComponent, PulseDemonOverloadMachineDoAfterEvent>(OnOverloadDoAfter);

        SubscribeLocalEvent<PulseDemonComponent, PulseDemonCableHopActionEvent>(OnCableHop);

        SubscribeLocalEvent<PulseDemonComponent, PulseDemonEmpActionEvent>(OnEmp);

        SubscribeLocalEvent<PulseDemonComponent, PulseDemonElectromagneticTamperActionEvent>(OnElectromagneticTamper);
    }

    #region Electromagnetic Tamper
    private void OnElectromagneticTamper(EntityUid uid, PulseDemonComponent comp, PulseDemonElectromagneticTamperActionEvent args)
    {
        if (args.Handled)
            return;

        if (comp.OnElectromagneticTamperActions == null)
            return;

        if (!TryComp<HijackedByPulseDemonComponent>(args.Target, out var hijackComp))
        {
            var message = Loc.GetString("pulse-demon-not-hijacked");
            _popup.PopupCursor(message, uid, Shared.Popups.PopupType.Medium);
            return;
        }

        if (hijackComp.Used)
        {
            var message = Loc.GetString("pulse-demon-already-hijacked");
            _popup.PopupCursor(message, uid, Shared.Popups.PopupType.Medium);
            return;
        }

        if (!TryComp<BatteryComponent>(args.Performer, out var pulseDemonBattery))
            return;

        foreach (var action in comp.OnElectromagneticTamperActions)
        {
            if (!action.Action(new ElectromagneticTamperActionArgs(uid, args.Target, EntityManager, _random, _gameTiming)))
                continue;

            CheckEnergyAndDealBatteryDamage(pulseDemonBattery, args.Cost);

            hijackComp.Used = true;
            args.Handled = true;

            break;
        }

        if (!TryComp<ApcPowerReceiverComponent>(args.Target, out var apcPowerComp) ||
            !apcPowerComp.Powered ||
            apcPowerComp.PowerDisabled)
            return;

        apcPowerComp.PowerDisabled = true;
    }
    #endregion

    #region Overload Machine
    private void OnOverload(EntityUid uid, PulseDemonComponent comp, PulseDemonOverloadMachineActionEvent args)
    {
        if (args.Handled)
            return;

        var pulseDemonBattery = Comp<BatteryComponent>(args.Performer);
        if (!CheckEnergyAndDealBatteryDamage(pulseDemonBattery, args.Cost))
            return;

        var targetCoords = Transform(args.Target).Coordinates;

        var explosiveParticle = EntityManager.SpawnEntity(PulseDemonExplosiveParticlePrototype, targetCoords);
        var explsoiveComp = AddComp<ExplosiveComponent>(explosiveParticle);

        explsoiveComp.ExplosionType = PulseDemonExplosionType;
        explsoiveComp.IntensitySlope = 1;
        explsoiveComp.TotalIntensity = _explosion.RadiusToIntensity(args.Radius, explsoiveComp.IntensitySlope, args.ExplosionForce);
        explsoiveComp.Exploded = false;
        explsoiveComp.CanCreateVacuum = false;

        var doAfter = new DoAfterArgs(EntityManager, explosiveParticle, args.ExplosionPreparation, new PulseDemonOverloadMachineDoAfterEvent(),
            explosiveParticle, explosiveParticle, null)
        {
            Hidden = true,
            BreakOnDamage = false,
            BreakOnMove = false,
            RequireCanInteract = false
        };

        args.Handled = _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnOverloadDoAfter(EntityUid uid, ExplosiveComponent comp, PulseDemonOverloadMachineDoAfterEvent args)
    {
        if (args.Args.Target == null)
            return;

        _explosion.TriggerExplosive((EntityUid) args.Args.Target, comp, true, comp.TotalIntensity);
    }
    #endregion

    #region Emp
    private void OnEmp(EntityUid uid, PulseDemonComponent comp, PulseDemonEmpActionEvent args)
    {
        if (args.Handled)
            return;

        var pulseDemonBattery = Comp<BatteryComponent>(args.Performer);
        if (!CheckEnergyAndDealBatteryDamage(pulseDemonBattery, args.Cost))
        {
            args.Handled = false;
            return;
        }

        _emp.EmpPulse(args.Target.ToMap(EntityManager), args.Radius, args.BatteryDamage, 5f);
        args.Handled = true;
    }
    #endregion

    #region Self Sustaining
    private void OnSelfSustaining(EntityUid uid, PulseDemonComponent comp, PulseDemonSelfSustainingActionEvent args)
    {
        if (args.Handled)
            return;

        var pulseDemonBattery = Comp<BatteryComponent>(args.Performer);
        if (!CheckEnergyAndDealBatteryDamage(pulseDemonBattery, args.Cost))
            return;

        SetCanExistOutsideCableValue(comp, true);

        var doAfter = new DoAfterArgs(EntityManager, args.Performer, args.TimeOutside, new PulseDemonSelfSustainingDoAfterEvent(), args.Performer, args.Performer, null)
        {
            Hidden = true,
            BreakOnDamage = false
        };

        args.Handled = _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnSelfSustainingDoAfter(EntityUid uid, PulseDemonComponent comp, PulseDemonSelfSustainingDoAfterEvent args)
    {
        SetCanExistOutsideCableValue(comp, false);
    }
    #endregion

    #region Hide
    private void OnHide(EntityUid uid, PulseDemonComponent comp, PulseDemonHideActionEvent args)
    {
        if (args.Handled)
            return;

        var pulseDemonBattery = Comp<BatteryComponent>(args.Performer);
        if (!CheckEnergyAndDealBatteryDamage(pulseDemonBattery, args.Cost))
            return;

        Hide(uid, true);
        comp.IsHiding = true;

        var doAfter = new DoAfterArgs(EntityManager, args.Performer, args.HidingTime, new PulseDemonHideDoAfterEvent(), args.Performer, args.Performer, null)
        {
            Hidden = true,
            BreakOnDamage = true,
            DistanceThreshold = PulseDemonDoAfterDistanceThresholdRange
        };

        args.Handled = _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnHideDoAfter(EntityUid uid, PulseDemonComponent comp, PulseDemonHideDoAfterEvent args)
    {
        Hide(uid, false);
        _light.SetEnabled(uid, true);
        comp.IsHiding = false;
    }

    private void Hide(EntityUid demonUid, bool hide)
    {
        if (!TryComp<FixturesComponent>(demonUid, out var fixtureComp))
            return;

        var fixtureId = fixtureComp.Fixtures.FirstOrNull();
        if (fixtureId == null)
            return;

        if (hide)
            _tag.AddTag(demonUid, "HideContextMenu");
        else _tag.RemoveTag(demonUid, "HideContextMenu");

        var collisionLayer = hide switch
        {
            true => 0,
            false => 65
        };

        _physics.SetCollisionLayer(demonUid, fixtureId.Value.Key, fixtureId.Value.Value, collisionLayer);

        _appearance.SetData(demonUid, PulseDemonState.IsHiding, hide);
        _light.SetEnabled(demonUid, false);
    }
    #endregion

    #region Apc Hijack
    private void OnApcHijack(EntityUid uid, PulseDemonComponent comp, PulseDemonApcHijackActionEvent args)
    {
        if (args.Handled)
            return;

        var pulseDemonBattery = Comp<BatteryComponent>(args.Performer);
        if (!CheckEnergyAndDealBatteryDamage(pulseDemonBattery, args.Cost))
            return;

        if (HasComp<HijackedByPulseDemonComponent>(args.Target))
        {
            var message = Loc.GetString("pulse-demon-already-hijacked");
            _popup.PopupCursor(message, uid, Shared.Popups.PopupType.Medium);
            return;
        }

        args.Handled = true;

        var doAfter = new DoAfterArgs(EntityManager, args.Performer, GetHijackTime(comp), new PulseDemonApcHijackDoAfterEvent(), args.Performer, args.Target, args.Performer)
        {
            BreakOnMove = true,
            BlockDuplicate = true,
            DuplicateCondition = DuplicateConditions.SameTool,
            RequireCanInteract = true,
            DistanceThreshold = PulseDemonDoAfterDistanceThresholdRange
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnApcHijackDoAfter(EntityUid uid, PulseDemonComponent comp, PulseDemonApcHijackDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        if (args.Target == null)
            return;

        if (!TryComp<ApcPowerProviderComponent>(args.Target.Value, out var apcProviderComp) ||
            apcProviderComp.Net == null ||
            !TryComp<AccessReaderComponent>(args.Target.Value, out var accessComp))
            return;

        var providers = ((ApcNet) apcProviderComp.Net).Providers;
        foreach (var provider in providers)
        {
            foreach (var receiver in provider.LinkedReceivers)
            {
                EnsureComp<HijackedByPulseDemonComponent>(receiver.Owner);
            }
        }

        EnsureComp<HijackedByPulseDemonComponent>(args.Target.Value);
        EnsureComp<EmaggedComponent>(args.Target.Value);

        accessComp.AccessKeys.Clear();
        accessComp.AccessLists.Clear();

        args.Handled = true;

        var targetCoords = Comp<TransformComponent>(args.Target.Value).Coordinates;
        EntityManager.SpawnEntity(_random.Pick(comp.ParticlesPrototypes), targetCoords);
    }
    #endregion

    #region Absorption
    private void OnAbsorption(EntityUid uid, PulseDemonComponent comp, PulseDemonAbsorptionActionEvent args)
    {
        if (args.Handled)
            return;

        if (comp.IsHiding)
        {
            OnHideDoAfter(uid, comp, new PulseDemonHideDoAfterEvent());
            return;
        }

        var targetBattery = Comp<BatteryComponent>(args.Target);
        var pulseDemonBattery = Comp<BatteryComponent>(args.Performer);

        if (!CheckTargetAndDemonEnergy(pulseDemonBattery, targetBattery))
            return;

        if (!CheckEnergyAndDealBatteryDamage(pulseDemonBattery, args.Cost))
            return;

        var doAfter = new DoAfterArgs(EntityManager, args.Performer, GetEfficiency(comp), new PulseDemonAbsorptionDoAfterEvent(), args.Performer, args.Target, args.Performer)
        {
            BreakOnMove = true,
            BlockDuplicate = true,
            BreakOnDamage = true,
            DuplicateCondition = DuplicateConditions.SameTool,
            DistanceThreshold = PulseDemonDoAfterDistanceThresholdRange
        };

        args.Handled = _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnAbsorptionDoAfter(EntityUid uid, PulseDemonComponent comp, PulseDemonAbsorptionDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        if (args.Target == null)
            return;

        var targetBattery = Comp<BatteryComponent>((EntityUid) args.Target);
        var pulseDemonBattery = Comp<BatteryComponent>(args.User);

        if (!CheckTargetAndDemonEnergy(pulseDemonBattery, targetBattery))
            return;

        var damageTargetBatteryValue = GetAbsorption(comp);
        if (targetBattery.CurrentCharge < damageTargetBatteryValue)
        {
            DealBatteryDamage(pulseDemonBattery, -targetBattery.CurrentCharge);
            DealBatteryDamage(targetBattery, targetBattery.CurrentCharge * 100);
        }
        else
        {
            DealBatteryDamage(pulseDemonBattery, -damageTargetBatteryValue);
            DealBatteryDamage(targetBattery, damageTargetBatteryValue * 100);
        }


        var targetCoords = Comp<TransformComponent>((EntityUid) args.Target).Coordinates;
        if (_random.Prob(comp.ParticlesSpawnProbability))
            EntityManager.SpawnEntity(_random.Pick(comp.ParticlesPrototypes), targetCoords);

        args.Repeat = true;
    }
    #endregion

    #region Cable Hop
    private void OnCableHop(EntityUid uid, PulseDemonComponent comp, PulseDemonCableHopActionEvent args)
    {
        if (args.Handled)
            return;

        var cable = args.Target.GetEntitiesInTile(LookupFlags.Uncontained, _lookup)
            .Where(HasComp<DemonCableMarkerComponent>)
            .FirstOrNull();

        if (cable == null)
        {
            args.Handled = false;
            return;
        }

        var pulseDemonBattery = Comp<BatteryComponent>(uid);

        if (!CheckEnergyAndDealBatteryDamage(pulseDemonBattery, args.Cost))
        {
            args.Handled = false;
            return;
        }

        args.Handled = true;

        var coords = Transform((EntityUid) cable).Coordinates;

        _transform.SetCoordinates(uid, coords);
        EntityManager.SpawnEntity(_random.Pick(comp.ParticlesPrototypes), coords);
    }
    #endregion

    /// <returns>True - if the target has energy left and the demon's energy is not full. False - if one of the previously mentioned conditions is not met</returns>
    private bool CheckTargetAndDemonEnergy(BatteryComponent demonBattery, BatteryComponent targetBattery)
    {
        if (demonBattery.IsFullyCharged)
        {
            var popupMessage = Loc.GetString("pulse-demon-energy-volume-full");
            _popup.PopupCursor(popupMessage, demonBattery.Owner, Shared.Popups.PopupType.Medium);
            return false;
        }

        if (targetBattery.CurrentCharge <= 1)
        {
            var popupMessage = Loc.GetString("pulse-demon-target-energy-volume-drain-out");
            _popup.PopupCursor(popupMessage, demonBattery.Owner, Shared.Popups.PopupType.Medium);
            return false;
        }

        return true;
    }

    private bool CheckEnergyAndDealBatteryDamage(BatteryComponent demonBattery, float cost)
    {
        if (demonBattery.CurrentCharge <= cost)
        {
            var popupMessage = Loc.GetString("pulse-demon-not-enough-energy", ("cost", cost));
            _popup.PopupEntity(popupMessage, demonBattery.Owner, Shared.Popups.PopupType.Medium);
            return false;
        }
        else
        {
            DealBatteryDamage(demonBattery, cost);
            return true;
        }
    }
}
