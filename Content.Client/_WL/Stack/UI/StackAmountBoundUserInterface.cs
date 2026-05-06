using Content.Shared._WL.Stacks;
using Content.Shared.Stacks;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._WL.Stack.UI;

[UsedImplicitly]
public sealed class StackAmountBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private StackAmountWindow? _window;

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

                SendMessage(new StackAmountSetValueMessage(i));
                _window.Close();
            }
        };
    }
}
