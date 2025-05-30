using Content.Server.AlertLevel;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Server._WL.ChatGpt.Elements.OpenAi.Functions
{
    public sealed class SetAlertLevelFunction : ToolFunctionModel
    {
        public override LocId Name => "gpt-command-set-alert-level-name";
        public override LocId Description => "gpt-command-set-alert-level-desc";
        public override IReadOnlyDictionary<string, Parameter<object>> Parameters => _parameters;
        public override JsonSchemeType ReturnType => JsonSchemeType.Object;
        public override LocId FallbackMessage => "gpt-command-set-alert-level-fallback";

        private readonly Dictionary<string, Parameter<object>> _parameters;

        private readonly AlertLevelSystem _alertLevel;
        private readonly EntityUid _station;
        private readonly IPrototypeManager _protoMan;

        [ValidatePrototypeId<EntityPrototype>]
        private readonly string BaseStationAlertProtoId = "stationAlerts";

        public SetAlertLevelFunction(
            AlertLevelSystem alertLevelSys,
            EntityUid station,
            IPrototypeManager protoMan)
        {
            _alertLevel = alertLevelSys;
            _protoMan = protoMan;
            _station = station;

            _parameters = Init();
        }

        public Dictionary<string, Parameter<object>> Init()
        {
            var alerts_proto = _protoMan.Index<AlertLevelPrototype>(BaseStationAlertProtoId);

            var levels = alerts_proto.Levels.Keys.ToArray();

            return new()
            {
                ["level"] = new Parameter<string>()
                {
                    Enum = levels.Select(x => (object?)x).ToHashSet(),
                    Description = "gpt-command-set-alert-level-arg-level-desc",
                    Required = true
                },

                ["locked"] = new Parameter<bool>()
                {
                    Description = "gpt-command-set-alert-level-arg-locked-desc",
                    Required = true
                }
            };
        }

        public override string? Invoke(ToolFunctionModel.Arguments? arguments)
        {
            if (arguments == null)
                return null;

            if (!arguments.TryCaste<string>("level", out var level))
                return null;

            if (!arguments.TryCaste<bool>("locked", out var locked))
                return null;

            _alertLevel.SetLevel(_station, level, true, true, true, locked);

            return Loc.GetString(FallbackMessage,
                ("level", level),
                ("locked", locked));
        }
    }
}
