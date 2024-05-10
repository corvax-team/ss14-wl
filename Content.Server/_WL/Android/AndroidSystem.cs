using Content.Server.DoAfter;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.PowerCell;
using Content.Server.Traits.Assorted;
using Content.Shared._WL.Android;
using Content.Shared.Bed.Sleep;
using Content.Shared.DoAfter;
using Content.Shared.PowerCell;
using Content.Shared.PowerCell.Components;
using Content.Shared.StatusEffect;
using Content.Shared.Verbs;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._WL.Android
{
    public sealed partial class AndroidSystem : EntitySystem
    {
        [Dependency] private readonly BatterySystem _battery = default!;
        [Dependency] private readonly PowerCellSystem _powerCell = default!;
        [Dependency] private readonly DoAfterSystem _doAfter = default!;
        [Dependency] private readonly NarcolepsySystem _narcolepsy = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;

        private const float AndroidDoAfterChargeTime = 1f;

        [ValidatePrototypeId<StatusEffectPrototype>]
        private const string ForcedSleepStatusEffect = "ForcedSleep";

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AndroidChargeTargetComponent, GetVerbsEvent<AlternativeVerb>>(OnVerb);
            SubscribeLocalEvent<AndroidComponent, AndroidChargeEvent>(OnDoAfter);
            SubscribeLocalEvent<AndroidComponent, StatusEffectAddedEvent>(OnSleepBegin);
            SubscribeLocalEvent<AndroidComponent, StatusEffectEndedEvent>(OnSleepEnd);
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

                if (!powerCellDrawComp.CanDraw || powerCellDrawComp.Drawing)
                    continue;

                _powerCell.SetPowerCellDrawEnabled(uid, true, powerCellDrawComp);
            }
        }

        private void OnSleepBegin(EntityUid android, AndroidComponent comp, StatusEffectAddedEvent args)
        {
            EnsureComp<ForcedSleepingComponent>(android);
        }

        private void OnSleepEnd(EntityUid android, AndroidComponent comp, StatusEffectEndedEvent args)
        {
            RemComp<ForcedSleepingComponent>(android);
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
                var sleepTime = _random.Next(comp.SleepTimeMin, comp.SleepTimeMax);
                _statusEffect.TryAddStatusEffect(android, ForcedSleepStatusEffect, sleepTime, true);
            }
        }

        private void OnVerb(EntityUid target, AndroidChargeTargetComponent comp, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanInteract)
                return;

            if (!HasComp<AndroidComponent>(args.User) ||
                !_powerCell.TryGetBatteryFromSlot(args.User, out _, out var battery) ||
                battery.IsFullyCharged)
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
