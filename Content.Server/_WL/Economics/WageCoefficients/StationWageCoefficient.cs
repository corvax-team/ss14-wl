using Content.Server._WL.Economics;
using Content.Server.Station.Systems;
using Content.Shared._WL.Economics;
using Content.Shared._WL.Economics.Components;
using Content.Shared.Medical.SuitSensor;

namespace Content.Server._WL.Economics.WageCoefficients
{
    public sealed partial class StationWageCoefficient : EconomicWageCoefficient
    {
        [DataField(required: true)]
        public Dictionary<StationType, float> StationTypes = new();

        public override string Name => "Коэффициент по станции";
        public override string Description => "";
        public override string Initials => "К.С.";
        public override float Calculate(IEntityManager entMan, EntityUid holder, SuitSensorStatus sensorStatus)
        {
            var stationSys = entMan.System<StationSystem>();
            var station = stationSys.GetOwningStation(holder);
            if (!entMan.TryGetComponent<StationTypeComponent>(station, out var stationTypeComp))
                return 1f;

            return StationTypes[stationTypeComp.StationType];
        }
    }
}
