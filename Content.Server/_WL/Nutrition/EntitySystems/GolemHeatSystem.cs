using JetBrains.Annotations;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Body.Systems;
using Content.Server._WL.Nutrition.Components;

namespace Content.Server._WL.Nutrition.EntitySystems
{
    [UsedImplicitly]
    public sealed class GolemHeatSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IEntitySystemManager _systemManager = default!;
        [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
        [Dependency] private readonly SharedBodySystem _bodySystem = default!;

        private const int HeatChangeAmount = 4000;
        private const float SprintSpeed = 3.24f;
        private const float WalkSpeed = 1.8f;
        private const int Acceleration = 20;

        private void ChangeGolemHeat(EntityUid uid)
        {
            if (!_entityManager.TryGetComponent(uid, out HungerComponent? hungerComponent))
                return;

            if (hungerComponent.CurrentThreshold != HungerThreshold.Overfed)
            {
                _bodySystem.UpdateMovementSpeed(uid);
                return;
            }

            if (!TryComp(uid, out TemperatureComponent? temperatureComponent))
                return;

            var temperatureSystem = _systemManager.GetEntitySystem<TemperatureSystem>();
            temperatureSystem.ChangeHeat(uid, HeatChangeAmount, true, temperatureComponent);

            var movementSpeed = EnsureComp<MovementSpeedModifierComponent>(uid);
            _movement.ChangeBaseSpeed(uid, WalkSpeed, SprintSpeed, Acceleration, movementSpeed);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var query = _entityManager.EntityQuery<GolemHeatComponent>();
            foreach (var entity in query)
            {
                ChangeGolemHeat(entity.Owner);
            }
        }
    }
}
