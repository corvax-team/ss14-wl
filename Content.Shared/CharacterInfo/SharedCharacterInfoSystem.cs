using Content.Shared._WL.Skills;
using Content.Shared.Objectives;
using Robust.Shared.Serialization;

namespace Content.Shared.CharacterInfo;

[Serializable, NetSerializable]
public sealed class RequestCharacterInfoEvent : EntityEventArgs
{
    public readonly NetEntity NetEntity;

    public RequestCharacterInfoEvent(NetEntity netEntity)
    {
        NetEntity = netEntity;
    }
}

[Serializable, NetSerializable]
public sealed class CharacterInfoEvent : EntityEventArgs
{
    public readonly NetEntity NetEntity;
    public readonly string JobTitle;
    public readonly Dictionary<string, List<ObjectiveInfo>> Objectives;
    public readonly string? Briefing;

    //WL-Skills-start
    public readonly List<SkillInfo> Skills;
    //WL-Skills-end

    public CharacterInfoEvent(
        NetEntity netEntity,
        string jobTitle,
        Dictionary<string,List<ObjectiveInfo>> objectives,
        string? briefing,
        /*WL-Skills-start*/List<SkillInfo> skills/*WL-Skills-end*/)
    {
        NetEntity = netEntity;
        JobTitle = jobTitle;
        Objectives = objectives;
        Briefing = briefing;

        //WL-Skills-start
        Skills = skills;
        //WL-Skills-end
    }
}
