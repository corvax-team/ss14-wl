using Content.Server.Corvax.StationGoal;
using Content.Server.Station.Systems;
using Content.Shared.Medical.SuitSensor;
using System.Linq;

namespace Content.Server._WL.Economics.WageCoefficients
{
    public sealed partial class StationGoalWageCoefficient : EconomicWageCoefficient
    {
        [DataField("needed", required: true)]
        public float JobNeededCoefficient;

        [DataField("noNeeded", required: true)]
        public float JobNoNeededCoefficient;

        public override string Name => "Коэффициент по цели станции";
        public override string Description => $"Если ваш отдел напрямую занимается целью - {JobNoNeededCoefficient}. Отдел не занимается целью - {JobNoNeededCoefficient}";
        public override string Initials => "К.ц.с.";

        public override float Calculate(IEntityManager entMan, EntityUid holder, SuitSensorStatus sensorStatus)
        {
            var stationGoalSystem = entMan.System<StationGoalPaperSystem>();
            var stationSystem = entMan.System<StationSystem>();

            var station = stationSystem.GetOwningStation(holder);

            if (station == null)
                return 1f;

            var departments = stationGoalSystem.GetStationGoals(station.Value)
                .Where(g => !string.IsNullOrEmpty(g.Department))
                .Select(g => g.Department ?? throw new NullReferenceException());

            if (departments == null)
                return 1f;

            return departments.Any(x => sensorStatus.JobDepartments.Contains(x))
                ? JobNeededCoefficient
                : JobNoNeededCoefficient;
        }
    }
}
