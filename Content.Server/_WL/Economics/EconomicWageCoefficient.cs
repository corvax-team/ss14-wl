using Content.Shared.Medical.SuitSensor;
using JetBrains.Annotations;
using Robust.Shared.Serialization;

namespace Content.Server._WL.Economics
{
    [ImplicitDataDefinitionForInheritors]
    [MeansImplicitUse]
    public abstract partial class EconomicWageCoefficient
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Initials { get; }

        public abstract float Calculate(IEntityManager entMan, EntityUid holder, SuitSensorStatus sensorStatus);
    }
}
