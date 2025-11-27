using System.Linq;
using Content.Server.EUI;
using Content.Server.Prayer;
using Content.Shared._WL.Skills;
using Content.Shared._WL.Skills.Components;
using Content.Shared._WL.Skills.UI;
using Content.Shared.Eui;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.Administration.Systems;

public sealed class SkillsAdminEui : BaseEui
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private readonly EntityUid _targetEntity;
    private readonly SharedSkillsSystem _skillsSystem;
    private readonly PrayerSystem _prayer;
    private SkillsComponent? _skillsComp;

    public SkillsAdminEui(EntityUid targetEntity)
    {
        IoCManager.InjectDependencies(this);

        _targetEntity = targetEntity;
        _skillsSystem = _entMan.System<SharedSkillsSystem>();
        _prayer = _entMan.System<PrayerSystem>();
        _entMan.TryGetComponent(_targetEntity, out _skillsComp);
    }

    public override EuiStateBase GetNewState()
    {
        if (_skillsComp == null)
            return new SkillsAdminEuiState(false, new(), 0, 0, "", GetEntityName(_targetEntity));

        var currentSkills = _skillsComp.Skills.ToDictionary(
            kvp => (byte)kvp.Key,
            kvp => kvp.Value
        );

        var defaultSkills = _skillsSystem.GetDefaultSkillsForJob(_skillsComp.CurrentJob);

        var jobName = Loc.GetString("skills-admin-skills-no-job");
        if (_proto.TryIndex(_skillsComp.CurrentJob, out var jobPrototype))
            jobName = jobPrototype.LocalizedName;

        return new SkillsAdminEuiState(
            true,
            currentSkills,
            _skillsComp.SpentPoints,
            _skillsComp.BonusPoints,
            jobName,
            GetEntityName(_targetEntity)
        );
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        switch (msg)
        {
            case SkillsAdminEuiClosedMessage:
                Close();
                break;

            case SkillsAdminEuiSkillChangedMessage changedMsg:
                HandleSkillChanged(changedMsg);
                break;

            case SkillsAdminEuiPointsChangedMessage pointsMsg:
                HandlePointsChanged(pointsMsg);
                break;

            case SkillsAdminEuiResetMessage:
                HandleResetAll();
                break;
        }
    }

    private void HandleSkillChanged(SkillsAdminEuiSkillChangedMessage message)
    {
        if (_skillsComp == null)
            return;

        var defaultSkills = _skillsSystem.ConvertToSkillTypeDict(_skillsSystem.GetDefaultSkillsForJob(_skillsComp.CurrentJob));

        var skillType = (SkillType)message.SkillKey;
        _skillsSystem.SetSkillLevelAdmin(_targetEntity, skillType, message.NewLevel, defaultSkills, _skillsComp);

        if (_entMan.TryGetComponent<ActorComponent>(_targetEntity, out var actor))
            _prayer.SendSubtleMessage(actor.PlayerSession, actor.PlayerSession, string.Empty,
                Loc.GetString("skills-admin-notify-skills-changed"));

        StateDirty();
    }

    private void HandlePointsChanged(SkillsAdminEuiPointsChangedMessage message)
    {
        if (_skillsComp == null)
            return;

        var totalBonusPoints = _skillsComp.BonusPoints;
        _skillsSystem.SetBonusPoints(_targetEntity, message.NewBonusPoints, _skillsComp);
        if (_entMan.TryGetComponent<ActorComponent>(_targetEntity, out var actor))
        {
            var messageKey = message.NewBonusPoints > totalBonusPoints
                ? "skills-admin-notify-points-added" : "skills-admin-notify-points-removed";

            _prayer.SendSubtleMessage(actor.PlayerSession, actor.PlayerSession, string.Empty,
                Loc.GetString(messageKey));
        }

        StateDirty();
    }

    private void HandleResetAll()
    {
        if (_skillsComp == null)
            return;

        _skillsSystem.ResetAllSkills(_targetEntity, _skillsComp);

        if (_entMan.TryGetComponent<ActorComponent>(_targetEntity, out var actor))
            _prayer.SendSubtleMessage(actor.PlayerSession, actor.PlayerSession, string.Empty,
                Loc.GetString("skills-admin-notify-skills-reset"));

        StateDirty();
    }

    private string GetEntityName(EntityUid entity)
    {
        return _entMan.GetComponent<MetaDataComponent>(entity).EntityName;
    }
}

