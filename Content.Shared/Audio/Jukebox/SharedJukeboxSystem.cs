using Content.Shared.Nutrition;
using Robust.Shared.Audio.Systems;

namespace Content.Shared.Audio.Jukebox;

public abstract class SharedJukeboxSystem : EntitySystem
{
    [Dependency] protected readonly SharedAudioSystem Audio = default!;

    // WL-Changes-start
    public static string GetSongRepresentation(string? author, string name)
    {
        return $"{author ?? "Unknown Artist"} - {name}";
    }

    public static string GetSongRepresentation(JukeboxPrototype proto)
    {
        return GetSongRepresentation(proto.Author, proto.Name);
    }
    // WL-Changes-end
}
