using Robust.Shared.Serialization;

namespace Content.Shared._WL.Audio.Jukebox;


[Serializable, NetSerializable]
public sealed class JukeboxVolumeChangedMessage(float newVolume) : BoundUserInterfaceMessage
{
    public float Volume { get; } = newVolume;
}
