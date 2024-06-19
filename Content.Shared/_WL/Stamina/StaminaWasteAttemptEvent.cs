using Content.Shared.Damage.Components;
using Content.Shared.Movement.Components;

namespace Content.Shared._WL.Stamina
{
    public sealed partial class StaminaWasteAttemptEvent : CancellableEntityEventArgs
    {
        public readonly Entity<MovementSpeedModifierComponent, StaminaComponent> Mover;
        public readonly float Speed;
        public float StaminaDamage;

        public StaminaWasteAttemptEvent(
            Entity<MovementSpeedModifierComponent, StaminaComponent> mover,
            float speed,
            float staminaDamage)
        {
            Mover = mover;
            Speed = speed;
            StaminaDamage = staminaDamage;
        }
    }
}
