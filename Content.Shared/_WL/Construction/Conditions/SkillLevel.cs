using Content.Shared._WL.Skills;
using Content.Shared._WL.Skills.Components;
using Content.Shared._WL.Skills.Systems;
using Content.Shared.Construction;
using Content.Shared.Construction.Conditions;
using Content.Shared.Construction.Prototypes;
using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using System.Text;

namespace Content.Shared._WL.Construction.Conditions
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class SkillLevel : IConstructionCondition
    {
        [DataField("need")]
        public Dictionary<ProtoId<SkillPrototype>, Skills.SkillLevel> NeedsSkillLevels = new();

        private SharedSkillsSystem? _skills = null;

        public ConstructionGuideEntry? GenerateGuideEntry() => null;

        public bool Condition(EntityUid user, EntityCoordinates location, Direction direction)
        {
            var entMan = IoCManager.Resolve<IEntityManager>();
            _skills ??= entMan.System<SharedSkillsSystem>();

            if (!entMan.TryGetComponent<SkillsHolderComponent>(user, out var skillsHolder))
                return true;

            foreach (var skillPair in NeedsSkillLevels)
            {
                if (!_skills.HasSkillMin((user, skillsHolder), skillPair.Key, skillPair.Value))
                    return false;
            }

            return true;
        }
    }
}
