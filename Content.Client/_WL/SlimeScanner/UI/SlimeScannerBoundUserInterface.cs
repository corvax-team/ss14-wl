using Content.Shared._WL.Xenobiology;
using JetBrains.Annotations;

namespace Content.Client._WL.SlimeScanner.UI
{
    [UsedImplicitly]
    public sealed class SlimeScannerBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private SlimeScannerWindow? _window;

        public SlimeScannerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();
            _window = new SlimeScannerWindow
            {
                Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName,
            };
            _window.OnClose += Close;
            _window.OpenCentered();
        }

        protected override void ReceiveMessage(BoundUserInterfaceMessage message)
        {
            if (_window == null)
                return;

            if (message is not SlimeScannerScannedUserMessage cast)
                return;

            _window.Populate(cast);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;

            if (_window != null)
                _window.OnClose -= Close;

            _window?.Dispose();
        }
    }
}
