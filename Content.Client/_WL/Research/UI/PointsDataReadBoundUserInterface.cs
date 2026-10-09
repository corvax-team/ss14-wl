using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Components;
using Robust.Client.UserInterface;

namespace Content.Client._WL.Research.UI
{
    public sealed class PointsDataReadBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private PointsDataReadMenu? _menu;

        public PointsDataReadBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _menu = this.CreateWindow<PointsDataReadMenu>();
            _menu.SetEntity(Owner);

            _menu.TransferButtonPressed += OnTransferPressed;

            _menu.OpenCentered();
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            if (state is not PointsDataReadBoundUserInterfaceState castState)
                return;

            _menu?.UpdatePoints(castState);
        }

        private void OnTransferPressed(ResearchPointsSpecifier points, bool direction)
        {
            SendMessage(new PointsTransferMessage(points, direction));
        }
    }
}
