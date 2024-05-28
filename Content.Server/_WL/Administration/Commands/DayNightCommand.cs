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
    public sealed partial class DayNightCommand : LocalizedCommands
    {
        [Dependency] private readonly IEntityManager _entMan = default!;
        [Dependency] private readonly IMapManager _mapMan = default!;

        public override string Command => "daynight";
        public override string Description
            =>
            """
                Добавляет карте смену дня и ночи.
                Желательно, чтоб это была планета.
                Также желательно, чтобы эта команда использовалась только с неинициализированными картами.
            """;

        public override string Help => "daynight <mapId> <fullCycle> <dayRatio> <nightRatio> <dayColor> <nightColor>";

        public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            if (args.Length == 1)
            {
                return CompletionResult.FromHintOptions(_mapMan.GetAllMapIds().Select(x => x.ToString()), "MapId");
            }
            else if (args.Length == 2)
            {
                return CompletionResult.FromHint("FullCycle in seconds");
            }
            else if (args.Length == 3)
            {
                return CompletionResult.FromHint("Day ratio an integer");
            }
            else if (args.Length == 4)
            {
                return CompletionResult.FromHint("Night ration an integer");
            }
            else if (args.Length == 5)
            {
                return CompletionResult.FromHint("Day Hex");
            }
            else if (args.Length == 6)
            {
                return CompletionResult.FromHint("Night Hex");
            }

            return CompletionResult.Empty;
        }

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 6 && args.Length != 4)
            {
                shell.WriteError(LocalizationManager.GetString("shell-wrong-arguments-number"));
                return;
            }

            var mapSys = _entMan.System<MapSystem>();

            if (!int.TryParse(args[0], out var mapIntegerId))
            {
                shell.WriteError("MapId должно быть числом!");
                return;
            }

            var mapId = new MapId(mapIntegerId);

            if (!mapSys.MapExists(mapId))
            {
                shell.WriteError($"Карты с ID равнм {mapIntegerId} не существует!");
                return;
            }

            if (!int.TryParse(args[1], out var fullCycleTime) || fullCycleTime <= 0)
            {
                shell.WriteError("fullCycleTime должен представлять целое число большее нуля!");
                return;
            }

            if (!int.TryParse(args[2], out var dayRatio) || dayRatio <= 0)
            {
                shell.WriteError("dayRatio должен представлять целое число большее нуля!");
                return;
            }

            if (!int.TryParse(args[3], out var nightRatio) || nightRatio <= 0)
            {
                shell.WriteError("nightRatio должен представлять целое число большее нуля!");
                return;
            }

            if (!mapSys.TryGetMap(mapId, out var mapUid) || mapUid == null)
            {
                shell.WriteError("Неизвестная ошибка.");
                return;
            }

            var dayNnightComp = _entMan.EnsureComponent<DayNightComponent>(mapUid.Value);

            dayNnightComp.DayNightRatio = new Vector2(dayRatio, nightRatio);
            dayNnightComp.FullCycle = TimeSpan.FromSeconds(fullCycleTime);

            if (args.Length != 6)
                return;

            var dayColor = Color.TryFromHex(args[4]);
            var nightColor = Color.TryFromHex(args[5]);
            if (dayColor != null)
            {
                dayNnightComp.DayHex = args[4];
            }
            if (nightColor != null)
            {
                dayNnightComp.NightHex = args[5];
            }
        }
    }
}
