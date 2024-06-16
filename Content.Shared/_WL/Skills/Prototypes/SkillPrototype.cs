using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Shared._WL.Skills
{
    [Prototype("skill")]
    public sealed partial class SkillPrototype : IPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = default!;

        /// <summary>
        ///     Содержит информацию о каждом уровне скилла.
        /// </summary>
        [DataField(required: true)]
        public Dictionary<SkillLevel, SkillLevelInfo> Info = new();

        /// <summary>
        ///     Цвет имени скилла.
        ///     Отображается в меню редактирования персонажа.
        /// </summary>
        [DataField("color")]
        public Color? NameColor = null;

        /// <summary>
        ///     Имя скилла.
        /// </summary>
        [DataField("name", required: true)]
        private LocId _name;

        /// <summary>
        ///     Описание скилла.
        /// </summary>
        [DataField("desc", required: true)]
        private LocId _desc;

        /// <summary>
        ///     Ограничения для определенных должностей.
        /// </summary>
        [DataField(customTypeSerializer: typeof(PrototypeIdDictionarySerializer<SkillLimitation, JobPrototype>))]
        public Dictionary<string, SkillLimitation> JobLimitations = new();

        public string Name => Loc.GetString(_name.Id);
        public string Description => Loc.GetString(_desc.Id);

        public static string GetSkillLocName(SkillLevel skill)
        {
            switch (skill)
            {
                case SkillLevel.Inexperienced:
                    return "Неопытный";
                case SkillLevel.Basic:
                    return "Базовый";
                case SkillLevel.Trained:
                    return "Обученный";
                case SkillLevel.Experienced:
                    return "Опытный";
                case SkillLevel.Master:
                    return "Мастер";
                default:
                    throw new();
            };
        }
    }


    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class SkillLevelInfo
    {
        /// <summary>
        ///     Количество очков
        /// </summary>
        [DataField(required: true)]
        public int Points;

        /// <summary>
        ///     Описание уровня скилла.
        /// </summary>
        [DataField("description", required: true)]
        private LocId _desc;

        public string Description => Loc.GetString(_desc.Id);
    }

    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class SkillLimitation
    {
        /// <summary>
        ///     Минимальный уровень скилла, возможный у данной роли.
        /// </summary>
        [DataField(required: true)]
        public SkillLevel MinSkillLevel;

        /// <summary>
        ///     Максимальный уровень скилла, возможный у данной роли.
        /// </summary>
        [DataField(required: true)]
        public SkillLevel MaxSkillLevel;
    }
}
