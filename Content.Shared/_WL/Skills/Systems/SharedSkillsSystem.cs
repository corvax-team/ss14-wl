using Content.Shared._WL.Skills.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Shared._WL.Skills.Systems
{
    public abstract partial class SharedSkillsSystem : EntitySystem
    {
        [Dependency] protected readonly IPrototypeManager _protoMan = default!;
        [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;

        public override void Initialize()
        {
            base.Initialize();


        }

        /// <summary>
        /// Устанавливает скилл указанного ID на определённый уровень, если скилла(по какой-то причине) нет, то он будет добавлен.
        /// </summary>
        /// <param name="holder">Сущность.</param>
        /// <param name="skillId">ID прототипа скилла.</param>
        /// <param name="level">Уровень скилла.</param>
        /// <param name="needLog">Оставьте null, если логирование действий не требуется.</param>
        public void SetSkill(Entity<SkillsHolderComponent?> holder, string skillId, SkillLevel level, ICommonSession? needLog = null)
        {
            if (!Resolve(holder.Owner, ref holder.Comp))
                return;

            var comp = holder.Comp;

            if (needLog != null)
            {
                var levelBefore = comp.Skills.FirstOrNull(k => k.Key.Id.Equals(skillId));
                if (levelBefore?.Value != level)
                {
                    var proto = _protoMan.Index<SkillPrototype>(skillId);
                    var msg =
                        $"Игрок {needLog.Name} изменил скилл '{proto.Name}' у сущности '{ToPrettyString(holder.Owner)}' " +
                        $"с уровня '{GetSkillLocName(levelBefore?.Value)}' на уровень '{GetSkillLocName(level)}'";
                    comp.ChangedSkills[skillId] = DateTime.Now.TimeOfDay;

                    _adminLog.Add(LogType.Skills, LogImpact.Medium, $"{msg}");
                }
            }

            comp.Skills[skillId] = level;

            Dirty(holder.Owner, holder.Comp);
        }

        /// <summary>
        /// Проверяет есть ли у сущности скилл с указанным ID.
        /// </summary>
        /// <param name="holder">Сущность с компонентом скиллов.</param>
        /// <param name="skillId">ID прототипа скилла.</param>
        /// <returns>True - если у сущности есть скилл с указанным ID, false - если нет.</returns>
        public bool HasSkill(Entity<SkillsHolderComponent?> holder, string skillId)
        {
            if (!Resolve(holder.Owner, ref holder.Comp))
                return false;

            return holder.Comp.Skills.ContainsKey(skillId);
        }

        /// <summary>
        /// Перегрузка метода <see cref="HasSkill(Entity{SkillsHolderComponent?}, string)" />, но с выходным параметром, содержащим информацию об уровне скилла.
        /// </summary>
        /// <param name="holder">Сущность.</param>
        /// <param name="skillId">ID прототипа скилла.</param>
        /// <param name="levelWithSkill">ID и уровень скилла. Равно null, если скилл с заданным ID не найден.</param>
        /// <returns></returns>
        public bool HasSkill(Entity<SkillsHolderComponent?> holder, string skillId, [NotNullWhen(true)] out (string Skill, SkillLevel Level)? levelWithSkill)
        {
            levelWithSkill = null;

            if (!Resolve(holder.Owner, ref holder.Comp))
                return false;

            if (!holder.Comp.Skills.TryGetValue(skillId, out var level))
                return false;

            levelWithSkill = (skillId, level);
            return true;
        }

        /// <summary>
        /// Проверяет есть ли у сущности скилл с указанным ID указанного уровня.
        /// </summary>
        /// <param name="holder">Сущность.</param>
        /// <param name="skillId">ID прототипа скилла.</param>
        /// <param name="level">Требуемый уровень скилла.</param>
        public bool HasSkill(Entity<SkillsHolderComponent?> holder, string skillId, SkillLevel level)
        {
            if (!HasSkill(holder, skillId, out var skill))
                return false;

            return skill.Value.Level == level;
        }

        /// <summary>
        /// Проверяет есть ли у сущности скилл с указанным ID и уровень скилла выше или равен указанного.
        /// Нужно это, так как знания скиллов суммируются.
        /// </summary>
        /// <param name="holder">Сущность.</param>
        /// <param name="skillId">ID прототипа скилла.</param>
        /// <param name="level">Минимальный уровень скилла.</param>
        public bool HasSkillMin(Entity<SkillsHolderComponent?> holder, string skillId, SkillLevel level)
        {
            if (!HasSkill(holder, skillId, out var skill))
                return false;

            return skill.Value.Level >= level;
        }

        /// <summary>
        /// Возвращает FormattedMessage формата: '[color=#FF11FFFF]Атмосферика[/color]: Мастер'.
        /// </summary>
        /// <param name="skillId">ID протоипа скилла.</param>
        /// <param name="level">Уровень скилла.</param>
        public FormattedMessage? GetFormattedMessage(string skillId, SkillLevel level, Entity<SkillsHolderComponent>? holder = null)
        {
            if (!_protoMan.TryIndex<SkillPrototype>(skillId, out var skillProto))
                return null;

            var msg = new FormattedMessage();

            var skillLevelName = GetSkillLocName(level);
            var skillName = skillProto.Name;
            if (skillProto.NameColor != null)
                skillName = $"[color={skillProto.NameColor.Value.ToHex()}]" + skillName + "[/color]";

            var full = skillName + ": " + skillLevelName;
            if (holder != null)
                if (holder.Value.Comp.ChangedSkills.TryGetValue(skillId, out var time))
                    full = $"{full} [bold]ИЗМЕНЕНО В {time}[/bold]";

            msg.AddMarkupOrThrow(full);

            return msg;
        }

        /// <summary>
        /// Возвращает строку формата: 'Атмосферика: Мастер'.
        /// </summary>
        /// <param name="skillId">ID прототипа скилла.</param>
        /// <param name="level">Уровень скилла.</param>
        public string? GetFormattedString(string skillId, SkillLevel level, Entity<SkillsHolderComponent>? holder = null)
        {
            if (!_protoMan.TryIndex<SkillPrototype>(skillId, out var skillProto))
                return null;

            var msg = $"{skillProto.Name}: {GetSkillLocName(level)}";
            if (holder != null)
                if (holder.Value.Comp.ChangedSkills.TryGetValue(skillId, out var time))
                    msg = $"{msg} ИЗМЕНЕНО В {time}";

            return msg;
        }

        /// <summary>
        /// Возвращает русское название уровня скилла с заглавной буквы.
        /// </summary>
        /// <param name="skill">Уровень скилла.</param>
        public static string GetSkillLocName(SkillLevel? skill)
        {
            return skill switch
            {
                SkillLevel.Inexperienced => "Неопытный",
                SkillLevel.Basic => "Базовый",
                SkillLevel.Trained => "Обученный",
                SkillLevel.Experienced => "Опытный",
                SkillLevel.Master => "Мастер",
                _ => "null"
            };
        }

        public IEnumerable<FormattedMessage> GetFormattedSkillPrototypes()
        {
            return _protoMan.EnumeratePrototypes<SkillPrototype>()
                .Select(p =>
                {
                    var msg = new FormattedMessage();

                    var id = p.ID;
                    var name = p.Name;
                    if (p.NameColor != null)
                        name = $"[color={p.NameColor.Value.ToHex()}]{name}[/color]";

                    msg.AddMarkupOrThrow($"{name}: {id}");

                    return msg;
                });
        }
    }
}
