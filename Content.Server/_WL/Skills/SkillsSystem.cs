using Content.Server._WL.Skills.UI;
using Content.Server.EUI;
using Content.Shared._WL.Skills;
using Content.Shared._WL.Skills.Components;
using Content.Shared.CCVar;
using Content.Shared.Mind;
using Content.Shared.Roles.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Server._WL.Skills;

public sealed partial class SkillsSystem : SharedSkillsSystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly EuiManager _eui = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<SelectSkillPressedEvent>(OnSelectSkill);
        SubscribeLocalEvent<SkillsComponent, SkillsAddedEvent>(OnSkillsAdded);
    }

    private void OnSelectSkill(SelectSkillPressedEvent args)
    {
        TrySetSkillLevel(GetEntity(args.Uid), args.Skill, args.TargetLevel, args.JobId);
    }

    private void OnSkillsAdded(EntityUid uid, SkillsComponent component, SkillsAddedEvent args)
    {
        // Disable AutoOpening for Development
        if (!_cfg.GetCVar(CCVars.GameLobbyEnabled))
            return;

        if (!_mind.TryGetMind(uid, out _, out var mind) || mind is { UserId: null }
            || !_player.TryGetSessionById(mind.UserId, out var session))
            return;

        var jobId = GetJobIdFromEntity(mind);
        if (ShouldForceSkillsSelection(uid, jobId, component))
        {
            OpenForcedSkillsMenu(session, uid, jobId);
        }
    }

    private string? GetJobIdFromEntity(MindComponent mind)
    {
        foreach (var roleId in mind.MindRoleContainer.ContainedEntities)
        {
            if (!TryComp<MindRoleComponent>(roleId, out var role))
                continue;

            if (role.JobPrototype != null)
            {
                return role.JobPrototype.Value;
            }
        }

        return null;
    }

    public void OpenForcedSkillsMenu(ICommonSession player, EntityUid entity, string? jobId)
    {
        jobId ??= "unknown";

        var eui = new SkillsEui(entity, this, jobId);
        _eui.OpenEui(eui, player);

        eui.StateDirty();
    }
}
