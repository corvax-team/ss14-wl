using Content.Server._WL.Economics;
using Content.Shared.Medical.SuitSensor;
using Content.Shared.Roles;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Server._WL.Economics.WageCoefficients
{
    public sealed partial class JobWageCoefficient : EconomicWageCoefficient
    {
        [DataField("jobs", required: true, customTypeSerializer: typeof(PrototypeIdDictionarySerializer<float, JobPrototype>))]
        public Dictionary<string, float> JobsAndCoefs = new();

        public override string Name => "Коэффициент по профессии";
        public override string Initials => "К.п.";
        public override string Description => "";

        public override float Calculate(IEntityManager entMan, EntityUid holder, SuitSensorStatus status)
        {
            if (!string.IsNullOrEmpty(status.Job) && JobsAndCoefs.TryGetValue(status.Job, out var coef))
                return coef;
            else return 1f;
        }
    }
}
