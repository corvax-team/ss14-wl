using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Components;
using Content.Client._WL.Research.UI;
using Robust.Client.UserInterface;

namespace Content.Client._WL.Research.UI;

public sealed class RecipesReaderBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private RecipesReaderMenu? _menu;

    public RecipesReaderBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    { }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<RecipesReaderMenu>();
        _menu.SetEntity(Owner);

        // _menu.TransferButtonPressed += OnTransferPressed;

        _menu.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

         if (state is not RecipesReaderBoundUserInterfaceState castState)
            return;

        _menu?.UpdateRecipes(castState);
    }

    /*
    private void OnTransferPressed(ResearchPointsSpecifier points, bool direction)
    {
        SendMessage(new PointsTransferMessage(points, direction));
    }
    */
}
