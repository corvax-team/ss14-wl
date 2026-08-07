namespace Content.Shared._WL.Barks;

/// <summary>
/// Randomizes an entity's bark voice settings once when it enters the map.
/// </summary>
[RegisterComponent]
public sealed partial class RandomizedSpeechBarksComponent : Component
{
    [DataField]
    public float MinPitch = 0.9f;

    [DataField]
    public float MaxPitch = 1.1f;

    [DataField]
    public float MinDelayLowerBound = 0.08f;

    [DataField]
    public float MinDelayUpperBound = 0.14f;

    [DataField]
    public float MaxDelayLowerBound = 0.25f;

    [DataField]
    public float MaxDelayUpperBound = 0.45f;
}
