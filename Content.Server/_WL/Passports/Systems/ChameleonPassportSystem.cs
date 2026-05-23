using Content.Shared.UserInterface;
using Content.Shared._WL.Passports.Components;
using Content.Shared._WL.Passports.Systems;
using Robust.Server.GameObjects;

namespace Content.Server._WL.Passports.Systems
{
    public sealed partial class ChameleonPassportSystem : SharedChameleonPassportSystem
    {
        [Dependency] private SharedChameleonPassportSystem _passportSystem = default!;
        [Dependency] private UserInterfaceSystem _uiSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            // BUI
            SubscribeLocalEvent<ChameleonPassportComponent, AfterActivatableUIOpenEvent>(AfterUIOpen);
            SubscribeLocalEvent<ChameleonPassportComponent, ChameleonPassportNameChangedMessage>(OnNameChanged);
            SubscribeLocalEvent<ChameleonPassportComponent, ChameleonPassportSpeciesChangedMessage>(OnSpeciesChanged);
            SubscribeLocalEvent<ChameleonPassportComponent, ChameleonPassportGenderChangedMessage>(OnGenderChanged);
            SubscribeLocalEvent<ChameleonPassportComponent, ChameleonPassportYOBChangedMessage>(OnYOBChanged);
            SubscribeLocalEvent<ChameleonPassportComponent, ChameleonPassportHeightChangedMessage>(OnHeightChanged);
            SubscribeLocalEvent<ChameleonPassportComponent, ChameleonPassportPIDChangedMessage>(OnPIDChanged);
        }

        private void AfterUIOpen(EntityUid uid, ChameleonPassportComponent component, AfterActivatableUIOpenEvent args)
        {
            if (!_uiSystem.HasUi(uid, ChameleonPassportUiKey.Key))
                return;

            if (!TryComp<PassportComponent>(uid, out var passport))
                return;

            var state = new ChameleonPassportBoundUserInterfaceState(passport.DisplayName ?? "", passport.DisplaySpecies ?? "", passport.DisplayGender ?? "",
                                                                     passport.DisplayHeight ?? "", passport.DisplayYearOfBirth ?? "", passport.DisplayPID ?? "");
            _uiSystem.SetUiState(uid, ChameleonPassportUiKey.Key, state);
        }

        private void OnNameChanged(EntityUid uid, ChameleonPassportComponent comp, ChameleonPassportNameChangedMessage args)
        {
            if (!TryComp<PassportComponent>(uid, out var passport))
                return;

            _passportSystem.TryChangeNameTitle(uid, args.Name, passport);
        }

        private void OnSpeciesChanged(EntityUid uid, ChameleonPassportComponent comp, ChameleonPassportSpeciesChangedMessage args)
        {
            if (!TryComp<PassportComponent>(uid, out var passport))
                return;

            _passportSystem.TryChangeSpeciesTitle(uid, args.Species, passport);
        }

        private void OnGenderChanged(EntityUid uid, ChameleonPassportComponent comp, ChameleonPassportGenderChangedMessage args)
        {
            if (!TryComp<PassportComponent>(uid, out var passport))
                return;

            _passportSystem.TryChangeGenderTitle(uid, args.Gender, passport);
        }

        private void OnYOBChanged(EntityUid uid, ChameleonPassportComponent comp, ChameleonPassportYOBChangedMessage args)
        {
            if (!TryComp<PassportComponent>(uid, out var passport))
                return;

            _passportSystem.TryChangeYOBTitle(uid, args.YOB, passport);
        }

        private void OnHeightChanged(EntityUid uid, ChameleonPassportComponent comp, ChameleonPassportHeightChangedMessage args)
        {
            if (!TryComp<PassportComponent>(uid, out var passport))
                return;

            _passportSystem.TryChangeHeightTitle(uid, args.Height, passport);
        }

        private void OnPIDChanged(EntityUid uid, ChameleonPassportComponent comp, ChameleonPassportPIDChangedMessage args)
        {
            if (!TryComp<PassportComponent>(uid, out var passport))
                return;

            _passportSystem.TryChangePIDTitle(uid, args.PID, passport);
        }
    }
}
