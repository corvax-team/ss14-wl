using Content.Shared._WL.Research.Components;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client._WL.Research.UI;

public sealed class ResearchScannerBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private ResearchScannerMenu? _menu;

    public ResearchScannerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    { }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<ResearchScannerMenu>();

        _menu.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not ResearchScannerUserInterfaceState castState)
            return;

        if (_menu == null)
            return;

        _menu.UpdateOutput(castState);
    }
}
