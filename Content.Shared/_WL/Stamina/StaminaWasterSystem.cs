using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Inventory;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Nutrition.Components;
using Content.Shared.Throwing;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Content.Shared._WL.Stamina
{
    public sealed partial class StaminaWasterSystem : EntitySystem
    {
        [Dependency] private readonly SharedTransformSystem _transform = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly StaminaSystem _stamina = default!;

        public override void Initialize()
        {
            base.Initialize();

            UpdatesOutsidePrediction = true;

            SubscribeLocalEvent<StaminaWasterComponent, MoveEvent>(OnMove);
            SubscribeLocalEvent<ThrownEvent>(OnThrown);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var query = EntityQueryEnumerator<StaminaWasterComponent, StaminaComponent>();
            while (query.MoveNext(out var mover, out var staminaWasterComp, out var staminaComp))
            {
                ref var damage = ref staminaWasterComp.Update;
                if (damage == null)
                    continue;

                _stamina.TakeStaminaDamage(mover, damage.Value, staminaComp, null, null, false, null);

                damage = null;
            }
        }

        //Асинхронный потому что... почему нет? Это параша вызывается ВСТАВЬТЕ_ТИКРЕЙТ раз в секунду
        private async void OnMove(EntityUid mover, StaminaWasterComponent comp, MoveEvent args)
        {
            try
            {
                await Calculate(mover, comp, args);
            }
            catch
            {
                throw new($"{nameof(StaminaWasterSystem)}: Неизвестная ошибка!!");
            }
        }

        private void OnThrown(ref ThrownEvent args)
        {
            if (args.User == null)
                return;

            var user = args.User.Value;
            var thrown = args.Thrown;
            if (!TryComp<StaminaWasterComponent>(user, out var staminaWasterComp) ||
                !TryComp<StaminaComponent>(user, out var staminaComp))
                return;

            var damage = 0f;

            if (TryComp<PhysicsComponent>(thrown, out var thrownPhysicsComp))
                damage += thrownPhysicsComp.Mass * staminaWasterComp.ThrowPenaltyForOneMassUnit;

            if (!CanAddDamage((user, staminaComp, staminaWasterComp), damage, out var need))
            {
                if (MathHelper.CloseTo(need.Value, 0f))
                    return;

                damage = Math.Clamp(damage, 0f, need.Value);
            }

            _stamina.TakeStaminaDamage(user, damage, staminaComp, null, null, false, null);
        }

        //Ну видишь эту громадину?! А она выполняется 60 раз в секунду, ппц
        private async Task Calculate(EntityUid mover, StaminaWasterComponent comp, MoveEvent args)
        {
            try
            {
                if (!mover.IsValid())
                    return;

                if (comp.Deleted)
                    return;

                if (!args.NewPosition.TryDistance(EntityManager, _transform, args.OldPosition, out var distance) ||
                    MathHelper.CloseTo(distance, 0f, 1E-05))
                    return;

                if (!TryComp<StaminaComponent>(mover, out var staminaComp))
                    return;

                if (!CanWaste((mover, staminaComp, comp)))
                    return;

                if (!TryComp<InventoryComponent>(mover, out var inventoryComp))
                    return;

                if (!TryComp<MovementSpeedModifierComponent>(mover, out var movementComp))
                    return;

                if (!TryComp<PullerComponent>(mover, out var pullerComp))
                    return;

                var entities = new List<EntityUid>();

                var speed = distance * _timing.TickRate;
                var speedRelativeToMaxPercentage =
                    speed / Math.Clamp(movementComp.BaseSprintSpeed, 0.001f, float.MaxValue);

                var pulling = pullerComp.Pulling;
                if (MathHelper.CloseTo(speed, movementComp.BaseWalkSpeed, 1E-02) && pulling == null)
                    return;

                if (pulling != null)
                    entities.Add(pulling.Value);

                foreach (var container in inventoryComp.Containers)
                {
                    if (container.ContainedEntity == null)
                        continue;

                    entities.Add(container.ContainedEntity.Value);
                }

                var mass = 0f;
                foreach (var entity in entities)
                {
                    if (!TryComp<PhysicsComponent>(entity, out var containedPhysicComp))
                        continue;

                    mass += containedPhysicComp.Mass;
                }

                var staminaChangedValue =
                    Math.Clamp(mass * comp.PenaltyForOneMassUnit, 1f, float.MaxValue) * comp.StaminaPerSecond * speedRelativeToMaxPercentage / _timing.TickRate;

                if (TryComp<HungerComponent>(mover, out var hungerComp))
                    if (comp.HungerPenalties.TryGetValue(hungerComp.CurrentThreshold, out var hungerModifier))
                        staminaChangedValue *= hungerModifier;

                var ev = new StaminaWasteAttemptEvent((mover, movementComp, staminaComp), speed, staminaChangedValue);
                RaiseLocalEvent(mover, ev);

                if (ev.Cancelled)
                    return;

                comp.Update = ev.StaminaDamage;
            }
            catch
            {
                throw new Exception($"{nameof(StaminaWasterSystem)}: Неизвестная ошибка!!");
            }
        }

        public static bool CanWaste(Entity<StaminaComponent, StaminaWasterComponent> entity)
        {
            var ent = entity.Owner;
            var staminaDamage = entity.Comp1.StaminaDamage;
            var maxStamina = entity.Comp1.CritThreshold;
            var minBound = entity.Comp2.MinStaminaBoundPercentage;

            if (staminaDamage >= maxStamina * minBound)
                return false;

            return true;
        }

        public static bool CanAddDamage(
            Entity<StaminaComponent, StaminaWasterComponent> entity,
            float damage,
            [NotNullWhen(false)] out float? need)
        {
            need = null;

            var ent = entity.Owner;
            var staminaDamage = entity.Comp1.StaminaDamage;
            var maxStamina = entity.Comp1.CritThreshold;
            var minBound = entity.Comp2.MinStaminaBoundPercentage;

            if (!CanWaste(entity))
            {
                need = 0;
                return false;
            }

            if (staminaDamage + damage <= minBound * maxStamina)
                return true;

            need = minBound * maxStamina - staminaDamage;
            return false;
        }
    }
}
