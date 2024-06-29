using Content.Server.Administration.Logs;
using Content.Server.GameTicking;
using Content.Shared._WL.Skills;
using Content.Shared._WL.Skills.Components;
using Content.Shared._WL.Skills.Systems;
using Content.Shared.Database;
using System.Text;

namespace Content.Server._WL.Skills
{
    public sealed partial class SkillsSystem : SharedSkillsSystem
    {
        [Dependency] private readonly IAdminLogManager _adminLog = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);

            InitializeMechanics();
        }

        private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
        {
            // Если должности нет, то скиллы получить не выйдет.
            if (args.JobId == null)
                return;

            // Получаем скиллы из профиля.
            if (!args.Profile.Skills.TryGetValue(args.JobId, out var skills))
                return;

            // Если для сущности не требуется наличие скиллов, то.... ладно.
            if (!TryComp<SkillsHolderComponent>(args.Mob, out var skillsHolderComp))
                return;

            var logMessage =
                new StringBuilder($"Игрок {args.Player.Name} зашёл в раунд за должность {args.JobId ?? "NULL"} со следующими скиллами:");

            foreach (var skill in skills)
            {
                // Синхронизируем компонент с профилем
                var skillsDict = skillsHolderComp.Skills;

                skillsDict[skill.Key] = skill.Value;

                // Логгирование
                if (_protoMan.TryIndex<SkillPrototype>(skill.Key, out var skillProto))
                    logMessage.AppendLine($"- {skillProto.Name}: уровень {SkillsSystem.GetSkillLocName(skill.Value)}");
            }

            _adminLog.Add(LogType.Skills, LogImpact.Medium, $"{logMessage.ToString()}");
        }
    }
}
