using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Skills.Prototypes
{
    [Prototype("skillsConfiguration")]
    public sealed partial class SkillsConfigurationPrototype : IPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField(required: true)]
        public Dictionary<int, int> AgeModifier { get; private set; } = new();
    }
}
