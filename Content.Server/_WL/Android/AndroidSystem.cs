using Content.Server.Actions;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.PowerCell;
using Content.Server.Speech.Components;
using Content.Server.StationEvents.Components;
using Content.Shared._WL.Android;
using Content.Shared._WL.Light.Events;
using Content.Shared.Bed.Sleep;
using Content.Shared.DoAfter;
using Content.Shared.GameTicking.Components;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Mobs;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.PowerCell;
using Content.Shared.PowerCell.Components;
using Content.Shared.StatusEffectNew;
using Content.Shared.Toggleable;
using Content.Shared.Verbs;
using Robust.Server.Audio;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System;

namespace Content.Server._WL.Android
{
    public sealed partial class AndroidSystem : EntitySystem
    {
        [Dependency] private readonly BatterySystem _battery = default!;
        [Dependency] private readonly PowerCellSystem _powerCell = default!;
        [Dependency] private readonly DoAfterSystem _doAfter = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
        [Dependency] private readonly MovementSpeedModifierSystem _move = default!;
        [Dependency] private readonly PopupSystem _popup = default!;
        [Dependency] private readonly PointLightSystem _pointLight = default!;
        [Dependency] private readonly ItemToggleSystem _toggle = default!;
        [Dependency] private readonly AudioSystem _audio = default!;
        [Dependency] private readonly ActionsSystem _actions = default!;
        [Dependency] private readonly ContainerSystem _container = default!;
        [Dependency] private readonly AppearanceSystem _appearance = default!;

        [ViewVariables(VVAccess.ReadOnly)]
        private const float AndroidDoAfterChargeTime = 1f;

       public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AndroidChargeTargetComponent, GetVerbsEvent<AlternativeVerb>>(OnVerb);

            SubscribeLocalEvent<GameRuleStartedEvent>(OnGameRuleStart);
            SubscribeLocalEvent<GameRuleEndedEvent>(OnGameRuleEnd);

            SubscribeLocalEvent<AndroidComponent, ComponentStartup>(OnStartup);

            SubscribeLocalEvent<AndroidComponent, AndroidChargeEvent>(OnDoAfter);
            SubscribeLocalEvent<AndroidComponent, MobStateChangedEvent>(OnMobstateChanged);
            SubscribeLocalEvent<AndroidComponent, BeforeDealHeatDamageFromLightBulbEvent>(OnGetLightBulb);
            SubscribeLocalEvent<AndroidComponent, RefreshMovementSpeedModifiersEvent>(OnModifiersRefresh);
            SubscribeLocalEvent<AndroidComponent, ItemToggledEvent>(OnToggle);
            SubscribeLocalEvent<AndroidComponent, ToggleActionEvent>(OnToggleLightAction);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var query = EntityQueryEnumerator<AndroidComponent, PowerCellDrawComponent, PowerCellSlotComponent>();
            while (query.MoveNext(out var uid, out var androidComp, out var powerCellDrawComp, out var powerCellSlotComp))
            {
                CheckAndDoForcedSleep(uid, androidComp);

                if (!_powerCell.HasDrawCharge(uid, powerCellDrawComp, powerCellSlotComp))
                {
                    continue;
                }

                if (!powerCellDrawComp.CanDraw)
                {
                    _powerCell.SetDrawEnabled((uid, powerCellDrawComp), false);
                    continue;
                }

                _powerCell.SetDrawEnabled((uid, powerCellDrawComp), true);
            }
        }

        private void OnStartup(EntityUid uid, AndroidComponent component, ComponentStartup args)
        {
            _actions.AddAction(uid, ref component.ToggleLightActionEntity, component.TogglelLightAction);

            if (TryComp<ContainerManagerComponent>(uid, out var containerManager))
            {
                var lightEntity = Spawn(component.LightEntityPrototype, Transform(uid).Coordinates);
                component.LightEntity = lightEntity;

                var container = _container.EnsureContainer<ContainerSlot>(uid, "light");
                container.OccludesLight = false;
                _container.Insert(lightEntity, container, null, true);
            }
        }

        private void OnToggle(EntityUid uid, AndroidComponent component, ItemToggledEvent args)
        {
            UpdateLight(uid, component);
        }

        //region Light
        private void OnToggleLightAction(EntityUid uid, AndroidComponent component, ToggleActionEvent args)
        {
            if (args.Handled || args.Action != component.ToggleLightActionEntity)
                return;

            UpdateLight(uid, component, !args.Action.Comp.Toggled);

            args.Toggle = true;
            args.Handled = true;
        }

        public void UpdateLight(EntityUid uid, AndroidComponent component, bool? enabled = null)
        {
            if (!component.LightEntity.HasValue)
                return;

            EntityUid lightEntity = component.LightEntity.Value;

            if (enabled != null)
            {
                _pointLight.SetEnabled(lightEntity, enabled.Value);
                _appearance.SetData(uid, AndroidLightVisuals.Enabled, enabled);

                _audio.PlayPvs(component.ToggleLightSound, uid);
            }

            _pointLight.SetEnergy(lightEntity, _toggle.IsActivated(uid) ? component.LightBaseRadius : component.LightBaseRadius / 3f);

            if (!TryComp<HumanoidAppearanceComponent>(uid, out var appearance))
                return;

            if (!appearance.MarkingSet.TryGetCategory(MarkingCategories.Overlay, out var markings) || markings.Count == 0)
                return;

            Color ledColor = markings[0].MarkingColors[0].WithAlpha(255);
            _pointLight.SetColor(lightEntity, ledColor);
        }
        //endregion Light

