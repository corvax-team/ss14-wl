using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Server.Database;
using Content.Server.Preferences.Managers;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Content.Server._WL.Administration.Commands
{
    [AdminCommand(AdminFlags.QuestionnaireSpecialist)]
    public sealed partial class JobForceEnableCommand : LocalizedCommands
    {
        [Dependency] private readonly IPrototypeManager _protoMan = default!;
        [Dependency] private readonly IServerDbManager _serverDb = default!;
        [Dependency] private readonly IAdminLogManager _adminLog = default!;

        public override string Command => "jobforceenable";
        public override string Description => "Позволяет насильно разблокировать проверку должности на возраст.";
        public override string Help => "jobforceenable <player_username> <character_name> <job_name> [True/False]";

        public override async ValueTask<CompletionResult> GetCompletionAsync(IConsoleShell shell, string[] args, string argStr, CancellationToken cancel)
        {
            if (args.Length == 2)
            {
                var username = args[0];

                var user_id = await _serverDb.GetPlayerRecordByUserName(username, cancel);

                if (user_id != null)
                {
                    var characters = await _serverDb.GetPlayerPreferencesAsync(user_id.UserId, cancel);
                    if (characters != null)
                        return CompletionResult.FromHintOptions(characters.Characters.Select(c => c.Value.Name), "<character_name>");
                }
            }
            else if (args.Length == 3)
            {
                var prototypes = _protoMan.EnumeratePrototypes<JobPrototype>();
                return CompletionResult.FromHintOptions(prototypes.Select(p => p.ID), "<job_id>");
            }
            else if (args.Length == 4)
            {
                return CompletionResult.FromHint("[True/False]");
            }

            return CompletionResult.Empty;
        }

        //TODO: сделать проверку на наличие игрока в _cachedPlayers, поле PlayerManager, и обращаться к бд, только если его не было в этом поле.
        public override async void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            try
            {
                if (args.Length != 4 && args.Length != 3)
                {
                    shell.WriteError(LocalizationManager.GetString("shell-wrong-arguments-number"));
                    return;
                }

                var username = args[0];
                var character_name = args[1];
                var job_id = args[2];
                var string_value = args.Length == 4
                    ? args[3]
                    : null;

                if (!_protoMan.TryIndex<JobPrototype>(job_id, out var jobProto))
                {
                    shell.WriteError($"Указанный ID прототипа {nameof(JobPrototype)} не был найден!");
                    return;
                }

                var net_user_id = (await _serverDb.GetPlayerRecordByUserName(username))?.UserId;

                if (net_user_id == null)
                {
                    shell.WriteError($"Игрока с никнеймом [color=yellow]{username}[/color] не найдено.");
                    return;
                }

                var characters = await _serverDb.GetPlayerPreferencesAsync(net_user_id.Value, CancellationToken.None);
                if (characters == null)
                {
                    shell.WriteError("Произошла неизвестная ошибка при получении профиля игрока!");
                    return;
                }

                var character_profile_and_slot = characters.Characters
                    .FirstOrNull(kv => kv.Value.Name.Equals(character_name, StringComparison.CurrentCultureIgnoreCase));

                var character_profile = (HumanoidCharacterProfile?)character_profile_and_slot?.Value;
                var character_slot = character_profile_and_slot?.Key;

                if (character_profile_and_slot == null || character_profile == null || character_slot == null)
                {
                    shell.WriteError($"Персонаж [color=blue]{character_name}[/color] игрока [color=yellow]{username}[/color] не был найден!");
                    return;
                }

                if (string_value != null)
                {
                    if (!bool.TryParse(string_value, out var value))
                    {
                        shell.WriteError($"Не удалось преобразовать [color=gray]{string_value}[/color] в [color=blue]{nameof(Boolean)}[/color]");
                        return;
                    }

                    var newProfile = character_profile.WithJobUnblocking(job_id, value);

                    await _serverDb.SaveCharacterSlotAsync(net_user_id.Value, newProfile, character_slot.Value);

                    //Логгирование
                    var off_on_string = value
                            ? "[color=red]выключил[/color]"
                            : "[color=green]включил[/color]";

                    _adminLog.Add(
                        LogType.WLCommand, LogImpact.Medium, $"{shell.Player?.Name ?? "Неизвестный пользователь"} {off_on_string} проверку на возраст у " +
                        $"персонажа {character_name} для долнжности {jobProto.LocalizedName} игрока {username}."
                        );
                }
                else
                {
                    var off_on_string = "";

                    if (character_profile.JobUnblockings.TryGetValue(job_id, out var value))
                        off_on_string = value
                            ? "[color=red]выключена[/color]"
                            : "[color=green]включена[/color]";
                    else off_on_string = "[color=green]включена[/color]";

                    var text = $"Для персонажа [color=green]{character_name}[/color] пользователя " +
                        $"[color=yellow]{username}[/color] в должности [color=gray]{jobProto.LocalizedName}[/color] " +
                        $"проверка на возраст {off_on_string}.";

                    shell.WriteLine(
                        FormattedMessage.FromMarkupOrThrow(text)
                    );
                }
            }
            catch (Exception ex)
            {
                shell.WriteError(ex.Message);
            }
        }
    }
}
