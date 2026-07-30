using Robust.Shared.Serialization;

namespace Content.Shared._WL.Barks;

[Serializable, NetSerializable]
public sealed class VoiceMaskChangeBarkMessage(string bark) : BoundUserInterfaceMessage
{
    public string Bark = bark;
}

[Serializable, NetSerializable]
public sealed class VoiceMaskChangeBarkPitchMessage(float pitch) : BoundUserInterfaceMessage
{
    public float Pitch = pitch;
}
