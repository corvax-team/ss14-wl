using Content.Shared.Damage.Components;
using Content.Shared.Movement.Components;
using Robust.Shared.Physics.Components;

namespace Content.Shared._WL.Stamina
{
    public sealed partial class StaminaWasteAttemptEvent : CancellableEntityEventArgs
    {
        public readonly Entity<PhysicsComponent, MovementSpeedModifierComponent, StaminaComponent> Mover;
        public readonly float Speed;
        public float StaminaDamage;

        public StaminaWasteAttemptEvent(
            Entity<PhysicsComponent, MovementSpeedModifierComponent, StaminaComponent> mover,
            float speed,
            float staminaDamage)
        {
            Mover = mover;
            Speed = speed;
            StaminaDamage = staminaDamage;
        }
    }
}
