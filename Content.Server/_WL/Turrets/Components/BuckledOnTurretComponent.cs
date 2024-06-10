using Content.Shared.Mind;

namespace Content.Server._WL.Turrets.Components
{
    [RegisterComponent]
    public sealed partial class BuckledOnTurretComponent : Component
    {
        public Entity<BuckleableTurretComponent>? Turret;
        public Entity<MindComponent>? Mind;
    }
}
