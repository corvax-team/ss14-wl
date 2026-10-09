using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Components;
using Robust.Client.UserInterface;

namespace Content.Client._WL.Research.UI;

public sealed class DAnalyzerBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private DAnalyzerMenu? _menu;

    public DAnalyzerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    { }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<DAnalyzerMenu>();
        // _menu.SetEntity(Owner);

        _menu.ButtonPressed += OnPressed;

        _menu.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not DAnalyzerBoundUserInterfaceState castState)
            return;

        _menu?.UpdateState(castState);
    }

    private void OnPressed(DAnalyzeType type)
    {
        SendMessage(new DAnalyzeMessage(type));
    }
}
