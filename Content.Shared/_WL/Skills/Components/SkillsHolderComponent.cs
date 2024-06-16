using Content.Shared._WL.Skills.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Shared._WL.Skills.Components
{
    [RegisterComponent, NetworkedComponent]
    [AutoGenerateComponentState]
    [Access(typeof(SharedSkillsSystem))]
    public sealed partial class SkillsHolderComponent : Component
    {
        [AutoNetworkedField]
        [DataField(required: true, customTypeSerializer: typeof(PrototypeIdDictionarySerializer<SkillLevel, SkillPrototype>))]
        public Dictionary<string, SkillLevel> Skills = new();
    }
}
