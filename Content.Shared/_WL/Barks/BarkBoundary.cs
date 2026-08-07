using Robust.Shared.Serialization;

namespace Content.Shared._WL.Barks;

[Serializable, NetSerializable]
public enum BarkBoundary : byte
{
    None,
    Comma,
    Clause,
    Period,
    Question,
    Exclamation,
    Ellipsis,
}

public static class BarkBoundaryExtensions
{
    public static float GetAdditionalDelay(this BarkBoundary boundary)
    {
        return boundary switch
        {
            BarkBoundary.Comma => 0.12f,
            BarkBoundary.Clause => 0.2f,
            BarkBoundary.Period => 0.28f,
            BarkBoundary.Question => 0.28f,
            BarkBoundary.Exclamation => 0.24f,
            BarkBoundary.Ellipsis => 0.42f,
            _ => 0f,
        };
    }

    public static float GetPitchMultiplier(this BarkBoundary boundary)
    {
        var semitones = boundary switch
        {
            BarkBoundary.Comma => 0.5f,
            BarkBoundary.Clause => -0.5f,
            BarkBoundary.Period => -1f,
            BarkBoundary.Question => 1.5f,
            BarkBoundary.Exclamation => 1f,
            BarkBoundary.Ellipsis => -1.5f,
            _ => 0f,
        };

        return MathF.Pow(2f, semitones / 12f);
    }
}
