using Content.Shared._WL.Passports.Systems;
using Content.Shared.StatusIcon;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._WL.Passports.UI
{
    /// <summary>
    /// Initializes a <see cref="ChameleonPassportWindow"/> and updates it when new server messages are received.
    /// </summary>
    public sealed class ChameleonPassportBoundUserInterface : BoundUserInterface
    {
        private ChameleonPassportWindow? _window;

        public ChameleonPassportBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<ChameleonPassportWindow>();

            _window.OnNameChanged += OnNameChanged;
            _window.OnSpeciesChanged += OnSpeciesChanged;
            _window.OnGenderChanged += OnGenderChanged;
            _window.OnYOBChanged += OnYOBChanged;
            _window.OnHeightChanged += OnHeightChanged;
            _window.OnPIDChanged += OnPIDChanged;
        }

        private void OnNameChanged(string newName)
        {
            SendMessage(new ChameleonPassportNameChangedMessage(newName));
        }

        private void OnSpeciesChanged(string newSpecies)
        {
            SendMessage(new ChameleonPassportSpeciesChangedMessage(newSpecies));
        }

        private void OnGenderChanged(string newGender)
        {
            SendMessage(new ChameleonPassportGenderChangedMessage(newGender));
        }

        private void OnYOBChanged(string newYOB)
        {
            SendMessage(new ChameleonPassportYOBChangedMessage(newYOB));
        }


        private void OnHeightChanged(string newHeight)
        {
            SendMessage(new ChameleonPassportHeightChangedMessage(newHeight));
        }

        private void OnPIDChanged(string newPID)
        {
            SendMessage(new ChameleonPassportPIDChangedMessage(newPID));
        }


        /// <summary>
        /// Update the UI state based on server-sent info
        /// </summary>
        /// <param name="state"></param>
        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);
            if (_window == null || state is not ChameleonPassportBoundUserInterfaceState cast)
                return;

            _window.SetCurrentName(cast.CurrentName);
            _window.SetCurrentSpecies(cast.CurrentSpecies);
            _window.SetCurrentGender(cast.CurrentGender);
            _window.SetCurrentYOB(cast.CurrentYOB);
            _window.SetCurrentHeight(cast.CurrentHeight);
            _window.SetCurrentPID(cast.CurrentPID);
        }
    }
}