        private void OnModifiersRefresh(EntityUid android, AndroidComponent comp, RefreshMovementSpeedModifiersEvent args)
        {
            if (comp.IsUnderIonStorm)
                args.ModifySpeed(comp.IonStormSlownessFactor, comp.IonStormSlownessFactor);
        }

        private void OnGameRuleStart(ref GameRuleStartedEvent args)
        {
            if (!TryComp<IonStormRuleComponent>(args.RuleEntity, out _))
                return;

            var query = EntityQueryEnumerator<AndroidComponent, MovementSpeedModifierComponent>();
            while (query.MoveNext(out var android, out var androidComp, out var movementSpeedComp))
            {
                if (!_random.Prob(androidComp.IonStormSlownessProbability))
                    return;

                androidComp.IsUnderIonStorm = true;
                _move.RefreshMovementSpeedModifiers(android, movementSpeedComp);

                _popup.PopupEntity(androidComp.IonStormPopupMessage, android, android, Shared.Popups.PopupType.Medium);

                EnsureComp<StutteringAccentComponent>(android);
            }
        }

        private void OnGameRuleEnd(ref GameRuleEndedEvent args)
        {
            if (!TryComp<IonStormRuleComponent>(args.RuleEntity, out _))
                return;

            var query = EntityQueryEnumerator<AndroidComponent, MovementSpeedModifierComponent>();
            while (query.MoveNext(out var android, out var androidComp, out var movementSpeedComp))
            {
                androidComp.IsUnderIonStorm = false;
                _move.RefreshMovementSpeedModifiers(android, movementSpeedComp);

                RemComp<StutteringAccentComponent>(android);
            }
        }

        private void OnGetLightBulb(EntityUid android, AndroidComponent comp, BeforeDealHeatDamageFromLightBulbEvent args)
        {
            args.Cancel();
        }

        private void OnMobstateChanged(EntityUid android, AndroidComponent comp, MobStateChangedEvent args)
        {
            if (!TryComp<PowerCellDrawComponent>(android, out var powerCellDrawComp))
                return;

            if (args.NewMobState == MobState.Dead)
                powerCellDrawComp.CanDraw = false;
            else powerCellDrawComp.CanDraw = true;
        }

        private void CheckAndDoForcedSleep(EntityUid android,
            AndroidComponent comp)
        {
            if (_gameTiming.CurTime < comp.NextTime)
                return;

            comp.NextTime = _gameTiming.CurTime + comp.TimeBetweenChecks;

            if (!_powerCell.TryGetBatteryFromSlot(android, out var battery))
                return;

            if (battery.CurrentCharge / battery.MaxCharge * 100 > 5f)
                return;

            if (_random.Prob(comp.ForcedSleepChance))
            {
                var duration = _random.Next(comp.SleepTimeMin, comp.SleepTimeMax);
                _statusEffect.TryAddStatusEffectDuration(android, SleepingSystem.StatusEffectForcedSleeping, duration);
            }
        }

        private void OnVerb(EntityUid target, AndroidChargeTargetComponent comp, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanInteract)
                return;

            if (!HasComp<AndroidComponent>(args.User) ||
                !_powerCell.TryGetBatteryFromSlot(args.User, out var battery_ent, out var battery_comp) ||
                _battery.IsFull(battery_ent.Value, battery_comp))
                return;

            if (!TryComp<BatteryComponent>(args.Target, out var targetBattery) ||
                targetBattery.CurrentCharge / targetBattery.MaxCharge * 100f <= 5f)
                return;

            var doAfter = new DoAfterArgs(EntityManager, args.User, AndroidDoAfterChargeTime, new AndroidChargeEvent(), args.User, target, null)
            {
                BlockDuplicate = true,
                DuplicateCondition = DuplicateConditions.SameEvent,
                BreakOnDamage = true,
                BreakOnHandChange = true,
                BreakOnMove = true
            };

            args.Verbs.Add(new AlternativeVerb()
            {
                Act = () => _doAfter.TryStartDoAfter(doAfter),
                IconEntity = GetNetEntity(target),
                Text = "Зарядка"
            });
        }

        private void OnDoAfter(EntityUid android, AndroidComponent comp, AndroidChargeEvent args)
        {
            if (args.Cancelled || args.Handled)
                return;

            if (!_powerCell.TryGetBatteryFromSlot(android, out var batteryEnt, out var battery)
                || battery.CurrentCharge / battery.MaxCharge * 100f >= 95f
                || batteryEnt == null
                || !TryComp<BatteryComponent>(args.Target, out var targetBattery)
                || targetBattery.CurrentCharge / targetBattery.MaxCharge * 100f <= 5f)
            {
                args.Handled = true;
                return;
            }

            _battery.SetCharge(batteryEnt.Value, battery.CurrentCharge + comp.ChargeRate, battery);
            _battery.SetCharge(args.Target.Value, targetBattery.CurrentCharge - comp.ChargeRate * comp.TargetDecreaseFactor);

            args.Repeat = true;
        }
    }
}
