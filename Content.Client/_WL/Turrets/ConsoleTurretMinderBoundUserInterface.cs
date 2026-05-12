using Content.Shared._WL.Turrets;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

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

            _window = this.CreateWindow<TurretMinderConsoleWindow>();
            _window.RideButtonPressed += SendState;
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            if (state is not TurretMinderConsoleBoundUserInterfaceState consoleState)
                return;

            _window?.UpdateState(consoleState);
        }

        private void SendState(NetEntity ent)
        {
            SendMessage(new TurretMinderConsolePressedUiButtonMessage(ent));
        }
    }
}
