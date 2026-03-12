using Content.Shared.StationRecords;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.MedicalRecords;

[Serializable, NetSerializable]
public enum MedicalRecordsConsoleKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class MedicalRecordsConsoleState : BoundUserInterfaceState
{
    public uint? SelectedKey = null;
    public GeneralStationRecord? StationRecord = null;
    public readonly Dictionary<uint, string>? RecordListing;
    public readonly StationRecordsFilter? Filter;
    public readonly bool CanPrintRecords;

    public MedicalRecordsConsoleState(Dictionary<uint, string>? recordListing, StationRecordsFilter? newFilter, bool canPrintRecords)
    {
        RecordListing = recordListing;
        Filter = newFilter;
        CanPrintRecords = canPrintRecords;
    }

    public MedicalRecordsConsoleState() : this(null, null, false) { }

    public bool IsEmpty() => SelectedKey == null && StationRecord == null && RecordListing == null;
}

[Serializable, NetSerializable]
public sealed class MedicalRecordsSelectStationRecord : BoundUserInterfaceMessage
{
    public readonly uint? SelectedKey;

    public MedicalRecordsSelectStationRecord(uint? selectedKey)
    {
        SelectedKey = selectedKey;
    }
}

[Serializable, NetSerializable]
public sealed class MedicalRecordsSetStationRecordFilter : BoundUserInterfaceMessage
{
    public readonly StationRecordFilterType Type;
    public readonly string Value;

    public MedicalRecordsSetStationRecordFilter(StationRecordFilterType type, string value)
    {
        Type = type;
        Value = value;
    }
}
