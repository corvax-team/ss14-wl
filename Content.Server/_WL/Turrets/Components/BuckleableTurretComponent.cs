using Content.Server._WL.Turrets.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._WL.Turrets
{
    [RegisterComponent]
    public sealed partial class BuckleableTurretComponent : Component
    {
        [DataField]
        public TimeSpan RideTime = TimeSpan.FromSeconds(2);

        public bool Riding = false;

        public Entity<BuckledOnTurretComponent>? User;

        public EntityUid? ExitRidingActionContainer;

        [DataField("exitAction", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string ExitRidingAction;
    }
}
