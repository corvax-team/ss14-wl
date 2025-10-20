using Content.Client.Eui;
using Content.Shared._WL.Skills.UI;
using Content.Shared.Eui;
using JetBrains.Annotations;

namespace Content.Client._WL.Administration.UI;

[UsedImplicitly]
public sealed class SkillsAdminEui : BaseEui
{
    private readonly SkillsAdminWindow _window;

    public SkillsAdminEui()
    {
        _window = new SkillsAdminWindow();

        _window.OnClose += () => SendMessage(new SkillsAdminEuiClosedMessage());
        _window.OnSkillChanged += (skillKey, newLevel) =>
            SendMessage(new SkillsAdminEuiSkillChangedMessage(skillKey, newLevel));
        _window.OnPointsChanged += (newBonus) =>
            SendMessage(new SkillsAdminEuiPointsChangedMessage(newBonus));
        _window.OnResetAll += () => SendMessage(new SkillsAdminEuiResetMessage());
    }

    public override void Opened()
    {
        _window.OpenCentered();
    }

    public override void Closed()
    {
        _window.Close();
    }

    public override void HandleState(EuiStateBase state)
    {
        base.HandleState(state);

        if (state is SkillsAdminEuiState adminState)
        {
            _window.UpdateState(adminState);
        }
    }
}
