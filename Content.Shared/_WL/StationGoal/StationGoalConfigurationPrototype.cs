using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.StationGoal
{
    [DataDefinition]
    [Serializable, NetSerializable]
    [Prototype("stationGoalConfiguration")]
    public sealed partial class StationGoalConfigurationPrototype : IPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField] public int MinGoals { get; private set; } = 1;

        [DataField] public int MaxGoals { get; private set; } = 4;

        [DataField] public int Priority { get; private set; } = 0;
    }
}
