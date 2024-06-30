using Content.Server.Mind;
using Content.Server.Roles;
using Content.Server.Roles.Jobs;
using Content.Shared._WL.Skills;
using Content.Shared._WL.Skills.Components;
using Content.Shared.CharacterInfo;
using Content.Shared.Objectives;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Server.CharacterInfo;

public sealed class CharacterInfoSystem : EntitySystem
{
    [Dependency] private readonly JobSystem _jobs = default!;
    [Dependency] private readonly MindSystem _minds = default!;
    [Dependency] private readonly RoleSystem _roles = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;
    //WL-Skills-start
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    //WL-Skills-end

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<RequestCharacterInfoEvent>(OnRequestCharacterInfoEvent);
    }

    private void OnRequestCharacterInfoEvent(RequestCharacterInfoEvent msg, EntitySessionEventArgs args)
    {
        if (!args.SenderSession.AttachedEntity.HasValue
            || args.SenderSession.AttachedEntity != GetEntity(msg.NetEntity))
            return;

        var entity = args.SenderSession.AttachedEntity.Value;

        var objectives = new Dictionary<string, List<ObjectiveInfo>>();
        var jobTitle = "No Profession";
        string? briefing = null;
        if (_minds.TryGetMind(entity, out var mindId, out var mind))
        {
            // Get objectives
            foreach (var objective in mind.AllObjectives)
            {
                var info = _objectives.GetInfo(objective, mindId, mind);
                if (info == null)
                    continue;

                // group objectives by their issuer
                var issuer = Comp<ObjectiveComponent>(objective).Issuer;
                if (!objectives.ContainsKey(issuer))
                    objectives[issuer] = new List<ObjectiveInfo>();
                objectives[issuer].Add(info.Value);
            }

            if (_jobs.MindTryGetJob(mindId, out _, out var jobProto))
                jobTitle = _roles.GetSubnameByEntity(mindId, jobProto.ID) ?? jobProto.LocalizedName;

            // Get briefing
            briefing = _roles.MindGetBriefing(mindId);
        }

        //WL-Skills-start
        var skillInfos = new List<SkillInfo>();
        if (TryComp<SkillsHolderComponent>(entity, out var skillsHolderComp))
        {
            foreach (var skill in skillsHolderComp.Skills)
            {
                if (!_protoMan.TryIndex(skill.Key, out var skillProto))
                    continue;

                var skillDescs = skillProto.Info.ToDictionary(k => k.Key, v => v.Value.Description);

                var info = new SkillInfo(
                    skillProto.NameColor ?? Color.White,
                    skill.Value,
                    skillProto.Name,
                    new(skillDescs));

                skillInfos.Add(info);
            }
        }
        //wl-Skills-end

        RaiseNetworkEvent(new CharacterInfoEvent(
            GetNetEntity(entity),
            jobTitle,
            objectives,
            briefing,
            skillInfos), args.SenderSession);
    }
}
