using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Skills.UI;

[Serializable, NetSerializable]
public sealed class SkillsEuiState : EuiStateBase
{
    public readonly string JobId;
    public readonly Dictionary<byte, int> CurrentSkills;
    public readonly Dictionary<byte, int> DefaultSkills;
    public readonly int TotalPoints;
    public readonly int SpentPoints;

    public SkillsEuiState(string jobId, Dictionary<byte, int> currentSkills,
        Dictionary<byte, int> defaultSkills, int totalPoints, int spentPoints)
    {
        JobId = jobId;
        CurrentSkills = currentSkills;
        DefaultSkills = defaultSkills;
        TotalPoints = totalPoints;
        SpentPoints = spentPoints;
    }
}

[Serializable, NetSerializable]
public sealed class SkillsEuiClosedMessage : EuiMessageBase
{
}

[Serializable, NetSerializable]
public sealed class SkillsEuiSkillChangedMessage : EuiMessageBase
{
    public readonly string JobId;
    public readonly byte SkillKey;
    public readonly int NewLevel;

    public SkillsEuiSkillChangedMessage(string jobId, byte skillKey, int newLevel)
    {
        JobId = jobId;
        SkillKey = skillKey;
        NewLevel = newLevel;
    }
}

#region Admin
[Serializable, NetSerializable]
public sealed class SkillsAdminEuiState : EuiStateBase
{
    public readonly bool HasSkills;
    public readonly Dictionary<byte, int> CurrentSkills;
    public readonly int SpentPoints;
    public readonly int BonusPoints;
    public readonly string CurrentJob;
    public readonly string EntityName;

    public SkillsAdminEuiState(bool hasSkills, Dictionary<byte, int> currentSkills,
        int spentPoints, int bonusPoints, string currentJob, string entityName)
    {
        HasSkills = hasSkills;
        CurrentSkills = currentSkills;
        SpentPoints = spentPoints;
        BonusPoints = bonusPoints;
        CurrentJob = currentJob;
        EntityName = entityName;
    }
}

[Serializable, NetSerializable]
public sealed class SkillsAdminEuiClosedMessage : EuiMessageBase
{
}

[Serializable, NetSerializable]
public sealed class SkillsAdminEuiResetMessage : EuiMessageBase
{
}

[Serializable, NetSerializable]
public sealed class SkillsAdminEuiSkillChangedMessage : EuiMessageBase
{
    public readonly byte SkillKey;
    public readonly int NewLevel;

    public SkillsAdminEuiSkillChangedMessage(byte skillKey, int newLevel)
    {
        SkillKey = skillKey;
        NewLevel = newLevel;
    }
}

[Serializable, NetSerializable]
public sealed class SkillsAdminEuiPointsChangedMessage : EuiMessageBase
{
    public readonly int NewBonusPoints;

    public SkillsAdminEuiPointsChangedMessage(int newBonusPoints)
    {
        NewBonusPoints = newBonusPoints;
    }
}
#endregion
