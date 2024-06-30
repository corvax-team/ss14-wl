using Robust.Shared.Serialization;

namespace Content.Shared._WL.Skills
{
    [NetSerializable, Serializable]
    public readonly record struct SkillInfo(
        Color Color,
        SkillLevel Level,
        string Name,
        Dictionary<SkillLevel, string> Descriptions);
}
