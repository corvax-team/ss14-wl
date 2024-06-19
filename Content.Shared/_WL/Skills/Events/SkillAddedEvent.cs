using Content.Shared._WL.Skills.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Skills.Events
{
    [ByRefEvent]
    public readonly record struct SkillAddedEvent(
        Entity<SkillsHolderComponent> Target,
        ProtoId<SkillPrototype> Prototype,
        SkillLevel Level);
}
