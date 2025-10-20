using Content.Shared._WL.CCVars;
using Content.Shared._WL.Skills.Components;
using Robust.Shared.Random;

namespace Content.Shared._WL.Skills;

public abstract partial class SharedSkillsSystem
{
    /// <summary>
    /// Checks whether the entity has a sufficient skill level to perform the action.
    /// </summary>
    /// <param name="uid">ID entities</param>
    /// <param name="skill">Required skill</param>
    /// <param name="requiredLevel">Required minimum level (1-4)</param>
    /// <param name="comp">An optional skill component</param>
    /// <returns>True if the skill is sufficient</returns>
    public bool HasSkill(EntityUid uid, SkillType skill, int requiredLevel = 1, SkillsComponent? comp = null)
    {
        if (!_cfg.GetCVar(WLCVars.SkillsEnabled))
            return true;

        if (!Resolve(uid, ref comp))
            return false;

        var currentLevel = comp.Skills.GetValueOrDefault(skill, 1);
        return currentLevel >= requiredLevel;
    }

    /// <summary>
    /// Checks if the entity has the correct skill level
    /// </summary>
    public bool HasExactSkill(EntityUid uid, SkillType skill, int exactLevel, SkillsComponent? comp = null)
    {
        if (!_cfg.GetCVar(WLCVars.SkillsEnabled))
            return true;

        if (!Resolve(uid, ref comp))
            return false;

        var currentLevel = comp.Skills.GetValueOrDefault(skill, 1);
        return currentLevel == exactLevel;
    }

    /// <summary>
    /// Gets the current skill level of the entity.
    /// </summary>
    public int GetSkillLevel(EntityUid uid, SkillType skill, SkillsComponent? comp = null)
    {
        if (!_cfg.GetCVar(WLCVars.SkillsEnabled))
            return 1;

        if (!Resolve(uid, ref comp))
            return 1;

        return comp.Skills.GetValueOrDefault(skill, 1);
    }

    /// <summary>
    /// Checks whether an entity can perform an action based on the skill's success chance.
    /// </summary>
    public bool CheckSkillChance(EntityUid uid, SkillType skill, float baseSuccessChance = 0.5f, SkillsComponent? comp = null)
    {
        if (!_cfg.GetCVar(WLCVars.SkillsEnabled))
            return true;

        if (!Resolve(uid, ref comp))
            return _random.Prob(baseSuccessChance); // Basic probability if there are no skills

        var skillLevel = comp.Skills.GetValueOrDefault(skill, 1);

        // Модификатор based on уровня навыка
        float skillModifier = skillLevel switch
        {
            1 => 0.8f, // Beginner - fine
            2 => 1.0f, // Amateur - basic
            3 => 1.3f, // Specialist - bonus
            4 => 1.7f, // Professional - big bonus
            _ => 1.0f
        };

        float successChance = baseSuccessChance * skillModifier;
        return _random.Prob(Math.Clamp(successChance, 0f, 1f));
    }

    /// <summary>
    /// Gets an efficiency modifier based on the skill level.
    /// </summary>
    public float GetSkillEfficiency(EntityUid uid, SkillType skill, SkillsComponent? comp = null)
    {
        if (!_cfg.GetCVar(WLCVars.SkillsEnabled))
            return 1.0f;

        if (!Resolve(uid, ref comp))
            return 1.0f; // Basic efficiency

        var skillLevel = comp.Skills.GetValueOrDefault(skill, 1);

        return skillLevel switch
        {
            1 => 0.7f, // Beginner - 70% efficiency
            2 => 1.0f, // Amateur - 100%
            3 => 1.4f, // Specialist - 140%
            4 => 1.8f, // Professional - 180%
            _ => 1.0f
        };
    }

    /// <summary>
    /// Checks whether an entity can perform a complex action (requires multiple skills)
    /// </summary>
    public bool CanPerformComplexAction(EntityUid uid, SkillsComponent? comp = null, params (SkillType skill, int requiredLevel)[] requirements)
    {
        if (!_cfg.GetCVar(WLCVars.SkillsEnabled))
            return true;

        if (!Resolve(uid, ref comp))
            return false; // Can't perform without skills

        foreach (var (skill, requiredLevel) in requirements)
        {
            if (!HasSkill(uid, skill, requiredLevel, comp))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Gets a description of the skill level for the UI.
    /// </summary>
    public string GetSkillLevelDescription(int level)
    {
        return level switch
        {
            1 => Loc.GetString("skill-level-1"),
            2 => Loc.GetString("skill-level-2"),
            3 => Loc.GetString("skill-level-3"),
            4 => Loc.GetString("skill-level-4"),
            _ => Loc.GetString("skill-level-unknown")
        };
    }

    /// <summary>
    /// Gets a color to display the skill level.
    /// </summary>
    public Color GetSkillLevelColor(int level)
    {
        return level switch
        {
            1 => Color.Gray,     // Beginner
            2 => Color.Yellow,   // Amateur
            3 => Color.Blue,     // Specialist
            4 => Color.Green,    // Professional
            _ => Color.White
        };
    }
}
