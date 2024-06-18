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
        /// По безопасности должно быть норм.
        /// </summary>
        [AutoNetworkedField]
        [DataField]
        public Dictionary<ProtoId<SkillPrototype>, SkillLevel> Skills = new();

        [DataField(serverOnly: true)]
        public Dictionary<string, TimeSpan> ChangedSkills = new();

        /// <summary>
        /// Вероятность того, что после клонирования игрок ЗАБУДЕТ какой-то навык
        /// </summary>
        [DataField]
        public float CloningNoPenaltyProbability = 0.9f;
    }
}
