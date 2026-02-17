using Robust.Shared.Serialization;

namespace Content.Shared._WL.Records;

[Serializable, NetSerializable]
public sealed class PrintStationRecord : BoundUserInterfaceMessage
{
    public PrintStationRecord(string content)
    {
        Content = content;
    }

    public readonly string Content;
}
