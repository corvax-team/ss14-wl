using Robust.Shared.Serialization;

namespace Content.Shared._WL.Barks;

[Serializable, NetSerializable]
public enum SpeechMode
{
    Tts,
    Barks,
    Disabled,
}
