using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Barks;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpeechBarksComponent : Component
{
    public const float DefaultPitch = 1f;
    public const float MinPitch = 0.6f;
    public const float MaxPitch = 1.5f;
    public const float DefaultMinDelay = 0.1f;
    public const float DefaultMaxDelay = 0.5f;
    public const float MinDelayLimit = 0.08f;
    public const float MaxDelayLimit = 0.6f;
    public const float ExperimentalMinDelay = 0.12f;
    public const float ExperimentalMaxDelay = 0.18f;

    [DataField]
    public ProtoId<BarkPrototype> Voice = "Human1";

    [DataField]
    public float Pitch = DefaultPitch;

    [DataField]
    public float MinDelay = DefaultMinDelay;

    [DataField]
    public float MaxDelay = DefaultMaxDelay;

    /// <summary>
    /// Whether barks should be played at the speaker when the message is sent over radio.
    /// </summary>
    [DataField]
    public bool PlayOnRadio = true;

    public static float SanitizePitch(float pitch)
    {
        if (!float.IsFinite(pitch))
            return DefaultPitch;

        return Math.Clamp(pitch, MinPitch, MaxPitch);
    }

    public static (float Min, float Max) SanitizeDelays(float min, float max)
    {
        if (!float.IsFinite(min))
            min = DefaultMinDelay;

        if (!float.IsFinite(max))
            max = DefaultMaxDelay;

        // Migrate profiles saved with the experimental narrow cadence back to
        // the ADT defaults. That narrow range made short voices sound like a
        // mechanical burst instead of distinct game-dialogue grains.
        if (MathHelper.CloseTo(min, ExperimentalMinDelay) &&
            MathHelper.CloseTo(max, ExperimentalMaxDelay))
        {
            return (DefaultMinDelay, DefaultMaxDelay);
        }

        min = Math.Clamp(min, MinDelayLimit, MaxDelayLimit);
        max = Math.Clamp(max, min, MaxDelayLimit);
        return (min, max);
    }
}
