using Content.Shared.Inventory;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Barks;

[Serializable, NetSerializable]
public sealed class PlaySpeechBarksEvent(
    NetEntity source,
    SoundSpecifier sound,
    float pitch,
    float minDelay,
    float maxDelay,
    BarkBoundary[] rhythm,
    BarkProsody prosody,
    bool isWhisper) : EntityEventArgs
{
    public NetEntity Source = source;
    public SoundSpecifier Sound = sound;
    public float Pitch = pitch;
    public float MinDelay = minDelay;
    public float MaxDelay = maxDelay;
    public BarkBoundary[] Rhythm = rhythm;
    public BarkProsody Prosody = prosody;
    public bool IsWhisper = isWhisper;
}

/// <summary>
/// Allows equipment, implants and speech proxy systems to replace bark settings
/// before a spoken message is sent to clients.
/// </summary>
public sealed class TransformSpeakerBarkEvent(
    EntityUid sender,
    ProtoId<BarkPrototype> voice,
    float pitch,
    float minDelay,
    float maxDelay) : EntityEventArgs, IInventoryRelayEvent
{
    public EntityUid Sender = sender;
    public ProtoId<BarkPrototype> Voice = voice;
    public float Pitch = pitch;
    public float MinDelay = minDelay;
    public float MaxDelay = maxDelay;

    SlotFlags IInventoryRelayEvent.TargetSlots => SlotFlags.WITHOUT_POCKET;
}
