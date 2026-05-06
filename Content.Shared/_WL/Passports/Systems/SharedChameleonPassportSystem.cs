using Content.Shared.Administration.Logs;
using Content.Shared._WL.Passports.Components;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Passports.Systems
{
    public abstract class SharedChameleonPassportSystem : EntitySystem
    {
        private int _maxNameLength = 32;
        private int _maxSpeciesLength = 15;
        private int _maxGenderLength = 11;
        private int _maxYOBLength = 4;
        private int _maxHeightLength = 3;
        private int _maxPIDLength = 17;

        public bool TryChangeNameTitle(EntityUid uid, string? nameTitle, PassportComponent? passport = null)
        {
            if (!Resolve(uid, ref passport))
                return false;

            nameTitle = nameTitle?.Trim();
            if (string.IsNullOrWhiteSpace(nameTitle))
                nameTitle = string.Empty;
            else if (nameTitle.Length > _maxNameLength)
                nameTitle = nameTitle[.._maxNameLength];

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
            else if (speciesTitle.Length > _maxSpeciesLength)
                speciesTitle = speciesTitle[.._maxSpeciesLength];

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
            else if (genderTitle.Length > _maxGenderLength)
                genderTitle = genderTitle[.._maxGenderLength];

            if (passport.DisplayGender == genderTitle)
                return true;
            passport.DisplayGender = genderTitle;
            Dirty(uid, passport);

            return true;
        }

        public bool TryChangeYOBTitle(EntityUid uid, string? yobTitle, PassportComponent? passport = null)
        {
            if (!Resolve(uid, ref passport))
                return false;

            yobTitle = yobTitle?.Trim();
            if (string.IsNullOrWhiteSpace(yobTitle))
                yobTitle = string.Empty;
            else if (yobTitle.Length > _maxYOBLength)
                yobTitle = yobTitle[.._maxYOBLength];

            if (passport.DisplayYearOfBirth == yobTitle)
                return true;
            passport.DisplayYearOfBirth = yobTitle;
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
            else if (heightTitle.Length > _maxHeightLength)
                heightTitle = heightTitle[.._maxHeightLength];

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
            else if (pidTitle.Length > _maxPIDLength)
                pidTitle = pidTitle[.._maxPIDLength];

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
    public sealed class ChameleonPassportBoundUserInterfaceState : BoundUserInterfaceState
    {
        public string CurrentName { get; }
        public string CurrentSpecies { get; }
        public string CurrentGender { get; }
        public string CurrentYOB { get; }
        public string CurrentHeight { get; }
        public string CurrentPID { get; }

        public ChameleonPassportBoundUserInterfaceState(string currentName, string currentSpecies, string currentGender,
                                                        string currentYOB, string currentHeight, string currentPID)
        {
            CurrentName = currentName;
            CurrentSpecies = currentSpecies;
            CurrentGender = currentGender;
            CurrentYOB = currentYOB;
            CurrentHeight = currentHeight;
            CurrentPID = currentPID;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ChameleonPassportNameChangedMessage : BoundUserInterfaceMessage
    {
        public string Name { get; }

        public ChameleonPassportNameChangedMessage(string name)
        {
            Name = name;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ChameleonPassportSpeciesChangedMessage : BoundUserInterfaceMessage
    {
        public string Species { get; }

        public ChameleonPassportSpeciesChangedMessage(string species)
        {
            Species = species;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ChameleonPassportGenderChangedMessage : BoundUserInterfaceMessage
    {
        public string Gender { get; }

        public ChameleonPassportGenderChangedMessage(string gender)
        {
            Gender = gender;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ChameleonPassportYOBChangedMessage : BoundUserInterfaceMessage
    {
        public string YOB { get; }

        public ChameleonPassportYOBChangedMessage(string yob)
        {
            YOB = yob;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ChameleonPassportHeightChangedMessage : BoundUserInterfaceMessage
    {
        public string Height { get; }

        public ChameleonPassportHeightChangedMessage(string height)
        {
            Height = height;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ChameleonPassportPIDChangedMessage : BoundUserInterfaceMessage
    {
        public string PID { get; }

        public ChameleonPassportPIDChangedMessage(string pid)
        {
            PID = pid;
        }
    }
}
