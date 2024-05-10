using System.Linq;
using System.Text.RegularExpressions;
using Content.Server.Fax;
using Content.Shared.Fax.Components;
using Content.Server.Station.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Paper;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared._WL.StationGoal;

namespace Content.Server.Corvax.StationGoal
{
    /// <summary>
    ///     System to spawn paper with station goal.
    /// </summary>
    public sealed class StationGoalPaperSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly FaxSystem _faxSystem = default!;
        [Dependency] private readonly StationSystem _station = default!;

        private static readonly Regex StationIdRegex = new(@".*\s(\w+-\w+)$");

        private static readonly Regex RandomValueInStringRegex = new(@"\{\{(.+?)\}\}");

        private static readonly string BaseNTLogo =
            """
            [color=#1b487e]███░███░░░░██░░░░[/color]
            [color=#1b487e]░██░████░░░██░░░░[/color]      [head=3]Бланк документа[/head]
            [color=#1b487e]░░█░██░██░░██░█░░[/color]               [head=3]NanoTrasen[/head]
            [color=#1b487e]░░░░██░░██░██░██░[/color]    [bold]Station { $station } ЦК-КОМ[/bold]
            [color=#1b487e]░░░░██░░░████░███[/color]
            ═════════════════════════════════════════
            ПРИКАЗ О НАЗНАЧЕНИИ ЦЕЛИ
            ═════════════════════════════════════════
            Дата: { $date }

            Уважаемое командование станции, задачами Вашей смены являются:

            """;

        private static readonly string BaseEndOfGoal =
            """

            ═════════════════════════════════════════
            [italic]Место для печатей[/italic]
            """;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<RoundStartedEvent>(OnRoundStarted);
        }

        private void OnRoundStarted(RoundStartedEvent ev)
        {
            SendRandomStationGoalsWithConfig();
        }

        /// <summary>
        ///     Send a station goal to all faxes which are authorized to receive it.
        /// </summary>
        /// <returns>True if at least one fax received paper</returns>
        public bool SendStationGoal(StationGoalPrototype goal)
        {
            var enumerator = EntityManager.EntityQueryEnumerator<FaxMachineComponent>();
            var wasSent = false;
            while (enumerator.MoveNext(out var uid, out var fax))
            {
                if (!fax.ReceiveStationGoal)
                    continue;

                if (!TryComp<MetaDataComponent>(_station.GetOwningStation(uid), out var meta))
                    continue;

                var stationId = StationIdRegex.Match(meta.EntityName).Groups[1].Value;
                var stationString = string.IsNullOrEmpty(stationId) ? "???" : stationId;

                var goalContent = FormatStringToGoalContent(BaseNTLogo + goal.Text + BaseEndOfGoal, stationString);

                var printout = new FaxPrintout(
                    goalContent,
                    Loc.GetString("station-goal-fax-paper-name"),
                    null,
                    null,
                    "paper_stamp-centcom",
                    new List<StampDisplayInfo>
                    {
                        new() { StampedName = Loc.GetString("stamp-component-stamped-name-centcom"), StampedColor = Color.FromHex("#006600") },
                    });

                _faxSystem.Receive(uid, printout, null, fax);

                wasSent = true;
            }

            return wasSent;
        }

        public void SendRandomStationGoalsWithConfig()
        {
            var config = GetStationGoalsConfig();
            if (config == null)
            {
                Logger.Error("Fail when selecting the configuration of the spawn station goals");
                return;
            }

            var allGoals = _prototypeManager.EnumeratePrototypes<StationGoalPrototype>();

            var amount = _random.Next(config.MinGoals, config.MaxGoals + 1);
            var pickedGoals = PickRandomGoalByWeight(allGoals, amount);
            StationGoalPrototype goalsTotal = new StationGoalPrototype();

            foreach (var goal in pickedGoals)
            {
                goalsTotal.Text = goalsTotal.Text + "     " + goal.Text;
            }

            SendStationGoal(goalsTotal);
        }

        public StationGoalPrototype? PickRandomGoalByWeight(IList<StationGoalPrototype> goals)
            => PickRandomGoalByWeight(goals.ToDictionary(x => x, x => x.Weight));

        public List<StationGoalPrototype> PickRandomGoalByWeight(IEnumerable<StationGoalPrototype> goals, int amount)
        {
            var toReturn = new List<StationGoalPrototype>();

            var goalsCopy = new List<StationGoalPrototype>(goals);

            var selected = 0;

            while (selected < amount)
            {
                var chosenGoal = PickRandomGoalByWeight(goalsCopy);

                if (chosenGoal == null)
                    break;

                toReturn.Add(chosenGoal);
                goalsCopy.RemoveAll(x => x.ID == chosenGoal.ID);
                selected++;
            }

            return toReturn;
        }

        private T? PickRandomGoalByWeight<T>(IDictionary<T, float> values)
        {
            var factor = _random.NextFloat();

            var maxSum = values.Values.Sum() * factor;

            var cumulative = 0f;

            foreach (var (key, weight) in values)
            {
                cumulative += weight;

                if (cumulative >= maxSum)
                    return key;
            }

            Logger.Error("Fail when selecting a random station goal");
            return default;
        }

        public StationGoalConfigurationPrototype? GetStationGoalsConfig()
        {
            return _prototypeManager.EnumeratePrototypes<StationGoalConfigurationPrototype>()
                .OrderBy(x => x.Priority)
                .FirstOrDefault();
        }

        public string FormatStringToGoalContent(string content, string station)
        {
            var dateString = DateTime.Now.AddYears(1000).ToString("dd.MM.yyyy");

            var toReplace = new Dictionary<string, string>();

            var substringsFromCommand = RandomValueInStringRegex.Matches(content);
            foreach (var match in substringsFromCommand.ToList())
            {
                var weightedRandomProto = _prototypeManager.Index<WeightedRandomPrototype>(match.Groups[1].Value);
                toReplace.Add(match.Value, weightedRandomProto.Pick(_random));
            }

            foreach (var replace in toReplace)
            {
                content = content.Replace(replace.Key, Loc.GetString(replace.Value));
            }

            return content
                .Replace("{ $station }", station)
                .Replace("{ $date }", dateString);
        }
    }
}
