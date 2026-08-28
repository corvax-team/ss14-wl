using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.StationaryComputer;

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
[Access(typeof(SharedStationaryComputerSystem))]
public sealed partial class StationaryComputerComponent : Component
{
    [DataField, AutoNetworkedField]
    public string CurrentRoot { get; set; } = "~/";

    [DataField, AutoNetworkedField]
    public List<string> BaseContent { get; set; } = new();

    [DataField, AutoNetworkedField]
    public List<StationaryComputerContentEntry> Content { get; set; } = new();

    [DataField, AutoNetworkedField]
    public Color ConsoleColor { get; set; } = Color.White;

    [DataField, AutoNetworkedField]
    public bool Locked { get; set; } = false;

    [DataField]
    public string? Password { get; set; }

    public void AddContent(string? content, string? root = null)
    {
        if (content == null)
            return;

        Content.Add(new(content, root));
    }
}

[Serializable, NetSerializable]
public readonly record struct StationaryComputerContentEntry(string Content, string? Root);

[ByRefEvent]
public record struct CheckCommandEnterEvent
{
    public readonly string Name;
    public readonly List<string> Positional;
    public readonly Dictionary<string, List<string>> Options;

    public string? Response { get; private set; }
    public bool Handled { get; private set; }

    public CheckCommandEnterEvent(string name, List<string> positional, Dictionary<string, List<string>> options)
    {
        Name = name;
        Positional = positional;
        Options = options;
    }

    public void Handle(string? response)
    {
        Handled = true;
        Response = response;
    }
}
