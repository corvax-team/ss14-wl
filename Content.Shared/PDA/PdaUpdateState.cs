using Content.Shared.CartridgeLoader;
using Robust.Shared.Serialization;

namespace Content.Shared.PDA
{
    [Serializable, NetSerializable]
    public sealed class PdaUpdateState : CartridgeLoaderUiState // WTF is this. what. I ... fuck me I just want net entities to work
        // TODO purge this shit
        //AAAAAAAAAAAAAAAA
    {
        public bool FlashlightEnabled;
        public bool HasPen;
        public bool HasPai;
        public PdaIdInfoText PdaOwnerInfo;
        public string? StationName;
        public bool HasUplink;
        public bool CanPlayMusic;
        public string? Address;
        // WL-Changes-start: ETA in PDA
        public TimeSpan? ExpectedETA;
        public TimeSpan? BeforeETA;
        public bool roundEnd;
        // WL-Changes-end

        public PdaUpdateState(
            List<NetEntity> programs,
            NetEntity? activeUI,
            bool flashlightEnabled,
            bool hasPen,
            bool hasPai,
            PdaIdInfoText pdaOwnerInfo,
            string? stationName,
            bool hasUplink = false,
            bool canPlayMusic = false,
            string? address = null,
            // WL-Changes-start: ETA in PDA
            TimeSpan? eta = null,
            TimeSpan? bETA = null,
            bool roundEND = false)
            // WL-Changes-end
            : base(programs, activeUI)
        {
            FlashlightEnabled = flashlightEnabled;
            HasPen = hasPen;
            HasPai = hasPai;
            PdaOwnerInfo = pdaOwnerInfo;
            HasUplink = hasUplink;
            CanPlayMusic = canPlayMusic;
            StationName = stationName;
            Address = address;
            // WL-Changes-start: ETA in PDA
            ExpectedETA = eta;
            BeforeETA = bETA;
            roundEnd = roundEND;
            // WL-Changes-end
        }
    }

    [Serializable, NetSerializable]
    public struct PdaIdInfoText
    {
        public string? ActualOwnerName;
        public string? IdOwner;
        public string? JobTitle;
        public string? StationAlertLevel;
        public Color StationAlertColor;
        public string? StationAlertInstructions; // WL-Changes: custom alert instructions in PDA
        public string? StationAlertName; // WL-Changes: Alert Level Rework
    }
}
