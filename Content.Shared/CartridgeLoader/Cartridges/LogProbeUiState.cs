//WL-Changes-NanoChat-Start
using Content.Shared._WL.CartridgeLoader.Cartridges;
//WL-Changes-NanoChat-End
using Robust.Shared.Serialization;

namespace Content.Shared.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class LogProbeUiState : BoundUserInterfaceState
{
    /// <summary>
    /// The name of the scanned entity.
    /// </summary>
    public string EntityName;

    /// <summary>
    /// The list of probed network devices
    /// </summary>
    public List<PulledAccessLog> PulledLogs;

    //WL-Changes-NanoChat-Start
    /// <summary>
    /// NanoChat data pulled from the scanned ID card, if any.
    /// </summary>
    public NanoChatData? NanoChat;

    public LogProbeUiState(string entityName, List<PulledAccessLog> pulledLogs, NanoChatData? nanoChat = null)
    {
        EntityName = entityName;
        PulledLogs = pulledLogs;
        NanoChat = nanoChat;
    }
    //WL-Changes-NanoChat-End
}

[Serializable, NetSerializable, DataRecord]
public sealed partial class PulledAccessLog
{
    public readonly TimeSpan Time;
    public readonly string Accessor;

    public PulledAccessLog(TimeSpan time, string accessor)
    {
        Time = time;
        Accessor = accessor;
    }
}
