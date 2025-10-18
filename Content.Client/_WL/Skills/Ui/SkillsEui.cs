using System.Numerics;
using Content.Client.Eui;
using Content.Shared._WL.Skills.UI;
using Content.Shared.Eui;
using JetBrains.Annotations;
using Robust.Client.Graphics;

namespace Content.Client._WL.Skills.Ui;

[UsedImplicitly]
public sealed class SkillsEui : BaseEui
{
    private readonly SkillsForcedWindow _window;

    public SkillsEui()
    {
        _window = new SkillsForcedWindow();

        _window.OnClose += () =>
        {
            SendMessage(new SkillsEuiClosedMessage());
        };

        _window.OnSkillChanged += (jobId, skillKey, newLevel) =>
        {
            SendMessage(new SkillsEuiSkillChangedMessage(jobId, skillKey, newLevel));
        };
    }

    public override void Opened()
    {
        IoCManager.Resolve<IClyde>().RequestWindowAttention();
        _window.OpenCenteredAt(new Vector2(0.5f, 0.75f));
    }

    public override void Closed()
    {
        _window.Close();
    }

    public override void HandleState(EuiStateBase state)
    {
        base.HandleState(state);

        if (state is SkillsEuiState skillsState)
        {
            _window.UpdateState(skillsState);
        }
    }
}