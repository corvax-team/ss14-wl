using Content.Shared._WL.CCVars;
using Content.Shared._WL.Skills.Components;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Configuration;

namespace Content.Shared._WL.Skills;

public abstract partial class SharedSkillsSystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    #region Initialization

    public void InitializeSkills()
    {
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        SubscribeLocalEvent<InitialSkillsComponent, MapInitEvent>(OnMapInit);
    }

    /// <summary>
    /// Initializes skills for a player when they spawn
    /// </summary>
    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        if (args.Profile is not HumanoidCharacterProfile profile || !_cfg.GetCVar(WLCVars.SkillsEnabled))
            return;

        InitializeSkills(args.Mob, args.JobId, profile);
    }

    /// <summary>
    /// Initializes skills component with default values and applies profile skills
    /// </summary>
    private void InitializeSkills(EntityUid mob, string? jobId, HumanoidCharacterProfile profile)
    {
        if (!_cfg.GetCVar(WLCVars.SkillsEnabled))
            return;

        var skillsComp = EnsureComp<SkillsComponent>(mob);
        skillsComp.Skills.Clear();

        skillsComp.CurrentJob = jobId;

        int bonusPoints = 0;
        Dictionary<SkillType, int> defaultSkills = new();
        if (jobId != null && _prototype.TryIndex<JobPrototype>(jobId, out var jobPrototype))
        {
            defaultSkills = jobPrototype.DefaultSkills;
            bonusPoints = jobPrototype.BonusSkillPoints;
        }

        var racialBonus = CalculateRacialBonus(profile.Species, profile.Age);
        var totalPoints = bonusPoints + racialBonus;
        foreach (SkillType skill in Enum.GetValues(typeof(SkillType)))
        {
            var defaultLevel = defaultSkills.GetValueOrDefault(skill, 1);
            skillsComp.Skills[skill] = defaultLevel;
        }

        if (jobId != null && profile.Skills.TryGetValue(jobId, out var profileSkills))
        {
            ApplyProfileSkillsWithLimit(mob, skillsComp, profileSkills, defaultSkills, totalPoints);
        }

        RecalculateSpentPoints(mob, skillsComp, defaultSkills);
        skillsComp.UnspentPoints = Math.Max(0, totalPoints - skillsComp.SpentPoints);

        Dirty(mob, skillsComp);

        var ev = new SkillsAddedEvent();
        RaiseLocalEvent(mob, ref ev);
    }

    #endregion

    #region Skill Calculation Methods

    /// <summary>
    /// Recalculates the total number of points spent for all skills, excluding default skill costs
    /// </summary>
    public void RecalculateSpentPoints(EntityUid uid, SkillsComponent? comp = null, Dictionary<SkillType, int>? defaultSkills = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.SpentPoints = 0;

        foreach (var (skill, level) in comp.Skills)
        {
            var defaultLevel = defaultSkills?.GetValueOrDefault(skill, 1) ?? 1;
            if (level > defaultLevel)
            {
                comp.SpentPoints += GetSkillTotalCost(skill, level) - GetSkillTotalCost(skill, defaultLevel);
            }
        }
    }

    /// <summary>
    /// Recalculates spent points using job defaults
    /// </summary>
    public void RecalculateSpentPoints(EntityUid uid, string? jobId, SkillsComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        var defaultSkills = GetDefaultSkillsForJob(jobId);
        RecalculateSpentPoints(uid, comp, ConvertToSkillTypeDict(defaultSkills));
    }

    /// <summary>
    /// Converts byte-based dictionary to SkillType-based dictionary
    /// </summary>
    public Dictionary<SkillType, int> ConvertToSkillTypeDict(Dictionary<byte, int> byteDict)
    {
        var result = new Dictionary<SkillType, int>();
        foreach (var (skillKey, level) in byteDict)
        {
            if (Enum.IsDefined(typeof(SkillType), (SkillType)skillKey))
            {
                result[(SkillType)skillKey] = level;
            }
        }
        return result;
    }

    /// <summary>
    /// Calculates the cost of increasing a skill to the specified level
    /// </summary>
    public int GetSkillTotalCost(SkillType skill, int targetLevel)
    {
        var costs = GetSkillCost(skill);
        targetLevel = Math.Clamp(targetLevel, 1, 4);

        int totalCost = 0;
        for (int i = 0; i < targetLevel; i++)
        {
            totalCost += costs[i];
        }
        return totalCost;
    }

    /// <summary>
    /// Calculates the cost of the current skill level
    /// </summary>
    public int GetCurrentSkillCost(SkillType skill, int currentLevel)
    {
        return GetSkillTotalCost(skill, currentLevel);
    }

    /// <summary>
    /// Calculates the cost of increasing a skill by one level
    /// </summary>
    public int GetSkillUpgradeCost(SkillType skill, int currentLevel)
    {
        var costs = GetSkillCost(skill);
        if (currentLevel >= 4) return 0;
        return costs[currentLevel];
    }

    #endregion

    #region Skill Application Methods

    /// <summary>
    /// Applies skills from the profile, taking into account the point limit
    /// </summary>
    private void ApplyProfileSkillsWithLimit(EntityUid uid, SkillsComponent skillsComp,
        Dictionary<byte, int> profileSkills, Dictionary<SkillType, int> defaultSkills, int totalPoints)
    {
        var spentPoints = 0;
        var tempSkills = new Dictionary<SkillType, int>(skillsComp.Skills);

        foreach (var (skillKey, level) in profileSkills)
        {
            var skillType = (SkillType)skillKey;
            if (!Enum.IsDefined(typeof(SkillType), skillType))
                continue;

            var clampedLevel = Math.Clamp(level, 1, 4);
            var defaultLevel = defaultSkills.GetValueOrDefault(skillType, 1);

            if (clampedLevel > defaultLevel)
            {
                var cost = GetSkillTotalCost(skillType, clampedLevel) - GetSkillTotalCost(skillType, defaultLevel);
                if (spentPoints + cost <= totalPoints)
                {
                    tempSkills[skillType] = clampedLevel;
                    spentPoints += cost;
                }
                else
                {
                    var maxAffordableLevel = FindMaxAffordableLevel(skillType, defaultLevel, totalPoints - spentPoints);
                    if (maxAffordableLevel > defaultLevel)
                    {
                        tempSkills[skillType] = maxAffordableLevel;
                        spentPoints += GetSkillTotalCost(skillType, maxAffordableLevel) - GetSkillTotalCost(skillType, defaultLevel);
                    }
                    else
                    {
                        tempSkills[skillType] = defaultLevel;
                    }
                }
            }
            else
            {
                tempSkills[skillType] = clampedLevel;
            }
        }

        skillsComp.Skills = tempSkills;
    }

    /// <summary>
    /// Finds the maximum available skill level within the budget
    /// </summary>
    private int FindMaxAffordableLevel(SkillType skill, int currentLevel, int availablePoints)
    {
        var maxLevel = currentLevel;
        var currentCost = 0;

        for (int targetLevel = currentLevel + 1; targetLevel <= 4; targetLevel++)
        {
            var additionalCost = GetSkillUpgradeCost(skill, targetLevel - 1);
            if (currentCost + additionalCost <= availablePoints)
            {
                currentCost += additionalCost;
                maxLevel = targetLevel;
            }
            else
            {
                break;
            }
        }

        return maxLevel;
    }

    /// <summary>
    /// Sets skill level with automatic detection of increase/decrease and default level check
    /// </summary>
    public bool TrySetSkillLevel(EntityUid uid, SkillType skill, int targetLevel, string? jobId = null, SkillsComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return false;

        targetLevel = Math.Clamp(targetLevel, 1, 4);
        var currentLevel = comp.Skills.GetValueOrDefault(skill, 1);
        if (currentLevel == targetLevel)
            return true;

        var defaultLevel = GetDefaultSkillLevelForJob(skill, jobId);
        if (targetLevel < defaultLevel)
            return false;

        if (targetLevel > currentLevel)
        {
            return TryIncreaseToLevel(uid, skill, targetLevel, comp, defaultLevel, jobId);
        }
        else
        {
            return TryDecreaseToLevel(uid, skill, targetLevel, comp, defaultLevel, jobId);
        }
    }

    /// <summary>
    /// Increases skill to specified level
    /// </summary>
    private bool TryIncreaseToLevel(EntityUid uid, SkillType skill, int targetLevel,
        SkillsComponent comp, int defaultLevel, string? jobId = null)
    {
        var currentLevel = comp.Skills.GetValueOrDefault(skill, 1);

        var totalCost = 0;
        for (int level = Math.Max(currentLevel, defaultLevel); level < targetLevel; level++)
        {
            totalCost += GetSkillUpgradeCost(skill, level);
        }

        if (comp.UnspentPoints < totalCost)
            return false;

        comp.UnspentPoints -= totalCost;
        comp.Skills[skill] = targetLevel;
        RecalculateSpentPoints(uid, jobId, comp);

        Dirty(uid, comp);

        return true;
    }

    /// <summary>
    /// Decreases skill to specified level
    /// </summary>
    private bool TryDecreaseToLevel(EntityUid uid, SkillType skill, int targetLevel,
        SkillsComponent comp, int defaultLevel, string? jobId = null)
    {
        var currentLevel = comp.Skills.GetValueOrDefault(skill, 1);

        var totalRefund = 0;
        for (int level = targetLevel; level < Math.Min(currentLevel, 4); level++)
        {
            if (level >= defaultLevel)
            {
                totalRefund += GetSkillUpgradeCost(skill, level);
            }
        }

        comp.UnspentPoints += totalRefund;
        comp.Skills[skill] = targetLevel;
        RecalculateSpentPoints(uid, jobId, comp);

        Dirty(uid, comp);

        return true;
    }

    #endregion

    #region Default Skills Methods

    /// <summary>
    /// Gets default skill level for job
    /// </summary>
    public int GetDefaultSkillLevelForJob(SkillType skill, string? jobId)
    {
        if (jobId != null && _prototype.TryIndex<JobPrototype>(jobId, out var jobPrototype))
        {
            return jobPrototype.DefaultSkills.GetValueOrDefault(skill, 1);
        }

        return 1; // Default base level
    }

    /// <summary>
    /// Gets default skills for job
    /// </summary>
    public Dictionary<byte, int> GetDefaultSkillsForJob(string? jobId)
    {
        var defaultSkills = new Dictionary<byte, int>();
        foreach (SkillType skill in Enum.GetValues(typeof(SkillType)))
        {
            defaultSkills[(byte)skill] = 1;
        }

        if (jobId != null && _prototype.TryIndex<JobPrototype>(jobId, out var jobPrototype))
        {
            foreach (var (skill, level) in jobPrototype.DefaultSkills)
            {
                defaultSkills[(byte)skill] = level;
            }
        }

        return defaultSkills;
    }

    /// <summary>
    /// Checks if forced skills selection should be shown - only if ONLY default skills are set
    /// </summary>
    public bool ShouldForceSkillsSelection(EntityUid uid, string? jobId = null, SkillsComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return false;

        var defaultSkills = GetDefaultSkillsForJob(jobId);
        foreach (var (skill, level) in comp.Skills)
        {
            var defaultLevel = defaultSkills.GetValueOrDefault((byte)skill, 1);
            if (level != defaultLevel)
                return false;
        }

        return true;
    }

    #endregion

    #region Admin Methods

    /// <summary>
    /// Directly sets bonus points for admin control
    /// </summary>
    public void SetBonusPoints(EntityUid uid, int points, SkillsComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.BonusPoints = points;
        Dirty(uid, comp);
    }

    /// <summary>
    /// Directly sets skill level without point restrictions for admin control
    /// </summary>
    public void SetSkillLevelAdmin(EntityUid uid, SkillType skill, int level,
        Dictionary<SkillType, int>? defaultSkills, SkillsComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        level = Math.Clamp(level, 1, 4);
        comp.Skills[skill] = level;

        RecalculateSpentPoints(uid, comp, defaultSkills);
        Dirty(uid, comp);
    }

    /// <summary>
    /// Resets all skills to level 1 for admin control
    /// </summary>
    public void ResetAllSkills(EntityUid uid, SkillsComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        foreach (SkillType skill in Enum.GetValues(typeof(SkillType)))
        {
            comp.Skills[skill] = 1;
        }

        comp.UnspentPoints = 0;
        comp.SpentPoints = 0;
        comp.BonusPoints = 0;

        Dirty(uid, comp);
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Gets total points for entity
    /// </summary>
    public int GetTotalPoints(EntityUid uid, string? jobId = null,
        SkillsComponent? comp = null, HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref comp) || !Resolve(uid, ref humanoid))
            return 0;

        int bonusPoints = 0;
        if (jobId != null && _prototype.TryIndex<JobPrototype>(jobId, out var jobPrototype))
            bonusPoints = jobPrototype.BonusSkillPoints;

        var racialBonus = CalculateRacialBonus(humanoid.Species, humanoid.Age);
        return bonusPoints + racialBonus + comp.BonusPoints;
    }

    /// <summary>
    /// Calculates racial bonus of skill points
    /// </summary>
    private int CalculateRacialBonus(string species, int age)
    {
        var bonus = 0;
        foreach (var racialBonusProto in _prototype.EnumeratePrototypes<RacialSkillBonusPrototype>())
        {
            if (racialBonusProto.Species != species)
                continue;

            bonus = racialBonusProto.GetBonusForAge(age);
            break;
        }

        return bonus;
    }

    #endregion

    #region Initial Skills

    private void OnMapInit(EntityUid uid, InitialSkillsComponent component, MapInitEvent args)
    {
        if (!_cfg.GetCVar(WLCVars.SkillsEnabled))
            return;

        var skillsComp = EnsureComp<SkillsComponent>(uid);

        if (component.RandomizeAllSkills)
        {
            RandomizeAllSkills(uid, skillsComp, component.RandomMinLevel, component.RandomMaxLevel);
        }
        else
        {
            ApplyInitialSkills(uid, skillsComp, component);
        }

        RecalculateSpentPoints(uid, skillsComp);
        RemCompDeferred<InitialSkillsComponent>(uid);

        Dirty(uid, skillsComp);
    }

    /// <summary>
    /// Applies the initial skills from the component
    /// </summary>
    public void ApplyInitialSkills(EntityUid uid, SkillsComponent skillsComp, InitialSkillsComponent initial)
    {
        if (initial.OverrideExisting)
        {
            skillsComp.Skills.Clear();
            foreach (SkillType skill in Enum.GetValues(typeof(SkillType)))
            {
                skillsComp.Skills[skill] = 1;
            }
        }

        foreach (var (skill, level) in initial.InitialSkills)
        {
            skillsComp.Skills[skill] = Math.Clamp(level, 1, 4);
        }

        foreach (var skill in initial.RandomSkills)
        {
            var randomLevel = _random.Next(initial.RandomMinLevel, initial.RandomMaxLevel + 1);
            skillsComp.Skills[skill] = Math.Clamp(randomLevel, 1, 4);
        }

        foreach (var (skill, addLevel) in initial.AddSkills)
        {
            var currentLevel = skillsComp.Skills.GetValueOrDefault(skill, 1);
            skillsComp.Skills[skill] = Math.Clamp(currentLevel + addLevel, 1, 4);
        }
    }

    /// <summary>
    /// Randomizes all entity skills within the specified range
    /// </summary>
    public void RandomizeAllSkills(EntityUid uid, SkillsComponent? comp = null, int minLevel = 1, int maxLevel = 4)
    {
        if (!Resolve(uid, ref comp))
            return;

        minLevel = Math.Clamp(minLevel, 1, 4);
        maxLevel = Math.Clamp(maxLevel, 1, 4);

        foreach (SkillType skill in Enum.GetValues(typeof(SkillType)))
        {
            comp.Skills[skill] = _random.Next(minLevel, maxLevel + 1);
        }
    }

    /// <summary>
    /// Copies all skills and points from one entity to another
    /// </summary>
    public void CopySkills(EntityUid fromEntity, EntityUid toEntity, SkillsComponent? fromSkills = null)
    {
        if (!Resolve(fromEntity, ref fromSkills, false))
            return;

        var toSkills = EnsureComp<SkillsComponent>(toEntity);

        toSkills.Skills.Clear();
        foreach (var (skill, level) in fromSkills.Skills)
        {
            toSkills.Skills[skill] = level;
        }

        toSkills.UnspentPoints = fromSkills.UnspentPoints;
        toSkills.SpentPoints = fromSkills.SpentPoints;

        Dirty(toEntity, toSkills);

        Log.Debug($"Copied skills from {ToPrettyString(fromEntity)} to {ToPrettyString(toEntity)}");
    }

    #endregion
}
