using System.Linq;
using Content.Shared._WL.Research.Components;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._WL.Research.UI;

public sealed partial class UniversalComputerBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private IPrototypeManager _prototype = default!;

    private UniversalComputerMenu? _menu;

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<UniversalComputerMenu>();

        _menu.OnProgramOpenPressed += id =>
        {
            SendMessage(new ProgramOpenMessage(id));
        };

        _menu.OpenToLeft();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not UniversalComputerBoundInterfaceState castState)
            return;

        _menu?.UpdatePrograms(castState);
    }
}
