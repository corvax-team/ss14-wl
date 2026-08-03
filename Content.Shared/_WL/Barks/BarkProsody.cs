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
    private const int MaxBarkCount = 24;
    private const string CyrillicVowels = "аеёиоуыэюяіїє";

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
        return GetBarkRhythm(message).Length;
    }

    public static BarkBoundary[] GetBarkRhythm(string message)
    {
        var rhythm = new List<BarkBoundary>(Math.Min(message.Length, MaxBarkCount));
        var wordStart = -1;

        for (var i = 0; i <= message.Length; i++)
        {
            if (i < message.Length && char.IsLetterOrDigit(message[i]))
            {
                if (wordStart == -1)
                    wordStart = i;

                continue;
            }

            if (wordStart != -1)
            {
                var syllables = EstimateWordSyllables(message.AsSpan(wordStart, i - wordStart));
                for (var syllable = 0; syllable < syllables && rhythm.Count < MaxBarkCount; syllable++)
                    rhythm.Add(BarkBoundary.None);

                wordStart = -1;
            }

            if (i < message.Length && rhythm.Count > 0)
            {
                var boundary = GetBoundary(message, i);
                if (GetBoundaryPriority(boundary) > GetBoundaryPriority(rhythm[^1]))
                    rhythm[^1] = boundary;
            }
        }

        return rhythm.ToArray();
    }

    private static BarkBoundary GetBoundary(string message, int index)
    {
        return message[index] switch
        {
            ',' => BarkBoundary.Comma,
            ';' or ':' => BarkBoundary.Clause,
            '—' or '–' => BarkBoundary.Clause,
            '-' when (index > 0 && char.IsWhiteSpace(message[index - 1])) ||
                (index + 1 < message.Length && char.IsWhiteSpace(message[index + 1])) => BarkBoundary.Clause,
            '?' => BarkBoundary.Question,
            '!' => BarkBoundary.Exclamation,
            '.' when (index > 0 && message[index - 1] == '.') ||
                (index + 1 < message.Length && message[index + 1] == '.') => BarkBoundary.Ellipsis,
            '.' => BarkBoundary.Period,
            '\n' or '\r' => BarkBoundary.Period,
            _ => BarkBoundary.None,
        };
    }

    private static int GetBoundaryPriority(BarkBoundary boundary)
    {
        return boundary switch
        {
            BarkBoundary.None => 0,
            BarkBoundary.Comma => 1,
            BarkBoundary.Clause => 2,
            BarkBoundary.Period => 3,
            BarkBoundary.Question => 4,
            BarkBoundary.Exclamation => 5,
            BarkBoundary.Ellipsis => 6,
            _ => 0,
        };
    }

    private static int EstimateWordSyllables(ReadOnlySpan<char> word)
    {
        var syllables = 0;
        var hasCyrillicVowel = false;
        var insideLatinVowelGroup = false;

        for (var i = 0; i < word.Length; i++)
        {
            var character = char.ToLowerInvariant(word[i]);
            if (CyrillicVowels.Contains(character))
            {
                syllables++;
                hasCyrillicVowel = true;
                insideLatinVowelGroup = false;
                continue;
            }

            if (IsLatinVowel(character, i))
            {
                if (!insideLatinVowelGroup)
                    syllables++;

                insideLatinVowelGroup = true;
                continue;
            }

            insideLatinVowelGroup = false;
        }

        if (!hasCyrillicVowel && HasSilentLatinE(word) && syllables > 1)
            syllables--;

        // Numbers, abbreviations and words from unsupported alphabets still
        // need one grain so that they do not disappear from the rhythm.
        return Math.Max(1, syllables);
    }

    private static bool IsLatinVowel(char character, int index)
    {
        return character is 'a' or 'e' or 'i' or 'o' or 'u' ||
            character == 'y' && index > 0;
    }

    private static bool HasSilentLatinE(ReadOnlySpan<char> word)
    {
        if (word.Length < 2 || char.ToLowerInvariant(word[^1]) != 'e')
            return false;

        // A final consonant + "le", as in "table", normally forms a syllable.
        return word.Length < 3 ||
            char.ToLowerInvariant(word[^2]) != 'l' ||
            IsLatinVowel(char.ToLowerInvariant(word[^3]), word.Length - 3);
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
