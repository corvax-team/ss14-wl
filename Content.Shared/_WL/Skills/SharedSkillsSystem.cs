using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._WL.Skills;

public abstract partial class SharedSkillsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeSkills();
    }

    /// <summary>
    /// Gets a skill prototype.
    /// </summary>
    public SkillPrototype GetSkillPrototype(SkillType skill)
    {
        var skillId = GetSkillPrototypeId(skill);
        return _prototype.Index<SkillPrototype>(skillId);
    }

    /// <summary>
    /// Gets the prototype ID for SkillType.
    /// </summary>
    public string GetSkillPrototypeId(SkillType skill)
    {
        foreach (var skillProto in _prototype.EnumeratePrototypes<SkillPrototype>())
        {
            if (skillProto.SkillType == skill)
                return skillProto.ID;
        }

        throw new ArgumentException($"No skill prototype found for SkillType: {skill}");
    }

    /// <summary>
    /// Gets an array of costs for a skill.
    /// </summary>
    public int[] GetSkillCost(SkillType skill)
    {
        var prototype = GetSkillPrototype(skill);
        return prototype.Costs;
    }

    /// <summary>
    /// Gets a color for the skill.
    /// </summary>
    public Color GetSkillColor(SkillType skill)
    {
        var prototype = GetSkillPrototype(skill);
        return prototype.Color;
    }
}
