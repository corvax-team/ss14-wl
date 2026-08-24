using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Skills.UI;

[Serializable, NetSerializable]
public sealed class SkillsEuiState(string jobId, Dictionary<byte, int> currentSkills,
        Dictionary<byte, int> defaultSkills, int totalPoints, int spentPoints) : EuiStateBase
{
    public readonly string JobId = jobId;
    public readonly Dictionary<byte, int> CurrentSkills = currentSkills;
    public readonly Dictionary<byte, int> DefaultSkills = defaultSkills;
    public readonly int TotalPoints = totalPoints;
    public readonly int SpentPoints = spentPoints;
}

[Serializable, NetSerializable]
public sealed class SkillsEuiClosedMessage : EuiMessageBase;

[Serializable, NetSerializable]
public sealed class SkillsEuiSkillChangedMessage(string jobId, byte skillKey, int newLevel) : EuiMessageBase
{
    public readonly string JobId = jobId;
    public readonly byte SkillKey = skillKey;
    public readonly int NewLevel = newLevel;
}

#region Admin
[Serializable, NetSerializable]
public sealed class SkillsAdminEuiState(bool hasSkills, Dictionary<byte, int> currentSkills,
        int spentPoints, int bonusPoints, string currentJob, string entityName) : EuiStateBase
{
    public readonly bool HasSkills = hasSkills;
    public readonly Dictionary<byte, int> CurrentSkills = currentSkills;
    public readonly int SpentPoints = spentPoints;
    public readonly int BonusPoints = bonusPoints;
    public readonly string CurrentJob = currentJob;
    public readonly string EntityName = entityName;
}

[Serializable, NetSerializable]
public sealed class SkillsAdminEuiClosedMessage : EuiMessageBase;

[Serializable, NetSerializable]
public sealed class SkillsAdminEuiResetMessage : EuiMessageBase;

[Serializable, NetSerializable]
public sealed class SkillsAdminEuiSkillChangedMessage(byte skillKey, int newLevel) : EuiMessageBase
{
    public readonly byte SkillKey = skillKey;
    public readonly int NewLevel = newLevel;
}

[Serializable, NetSerializable]
public sealed class SkillsAdminEuiPointsChangedMessage(int newBonusPoints) : EuiMessageBase
{
    public readonly int NewBonusPoints = newBonusPoints;
}
#endregion
