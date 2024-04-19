using Content.Shared.Whitelist;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

// don't worry about it

namespace Content.Shared.Traits
{
    /// <summary>
    ///     Describes a trait.
    /// </summary>
    [Prototype("trait")]
    public sealed partial class TraitPrototype : IPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = default!;

        /// <summary>
        ///     The name of this trait.
        /// </summary>
        [DataField("name")]
        public string Name { get; private set; } = "";

        /// <summary>
        ///     The description of this trait.
        /// </summary>
        [DataField("description")]
        public string? Description { get; private set; }

        /// <summary>
        ///     The stronger the difference with zero, the better or worse the trait should be.
        ///     When selecting traits, the total should always be 0.
        /// </summary>
        [DataField("points")]
        public int AccumulatedPoints = 0;

        /// <summary>
        ///     Don't apply this trait to entities this whitelist IS NOT valid for.
        /// </summary>
        [DataField("whitelist")]
        public EntityWhitelist? Whitelist;

        /// <summary>
        ///     Don't apply this trait to entities this whitelist IS valid for. (hence, a blacklist)
        /// </summary>
        [DataField("blacklist")]
        public EntityWhitelist? Blacklist;

        /// <summary>
        ///     Всякие штучки, которые должны выполняться до или после спавна.
        ///     Тихий спавн тому пример.
        ///     Эффекты, которые должны выполняться после спавна, выполняются после добавления специфичных компонентов.
        ///     Эффекты, выполняющиеся перед спавном, не проверяют игрока на наличие компонентов из <see cref="Whitelist"/> и <see cref="Blacklist"/>, потому что сущности игрока еще как бы нету, мда.
        /// </summary>
        [DataField("effects")]
        public List<TraitEffectEntry>? Effects = null;

        /// <summary>
        ///     The components that get added to the player, when they pick this trait.
        /// </summary>
        [DataField("components")]
        public ComponentRegistry Components { get; private set; } = default!;

        /// <summary>
        ///     Gear that is given to the player, when they pick this trait.
        /// </summary>
        [DataField("traitGear", required: false, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? TraitGear;
    }

    /// <summary>
    ///    Дополнительный класс для эффектов черт, служащий для определения дополнительной информации по выполнению эффектов.
    ///    Например, выполнение после спавна или до.
    /// </summary>
    [DataDefinition]
    [UsedImplicitly]
    public sealed partial class TraitEffectEntry
    {
        /// <summary>
        ///     Собственно, эффект.
        /// </summary>
        [DataField(required: true, serverOnly: true)]
        public TraitEffect Effect;

        /// <summary>
        ///     Будет ли эффект выполняться после спавна игрока или же до?
        /// </summary>
        [DataField]
        public bool IsAfterSpawn = true;

        /// <summary>
        ///     Приоритет выполнения эффектов.
        ///     Сначала выполняются эффекты с наибольшим приоритетом.
        /// </summary>
        [DataField]
        public int Priority = 1;
    }
}
