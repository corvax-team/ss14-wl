using Content.Shared.StationRecords;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.MedicalRecords;

[Serializable, NetSerializable]
public enum MedicalRecordsConsoleKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class MedicalRecordsConsoleState(Dictionary<uint, string>? recordListing, StationRecordsFilter? newFilter, bool canPrintRecords) : BoundUserInterfaceState
{
    public uint? SelectedKey = null;
    public GeneralStationRecord? StationRecord = null;
    public readonly Dictionary<uint, string>? RecordListing = recordListing;
    public readonly StationRecordsFilter? Filter = newFilter;
    public readonly bool CanPrintRecords = canPrintRecords;

    public MedicalRecordsConsoleState() : this(null, null, false) { }

    public bool IsEmpty() => SelectedKey == null && StationRecord == null && RecordListing == null;
}

[Serializable, NetSerializable]
public sealed class MedicalRecordsSelectStationRecord(uint? selectedKey) : BoundUserInterfaceMessage
{
    public readonly uint? SelectedKey = selectedKey;
}

[Serializable, NetSerializable]
public sealed class MedicalRecordsSetStationRecordFilter(StationRecordFilterType type, string value) : BoundUserInterfaceMessage
{
    public readonly StationRecordFilterType Type = type;
    public readonly string Value = value;
}
