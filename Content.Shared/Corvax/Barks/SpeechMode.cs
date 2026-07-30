using Robust.Shared.Serialization;

namespace Content.Shared.Corvax.Barks;

[Serializable, NetSerializable]
public enum SpeechMode
{
    Tts,
    Barks,
    Disabled,
}
