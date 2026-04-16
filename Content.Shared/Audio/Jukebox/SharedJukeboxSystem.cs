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

    /// <summary>
    /// Returns whether or not the given jukebox is currently playing a song.
    /// </summary>
    public bool IsPlaying(Entity<JukeboxComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp))
            return false;

        return entity.Comp.AudioStream is { } audio && Audio.IsPlaying(audio);
    }
}
