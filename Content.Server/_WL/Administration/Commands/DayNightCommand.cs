using Content.Server._WL.DayNight;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Server.GameObjects;
using Robust.Shared.Console;
using Robust.Shared.Map;
using System.Linq;
using System.Numerics;

namespace Content.Server._WL.Administration.Commands
{
    [AdminCommand(AdminFlags.Mapping)]
    public sealed partial class DayNightCommand : LocalizedEntityCommands
    {
        [Dependency] private MapSystem _map = default!;

        public override string Command => "daynight";

        public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            var length = args.Length;

            switch (length)
            {
                case 1:
                    return CompletionResult.FromHintOptions(_map.GetAllMapIds().Select(x => x.ToString()), Loc.GetString("generic-mapid"));
                case 2:
                    return CompletionResult.FromHint(Loc.GetString("cmd-daynight-full-cycle"));
                case 3:
                    return CompletionResult.FromHint(Loc.GetString("cmd-daynight-day-ratio"));
                case 4:
                    return CompletionResult.FromHint(Loc.GetString("cmd-daynight-night-ratio"));
                case 5:
                    return CompletionResult.FromHint(Loc.GetString("cmd-daynight-day-hex"));
                case 6:
                    return CompletionResult.FromHint(Loc.GetString("cmd-daynight-night-hex"));
                default:
                    return CompletionResult.Empty;

            }
        }

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 6 && args.Length != 4)
            {
                shell.WriteError(LocalizationManager.GetString("shell-need-between-arguments", ("lower", 4), ("upper", 6)));
                return;
            }

            if (!int.TryParse(args[0], out var mapIntegerId))
            {
                shell.WriteError(Loc.GetString("shell-invalid-map-id"));
                return;
            }

            var mapId = new MapId(mapIntegerId);

            if (!_map.MapExists(mapId) || !_map.TryGetMap(mapId, out var mapUid))
            {
                shell.WriteError(Loc.GetString("cmd-savemap-not-exist"));
                return;
            }

            if (!int.TryParse(args[1], out var fullCycleTime) || fullCycleTime <= 0)
            {
                shell.WriteError(Loc.GetString("cmd-daynight-failure-parse-cycle"));
                return;
            }

            if (!int.TryParse(args[2], out var dayRatio) || dayRatio <= 0)
            {
                shell.WriteError(Loc.GetString("cmd-daynight-failure-parse-day"));
                return;
            }

            if (!int.TryParse(args[3], out var nightRatio) || nightRatio <= 0)
            {
                shell.WriteError(Loc.GetString("cmd-daynight-failure-parse-night"));
                return;
            }

            var dayNnightComp = EntityManager.EnsureComponent<DayNightComponent>(mapUid.Value);

            dayNnightComp.DayNightRatio = new Vector2(dayRatio, nightRatio);
            dayNnightComp.FullCycle = TimeSpan.FromSeconds(fullCycleTime);

            if (args.Length != 6)
                return;

            var dayColor = Color.TryFromHex(args[4]);
            var nightColor = Color.TryFromHex(args[5]);

            if (dayColor is null || nightColor is null)
            {
                shell.WriteError(Loc.GetString("shell-invalid-color-hex"));
                return;
            }

            dayNnightComp.DayHex = args[4];
            dayNnightComp.NightHex = args[5];
        }
    }
}
