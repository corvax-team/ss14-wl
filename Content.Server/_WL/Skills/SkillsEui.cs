using System.Linq;
using Content.Server.EUI;
using Content.Shared._WL.Skills;
using Content.Shared._WL.Skills.Components;
using Content.Shared._WL.Skills.UI;
using Content.Shared.Eui;

namespace Content.Server._WL.Skills.UI;

public sealed class SkillsEui : BaseEui
{
    [Dependency] private readonly IEntityManager _entMan = default!;

    private readonly EntityUid _entity;
    private readonly SharedSkillsSystem _skillsSystem;
    private readonly string _jobId;

    public SkillsEui(EntityUid entity, SharedSkillsSystem skillsSystem, string jobId)
    {
        IoCManager.InjectDependencies(this);

        _entity = entity;
        _skillsSystem = skillsSystem;
        _jobId = jobId;
    }

    public override EuiStateBase GetNewState()
    {
        if (!_entMan.TryGetComponent<SkillsComponent>(_entity, out var skillsComp))
            return new SkillsEuiState(_jobId, new(), new(), 0, 0);

        var currentSkills = skillsComp.Skills.ToDictionary(
            kvp => (byte)kvp.Key,
            kvp => kvp.Value
        );

        var defaultSkills = _skillsSystem.GetDefaultSkillsForJob(_jobId);

        return new SkillsEuiState(_jobId, currentSkills, defaultSkills,
            _skillsSystem.GetTotalPoints(_entity, _jobId, skillsComp), skillsComp.SpentPoints);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        switch (msg)
        {
            case SkillsEuiClosedMessage:
                Close();
                break;

            case SkillsEuiSkillChangedMessage changedMsg:
                HandleSkillChanged(changedMsg);
                break;
        }
    }

    private void HandleSkillChanged(SkillsEuiSkillChangedMessage message)
    {
        if (!_entMan.TryGetComponent<SkillsComponent>(_entity, out var skillsComp))
            return;

        var skillType = (SkillType)message.SkillKey;
        _skillsSystem.TrySetSkillLevel(_entity, skillType, message.NewLevel, _jobId, skillsComp);

        StateDirty();
    }
}
