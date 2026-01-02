using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared.Audio.Jukebox;

/// <summary>
/// Soundtrack that's visible on the jukebox list.
/// </summary>
[Prototype]
public sealed partial class JukeboxPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = string.Empty;

    // WL-Changes-start
    /// <summary>
    /// User friendly name to use in UI.
    /// </summary>
    [DataField]
    public string? Author;

    [DataField(required: true)]
    public string Name { get; set; } = string.Empty;
    // WL-Changes-end

    [DataField(required: true)]
    public SoundPathSpecifier Path = default!;
}
