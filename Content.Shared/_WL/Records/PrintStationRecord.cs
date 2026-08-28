using Robust.Shared.Serialization;

namespace Content.Shared._WL.Records;

[Serializable, NetSerializable]
public sealed class PrintStationRecord(uint id) : BoundUserInterfaceMessage
{
    public readonly uint Id = id;
}
