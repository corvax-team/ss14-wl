using Content.Shared.StationRecords;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Compromise;

[Serializable, NetSerializable]
public enum CompromiseTabletKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class CompromiseTabletState : BoundUserInterfaceState
{
    public uint? SelectedKey = null;
    public CompromiseRecord? Record = null;
    public GeneralStationRecord? StationRecord = null;
    public readonly Dictionary<uint, string>? RecordListing;
    public readonly StationRecordsFilter? Filter;
    public readonly bool CanPrint;

    public CompromiseTabletState(Dictionary<uint, string>? recordListing, StationRecordsFilter? newFilter, bool canPrint)
    {
        RecordListing = recordListing;
        Filter = newFilter;
        CanPrint = canPrint;
    }

    public CompromiseTabletState() : this(null, null, false) { }

    public bool IsEmpty() => SelectedKey == null && StationRecord == null && Record == null && RecordListing == null;
}

[Serializable, NetSerializable]
public sealed class PrintCompromiseRecord(uint id) : BoundUserInterfaceMessage
{
    public readonly uint Id = id;
}
