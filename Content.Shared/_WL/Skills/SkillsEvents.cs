using Robust.Shared.Serialization;

namespace Content.Shared._WL.Skills;

[ByRefEvent]
public record struct SkillsAddedEvent();

[Serializable, NetSerializable]
public sealed class SelectSkillPressedEvent(NetEntity uid, SkillType skill, int targetLevel, string? jobId = null) : EntityEventArgs
{
    public NetEntity Uid { get; } = uid;
    public SkillType Skill { get; } = skill;
    public int TargetLevel { get; } = targetLevel;
    public string? JobId { get; } = jobId;
}
