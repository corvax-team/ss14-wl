namespace Content.Shared._WL.Paper;

/// <summary>
/// Converts a localized prototype template into the same ordered element model used by player-created forms.
/// Templates support <c>%%field:id%%</c>, <c>%%multiline:id%%</c>, and
/// <c>%%signature:id%%</c>. A field may include initial text after an equals sign.
/// </summary>
public static class StructuredPaperTemplateParser
{
    private const string MarkerStart = "%%";
    private const string MarkerEnd = "%%";
    private const string SingleLinePrefix = "field:";
    private const string MultilinePrefix = "multiline:";
    private const string SignaturePrefix = "signature:";

    public static bool TryParse(
        string template,
        out List<StructuredPaperElement> elements,
        out string? error)
    {
        var parsedElements = new List<StructuredPaperElement>();
        elements = parsedElements;
        error = null;

        var normalized = template.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = normalized.Split('\n');
        var pendingLines = new List<string>();
        var pendingEndsWithNewLine = false;
        var usedIds = new HashSet<string>();
        var staticIndex = 0;

        for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            var line = lines[lineIndex];
            var hasNewLineAfter = lineIndex < lines.Length - 1;
            if (!line.Contains(MarkerStart, StringComparison.Ordinal))
            {
                pendingLines.Add(line);
                pendingEndsWithNewLine = hasNewLineAfter;
                continue;
            }

            FlushPendingLines();
            if (!TryParseMarkerLine(line, hasNewLineAfter, usedIds, parsedElements, ref staticIndex, out error))
            {
                parsedElements.Clear();
                return false;
            }
        }

        FlushPendingLines();
        return true;

        void FlushPendingLines()
        {
            if (pendingLines.Count == 0)
                return;

            var text = string.Join('\n', pendingLines);
            AddStaticElement(text, pendingEndsWithNewLine, parsedElements, ref staticIndex);
            pendingLines.Clear();
            pendingEndsWithNewLine = false;
        }
    }

    private static bool TryParseMarkerLine(
        string line,
        bool hasNewLineAfter,
        HashSet<string> usedIds,
        List<StructuredPaperElement> elements,
        ref int staticIndex,
        out string? error)
    {
        error = null;
        var cursor = 0;
        var lineStartIndex = elements.Count;

        while (cursor < line.Length)
        {
            var markerStart = line.IndexOf(MarkerStart, cursor, StringComparison.Ordinal);
            if (markerStart < 0)
            {
                AddStaticElement(line[cursor..], false, elements, ref staticIndex);
                break;
            }

            AddStaticElement(line[cursor..markerStart], false, elements, ref staticIndex);

            var markerEnd = line.IndexOf(MarkerEnd, markerStart + MarkerStart.Length, StringComparison.Ordinal);
            if (markerEnd < 0)
            {
                error = $"Unclosed field marker in template line: {line}";
                return false;
            }

            var marker = line[(markerStart + MarkerStart.Length)..markerEnd];
            StructuredPaperElementType type;
            string payload;
            if (marker.StartsWith(SingleLinePrefix, StringComparison.Ordinal))
            {
                type = StructuredPaperElementType.SingleLineField;
                payload = marker[SingleLinePrefix.Length..];
            }
            else if (marker.StartsWith(MultilinePrefix, StringComparison.Ordinal))
            {
                type = StructuredPaperElementType.MultilineField;
                payload = marker[MultilinePrefix.Length..];
            }
            else if (marker.StartsWith(SignaturePrefix, StringComparison.Ordinal))
            {
                type = StructuredPaperElementType.Signature;
                payload = marker[SignaturePrefix.Length..];
            }
            else
            {
                error = $"Unknown structured paper marker '%%{marker}%%'.";
                return false;
            }

            var valueSeparator = payload.IndexOf('=');
            var id = valueSeparator < 0 ? payload : payload[..valueSeparator];
            var initialText = valueSeparator < 0 ? string.Empty : payload[(valueSeparator + 1)..];

            if (!IsValidFieldId(id) || !usedIds.Add(id))
            {
                error = $"Invalid or duplicate structured paper field id '{id}'.";
                return false;
            }

            if (type == StructuredPaperElementType.MultilineField &&
                (!string.IsNullOrWhiteSpace(line[..markerStart]) ||
                 !string.IsNullOrWhiteSpace(line[(markerEnd + MarkerEnd.Length)..])))
            {
                error = $"Multiline field '{id}' must be the only content on its template line.";
                return false;
            }

            if (initialText.Length > new StructuredPaperElement(string.Empty, type, string.Empty).GetMaxLength())
            {
                error = $"Initial value for structured paper field '{id}' is too long.";
                return false;
            }

            elements.Add(new StructuredPaperElement(id, type, initialText, newLineAfter: false));
            cursor = markerEnd + MarkerEnd.Length;
        }

        if (lineStartIndex == elements.Count)
            AddStaticElement(string.Empty, hasNewLineAfter, elements, ref staticIndex);
        else if (hasNewLineAfter)
            elements[^1].NewLineAfter = true;

        return true;
    }

    private static void AddStaticElement(
        string text,
        bool newLineAfter,
        List<StructuredPaperElement> elements,
        ref int staticIndex)
    {
        if (text.Length == 0)
        {
            if (!newLineAfter)
                return;

            // A standalone blank line needs actual label height in the client UI.
            text = "\n";
            newLineAfter = false;
        }

        staticIndex++;
        elements.Add(new StructuredPaperElement(
            $"static-{staticIndex}",
            StructuredPaperElementType.StaticText,
            text,
            newLineAfter: newLineAfter));
    }

    private static bool IsValidFieldId(string id)
    {
        if (string.IsNullOrWhiteSpace(id) ||
            id.Length > 64 ||
            id.StartsWith("static-", StringComparison.Ordinal))
            return false;

        foreach (var character in id)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not '-' and not '_')
                return false;
        }

        return true;
    }
}
