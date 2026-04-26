using Content.Shared._WL.Stakc;
using Content.Shared.Stacks;
using JetBrains.Annotations;
using Robust.Client.Player;
using Robust.Client.UserInterface;

namespace Content.Client._WL.Stack.UI;

[UsedImplicitly]
public sealed class StackAmountBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private StackAmountWindow? _window;

    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<StackAmountWindow>();

        if (EntMan.TryGetComponent<StackComponent>(Owner, out var comp))
            _window.SetBounds(1, comp.Count);

        _window.ApplyButton.OnPressed += _ =>
        {
            if (int.TryParse(_window.AmountLineEdit.Text, out var i))
            {
                if (!_player.LocalEntity.HasValue)
                    return;

                if (!_ent.TryGetNetEntity(_player.LocalEntity.Value, out var netEntity))
                    return;

                SendMessage(new StakcAmountSetValueMessage(i, netEntity.Value));
                _window.Close();
            }
        };
    }
}
