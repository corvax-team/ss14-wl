using Content.Client.UserInterface.Controls;
using Content.Shared._WL.Languages;
using Robust.Client.UserInterface;

namespace Content.Client._WL.Languages;

public sealed class BorgLanguageModuleBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private BorgLanguageModuleWindow? _window;

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<BorgLanguageModuleWindow>();
        _window.Confirmed += languages => SendMessage(new BorgLanguageModuleConfirmMessage(languages));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is BorgLanguageModuleState card)
            _window?.UpdateState(card);
    }
}
