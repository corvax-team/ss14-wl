using Content.Shared._WL.Skills.Components;
using Content.Shared._WL.Skills.Events;
using Content.Shared._WL.Skills.Prototypes;
using Content.Shared.Administration.Logs;
using Content.Shared.Cloning;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.Mind;
using Content.Shared.Random.Helpers;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Shared._WL.Skills.Systems
{
    public abstract partial class SharedSkillsSystem : EntitySystem
    {
        [Dependency] protected readonly IPrototypeManager _protoMan = default!;
        [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;
        [Dependency] protected readonly IConfigurationManager _confMan = default!;
        [Dependency] protected readonly IRobustRandom _random = default!;
        [Dependency] protected readonly DamageableSystem _damage = default!;
        [Dependency] protected readonly SharedJobSystem _job = default!;
        [Dependency] protected readonly SharedMindSystem _mind = default!;

        public SkillsConfigurationPrototype Config { get; private set; } = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RandomSkillsComponent, MapInitEvent>(OnRandomMapInit);

            SubscribeLocalEvent<SkillsHolderComponent, CloningEvent>(OnCloning);
            SubscribeLocalEvent<SkillsHolderComponent, SkillAddedEvent>(OnSkillAdded);

            SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);

            Config = _protoMan.EnumeratePrototypes<SkillsConfigurationPrototype>()
                .MaxBy(x => x.Priority)!;
        }

        private void OnRandomMapInit(EntityUid holder, RandomSkillsComponent comp, MapInitEvent ev)
        {
            var skillHolderComp = EnsureComp<SkillsHolderComponent>(holder);

            var skills = _protoMan.EnumeratePrototypes<SkillPrototype>();

            if (comp.Adjust != null)
            {
                var toAdjust = comp.Adjust.Value;
                foreach (var skill in skills)
                {
                    var min = skillHolderComp.Skills.FirstOrNull(s => s.Key.Id.Equals(skill.ID))?.Value
                        ?? SkillLevel.Inexperienced;
                    var max = (int) SkillLevel.Master - (int) min;

                    toAdjust = Math.Clamp(toAdjust, 0, max);

                    SetSkill((holder, skillHolderComp), skill, min + toAdjust, null);
                }
            }
            else
            {
                var job = (JobPrototype?) null;
                var mind = _mind.GetMind(holder);
                if (_job.MindTryGetJob(mind, out _, out var jobProto))
                    job = jobProto;

                foreach (var skill in skills)
                {
                    var limitation = job == null ? null : skill.JobLimitations[job.ID];

                    var minLevel = SkillLevel.Inexperienced;
                    var maxLevel = SkillLevel.Master;

                    if (limitation != null)
                    {
                        minLevel = limitation.MinSkillLevel;
                        maxLevel = limitation.MaxSkillLevel;
                    }

                    var level = _random.Next((int) minLevel, (int) maxLevel + 1);

                    SetSkill((holder, skillHolderComp), skill, (SkillLevel) level, null);
                }
            }
        }

        private void OnCloning(EntityUid holder, SkillsHolderComponent comp, ref CloningEvent args)
        {
            var cloned = args.Source;
            var spawned = args.Target;

            if (!TryComp<SkillsHolderComponent>(spawned, out var spawnedSkillsHolderComp))
                return;

            var ent = new Entity<SkillsHolderComponent?>(spawned, spawnedSkillsHolderComp);
            foreach (var skill in comp.Skills)
            {
                SetSkill(ent, skill.Key.Id, skill.Value, null);
            }

            if (!TryComp<DamageableComponent>(cloned, out var damageableComp))
                return;

            var chance = comp.CloningNoPenaltyProbability;

            foreach (var damage in damageableComp.Damage.DamageDict)
            {
                if (!Config.CloningConfig.CloningPenalties.TryGetValue(damage.Key, out var dict))
                    continue;

                var cached = (float?) null;
                foreach (var item in dict)
                {
                    if (item.Key < damage.Value)
                    {
                        cached = item.Value;
                        continue;
                    }
                }

                if (cached != null)
                    chance *= cached.Value;
            }

            chance = Math.Clamp(chance, 0f, 1f);

            var isSuccessful = _random.Prob(chance);
            if (!isSuccessful)
            {
                var count = _random.Pick(Config.CloningConfig.SkillsForgettingCountWeights);

                var picked = _random.GetItems(spawnedSkillsHolderComp.Skills.Keys.ToList(), count, false);
                foreach (var pick in picked)
                {
                    //TODO: отслеживать максимальное и минимальное значения скиллов и выбирать рандомно между ними.
                    spawnedSkillsHolderComp.Skills[pick] = SkillLevel.Inexperienced;
                }
            }
        }

        private void OnSkillAdded(EntityUid holder, SkillsHolderComponent comp, ref SkillAddedEvent args)
        {
            var values = Enum.GetValues<SkillLevel>()
                .Order();

            foreach (var value in values)
            {
                if (value > args.Level)
                    continue;

                var info = GetSkillInfo(args.Prototype.Id, value);
                if (info == null || info.ExtraComponents == null)
                    continue;

                EntityManager.AddComponents(holder, info.ExtraComponents, true);
            }
        }

        private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
        {
            if (!args.WasModified<SkillsConfigurationPrototype>())
                return;

            var proto = _protoMan.EnumeratePrototypes<SkillsConfigurationPrototype>()
                .MaxBy(p => p.Priority)!;

            Config = proto;
        }

        #region Public
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

            var ev = new SkillAddedEvent((holder.Owner, comp), skillId, level);
            RaiseLocalEvent(holder.Owner, ref ev);
        }

        /// <summary>
        /// Устанавливает скилл указанного ID на определённый уровень, если скилла(по какой-то причине) нет, то он будет добавлен.
        /// </summary>
        /// <param name="holder">Сущность.</param>
        /// <param name="skill">Прототип скилла.</param>
        /// <param name="level">Уровень скилла.</param>
        /// <param name="needLog">Оставьте null, если логирование действий не требуется.</param>
        public void SetSkill(Entity<SkillsHolderComponent?> holder, SkillPrototype skill, SkillLevel level, ICommonSession? needLog = null)
        {
            SetSkill(holder, skill.ID, level, needLog);
        }

        /// <summary>
        /// Устанавливает скилл указанного ID на определённый уровень, если скилла(по какой-то причине) нет, то он будет добавлен.
        /// </summary>
        /// <param name="holder">Сущность.</param>
        /// <param name="skill">Обёртка прототипа скилла.</param>
        /// <param name="level">Уровень скилла.</param>
        /// <param name="needLog">Оставьте null, если логирование действий не требуется.</param>
        public void SetSkill(Entity<SkillsHolderComponent?> holder, ProtoId<SkillPrototype> skill, SkillLevel level, ICommonSession? needLog = null)
        {
            SetSkill(holder, skill.Id, level, needLog);
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

        /// <summary>
        /// Получает информацию об определенном уровне скилла для определенного прототипа скилла.
        /// </summary>
        public SkillLevelInfo? GetSkillInfo(string? skillid, SkillLevel level)
        {
            if (skillid == null)
                return null;

            if (!_protoMan.TryIndex<SkillPrototype>(skillid, out var proto))
                return null;

            return GetSkillInfo(proto, level);
        }

        /// <summary>
        /// Получает информацию об определенном уровне скилла для определенного прототипа скилла.
        /// </summary>
        public SkillLevelInfo? GetSkillInfo(SkillPrototype? skill, SkillLevel level)
        {
            if (skill == null)
                return null;

            skill.Info.TryGetValue(level, out var info);

            return info;
        }
        #endregion
    }
}
