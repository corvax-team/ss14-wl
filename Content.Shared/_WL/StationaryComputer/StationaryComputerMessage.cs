using Content.Shared.Store;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.StationaryComputer;

[Serializable, NetSerializable]
public sealed class StationaryComputerMessage : BoundUserInterfaceMessage
{
    public string RawText { get; }
    public string CommandName { get; }
    public Dictionary<string, List<string>> Flags { get; }
    public List<string> Positional { get; }
    public string? Root { get; }

    public StationaryComputerMessage(
        string? root,
        string commandName,
        string rawText,
        List<string> positional,
        Dictionary<string, List<string>> options)
    {
        Root = root;
        CommandName = commandName;
        RawText = rawText;
        Positional = positional;
        Flags = options;
    }
}
