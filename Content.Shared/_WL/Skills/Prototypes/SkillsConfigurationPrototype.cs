using Content.Shared.Damage.Prototypes;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Skills.Prototypes
{
    [Prototype("skillsConfiguration")]
    public sealed partial class SkillsConfigurationPrototype : IPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField]
        public int Priority = 0;

        /// <summary>
        /// Ключ - возраст, при достижении которого к общим очкам скиллов будет добавлено значения словарая.
        /// </summary>
        [DataField(required: true)]
        public Dictionary<int, int> AgeModifier { get; private set; } = new();

        [DataField(required: true)]
        public SkillCloningPenaltyEntry CloningConfig { get; private set; } = new();
    }

    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class SkillCloningPenaltyEntry
    {
        [DataField] public Dictionary<ProtoId<DamageTypePrototype>, Dictionary<float, float>> CloningPenalties { get; private set; } = new();
        [DataField("forgetWeights")] public Dictionary<int, float> SkillsForgettingCountWeights { get; private set; } = new();
    }
}
