using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

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
        public Dictionary<SkillLevel, SkillLevelInfo> Info { get; private set; } = new();

        /// <summary>
        ///     Цвет имени скилла.
        ///     Отображается в меню редактирования персонажа.
        /// </summary>
        [DataField("color")]
        public Color? NameColor { get; private set; } = null;

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
        [DataField]
        public Dictionary<ProtoId<JobPrototype>, SkillLimitation> JobLimitations { get; private set; } = new();

        public string Name => Loc.GetString(_name.Id);
        public string Description => Loc.GetString(_desc.Id);
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
