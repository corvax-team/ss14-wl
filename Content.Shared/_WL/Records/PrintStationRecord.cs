using Robust.Shared.Serialization;

namespace Content.Shared._WL.Records;

[Serializable, NetSerializable]
public sealed class PrintStationRecord : BoundUserInterfaceMessage
{
    public PrintStationRecord(uint id)
    {
        Id = id;
    }

    public readonly uint Id;
}
