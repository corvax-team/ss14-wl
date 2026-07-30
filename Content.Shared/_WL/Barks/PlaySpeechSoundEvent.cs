using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Barks;

/// <summary>
/// Sends the speaker's normal species voice sound to clients so each listener can
/// adjust it for their selected speech mode.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlaySpeechSoundEvent(NetEntity source, SoundSpecifier sound) : EntityEventArgs
{
    public NetEntity Source = source;
    public SoundSpecifier Sound = sound;
}
