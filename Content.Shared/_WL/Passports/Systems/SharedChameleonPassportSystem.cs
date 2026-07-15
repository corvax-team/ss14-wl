using Content.Shared._WL.Passports.Components;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Passports.Systems
{
    public abstract class SharedChameleonPassportSystem : EntitySystem
    {
        private const int MaxNameLength = 32;
        private const int MaxSpeciesLength = 15;
        private const int MaxGenderLength = 11;
        private const int MaxDateOfBirthLength = 24;
        private const int MaxHeightLength = 3;
        private const int MaxPIDLength = 17;

        public bool TryChangeNameTitle(EntityUid uid, string? nameTitle, PassportComponent? passport = null)
        {
            if (!Resolve(uid, ref passport))
                return false;

            nameTitle = nameTitle?.Trim();
            if (string.IsNullOrWhiteSpace(nameTitle))
                nameTitle = string.Empty;
            else if (nameTitle.Length > MaxNameLength)
                nameTitle = nameTitle[..MaxNameLength];

            if (passport.DisplayName == nameTitle)
                return true;
            passport.DisplayName = nameTitle;
            Dirty(uid, passport);

            return true;
        }

        public bool TryChangeSpeciesTitle(EntityUid uid, string? speciesTitle, PassportComponent? passport = null)
        {
            if (!Resolve(uid, ref passport))
                return false;

            speciesTitle = speciesTitle?.Trim();
            if (string.IsNullOrWhiteSpace(speciesTitle))
                speciesTitle = string.Empty;
            else if (speciesTitle.Length > MaxSpeciesLength)
                speciesTitle = speciesTitle[..MaxSpeciesLength];

            if (passport.DisplaySpecies == speciesTitle)
                return true;
            passport.DisplaySpecies = speciesTitle;
            Dirty(uid, passport);

            return true;
        }

        public bool TryChangeGenderTitle(EntityUid uid, string? genderTitle, PassportComponent? passport = null)
        {
            if (!Resolve(uid, ref passport))
                return false;

            genderTitle = genderTitle?.Trim();
            if (string.IsNullOrWhiteSpace(genderTitle))
                genderTitle = string.Empty;
            else if (genderTitle.Length > MaxGenderLength)
                genderTitle = genderTitle[..MaxGenderLength];

            if (passport.DisplayGender == genderTitle)
                return true;
            passport.DisplayGender = genderTitle;
            Dirty(uid, passport);

            return true;
        }

        public bool TryChangeDateOfBirthTitle(EntityUid uid, string? yobTitle, PassportComponent? passport = null)
        {
            if (!Resolve(uid, ref passport))
                return false;

            yobTitle = yobTitle?.Trim();
            if (string.IsNullOrWhiteSpace(yobTitle))
                yobTitle = string.Empty;
            else if (yobTitle.Length > MaxDateOfBirthLength)
                yobTitle = yobTitle[..MaxDateOfBirthLength];

            if (passport.DisplayDateOfBirth == yobTitle)
                return true;
            passport.DisplayDateOfBirth = yobTitle;
            Dirty(uid, passport);

            return true;
        }

        public bool TryChangeHeightTitle(EntityUid uid, string? heightTitle, PassportComponent? passport = null)
        {
            if (!Resolve(uid, ref passport))
                return false;

            heightTitle = heightTitle?.Trim();
            if (string.IsNullOrWhiteSpace(heightTitle))
                heightTitle = string.Empty;
            else if (heightTitle.Length > MaxHeightLength)
                heightTitle = heightTitle[..MaxHeightLength];

            if (passport.DisplayHeight == heightTitle) // предполагаем, что DisplayHeight тоже string
                return true;
            passport.DisplayHeight = heightTitle;
            Dirty(uid, passport);

            return true;
        }

        public bool TryChangePIDTitle(EntityUid uid, string? pidTitle, PassportComponent? passport = null)
        {
            if (!Resolve(uid, ref passport))
                return false;

            pidTitle = pidTitle?.Trim();
            if (string.IsNullOrWhiteSpace(pidTitle))
                pidTitle = string.Empty;
            else if (pidTitle.Length > MaxPIDLength)
                pidTitle = pidTitle[..MaxPIDLength];

            if (passport.DisplayPID == pidTitle)
                return true;
            passport.DisplayPID = pidTitle;
            Dirty(uid, passport);

            return true;
        }
    }

    /// <summary>
    /// Key representing which <see cref="PlayerBoundUserInterface"/> is currently open.
    /// Useful when there are multiple UI for an object. Here it's future-proofing only.
    /// </summary>
    [Serializable, NetSerializable]
    public enum ChameleonPassportUiKey : byte
    {
        Key,
    }

    /// <summary>
    /// Represents an <see cref="ChameleonPassportComponent"/> state that can be sent to the client
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class ChameleonPassportBoundUserInterfaceState(
        string currentName,
        string currentSpecies,
        string currentGender,
        string currentDateOfBirth,
        string currentHeight,
        string currentPid)
        : BoundUserInterfaceState
    {
        public string CurrentName { get; } = currentName;
        public string CurrentSpecies { get; } = currentSpecies;
        public string CurrentGender { get; } = currentGender;
        public string CurrentDateOfBirth { get; } = currentDateOfBirth;
        public string CurrentHeight { get; } = currentHeight;
        public string CurrentPID { get; } = currentPid;
    }

    [Serializable, NetSerializable]
    public sealed class ChameleonPassportNameChangedMessage(string name) : BoundUserInterfaceMessage
    {
        public string Name { get; } = name;
    }

    [Serializable, NetSerializable]
    public sealed class ChameleonPassportSpeciesChangedMessage(string species) : BoundUserInterfaceMessage
    {
        public string Species { get; } = species;
    }

    [Serializable, NetSerializable]
    public sealed class ChameleonPassportGenderChangedMessage(string gender) : BoundUserInterfaceMessage
    {
        public string Gender { get; } = gender;
    }

    [Serializable, NetSerializable]
    public sealed class ChameleonPassportDateOfBirthChangedMessage(string date) : BoundUserInterfaceMessage
    {
        public string DateOfBirth { get; } = date;
    }

    [Serializable, NetSerializable]
    public sealed class ChameleonPassportHeightChangedMessage(string height) : BoundUserInterfaceMessage
    {
        public string Height { get; } = height;
    }

    [Serializable, NetSerializable]
    public sealed class ChameleonPassportPIDChangedMessage(string pid) : BoundUserInterfaceMessage
    {
        public string PID { get; } = pid;
    }
}
