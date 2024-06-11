using Content.Client.Ame.UI;
using Content.Shared._WL.Turrets;
using JetBrains.Annotations;

namespace Content.Client._WL.Turrets
{
    [UsedImplicitly]
    public sealed class ConsoleTurretMinderBoundUserInterface : BoundUserInterface
    {
        private TurretMinderConsoleWindow? _window;

        public ConsoleTurretMinderBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _window = new TurretMinderConsoleWindow();
            _window.RideButtonPressed += SendState;
            _window.OnClose += Close;
            _window.OpenCentered();
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            if (state is not TurretMinderConsoleBoundUserInterfaceState consoleState)
                return;

            _window?.UpdateState(consoleState);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _window?.Dispose();
            }
        }

        private void SendState(NetEntity ent)
        {
            SendMessage(new TurretMinderConsolePressedUiButtonMessage(ent));
        }
    }
}
