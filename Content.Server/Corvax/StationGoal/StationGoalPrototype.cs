using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Corvax.StationGoal
{
    [Serializable, Prototype("stationGoal")]
    public sealed class StationGoalPrototype : IPrototype
    {
        [IdDataField] public string ID { get; } = default!;

        [DataField] public string Text { get; set; } = string.Empty;

        [DataField] public float Weight { get; private set; } = 1;

        [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<DepartmentPrototype>))]
        public string? Department = null;
    }
}
