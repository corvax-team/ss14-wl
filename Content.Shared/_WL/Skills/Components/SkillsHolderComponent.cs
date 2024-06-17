using Content.Shared._WL.Skills.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Skills.Components
{
    [RegisterComponent, NetworkedComponent]
    [AutoGenerateComponentState]
    [Access(typeof(SharedSkillsSystem))]
    public sealed partial class SkillsHolderComponent : Component
    {
        /// <summary>
        /// Словарь со скиллами.
        /// Синхронизируется с клиентом(вдруг для каких-то клиентских штучек понадобится.)
        /// </summary>
        [AutoNetworkedField]
        [DataField]
        public Dictionary<ProtoId<SkillPrototype>, SkillLevel> Skills = new();

        [DataField(serverOnly: true)]
        public Dictionary<string, TimeSpan> ChangedSkills = new();
    }
}
