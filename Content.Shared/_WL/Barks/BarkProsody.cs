using Robust.Shared.Serialization;

namespace Content.Shared._WL.Barks;

/// <summary>
/// Describes the phrase-wide pitch and cadence changes applied to speech barks.
/// Values are derived only from the message, so previews and every listener use
/// the same emotional contour.
/// </summary>
[Serializable, NetSerializable]
public readonly record struct BarkProsody(
    float StartSemitones,
    float EndSemitones,
    float DelayScale,
    float VolumeOffset)
{
    public static readonly BarkProsody Neutral = new(0f, 0f, 1f, 0f);

    public float GetPitchMultiplier(float progress)
    {
        progress = Math.Clamp(progress, 0f, 1f);
        var semitones = StartSemitones + (EndSemitones - StartSemitones) * progress;
        return MathF.Pow(2f, semitones / 12f);
    }

    public static BarkProsody FromMessage(string message)
    {
        var ending = GetEmotionalEnding(message);
        return ending switch
        {
            EmotionalEnding.TwoDots => new BarkProsody(-1f, -3f, 1.12f, -0.5f),
            EmotionalEnding.Ellipsis => new BarkProsody(-2f, -5f, 1.25f, -1f),
            EmotionalEnding.OneExclamation => new BarkProsody(0.5f, 2.5f, 0.92f, 1.5f),
            EmotionalEnding.TwoExclamations => new BarkProsody(1f, 4f, 0.84f, 2.5f),
            EmotionalEnding.ThreeExclamations => new BarkProsody(1.5f, 5.5f, 0.76f, 3.5f),
            _ => Neutral,
        };
    }

    public static int GetBarkCount(string message)
    {
        // Match ADT: one initial grain, then one more for every three
        // characters. Exact multiples intentionally receive the trailing
        // grain as well (3 -> 2, 6 -> 3).
        return Math.Max(1, message.Length / 3 + 1);
    }

    private static EmotionalEnding GetEmotionalEnding(string message)
    {
        var index = message.Length - 1;
        while (index >= 0 && IsIgnoredEndingCharacter(message[index]))
            index--;

        if (index < 0)
            return EmotionalEnding.Neutral;

        var endingCharacter = message[index];
        if (endingCharacter is not ('.' or '!'))
            return EmotionalEnding.Neutral;

        var count = 0;
        while (index >= 0 && message[index] == endingCharacter)
        {
            count++;
            index--;
        }

        if (endingCharacter == '.')
        {
            return count switch
            {
                2 => EmotionalEnding.TwoDots,
                >= 3 => EmotionalEnding.Ellipsis,
                _ => EmotionalEnding.Neutral,
            };
        }

        return count switch
        {
            1 => EmotionalEnding.OneExclamation,
            2 => EmotionalEnding.TwoExclamations,
            >= 3 => EmotionalEnding.ThreeExclamations,
            _ => EmotionalEnding.Neutral,
        };
    }

    private static bool IsIgnoredEndingCharacter(char character)
    {
        return char.IsWhiteSpace(character) || character is '"' or '\'' or '»' or ')' or ']' or '}';
    }

    private enum EmotionalEnding : byte
    {
        Neutral,
        TwoDots,
        Ellipsis,
        OneExclamation,
        TwoExclamations,
        ThreeExclamations,
    }
}
