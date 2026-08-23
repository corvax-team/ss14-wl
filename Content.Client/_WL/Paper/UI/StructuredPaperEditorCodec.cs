using System.Text;
using System.Linq;
using Content.Shared._WL.Paper;

namespace Content.Client._WL.Paper.UI;

/// <summary>
/// Converts structured paper elements to the compact source shown by the full paper editor.
/// Aliases are explicit editor-session handles, so moving a tag never rebinds a field by position.
/// </summary>
public sealed class StructuredPaperEditorCodec
{
    private readonly Dictionary<string, StructuredPaperElement> _aliases = new();

    private StructuredPaperEditorCodec()
    {
    }

    public static StructuredPaperEditorCodec Create(
        IReadOnlyList<StructuredPaperElement> elements,
        bool appendOnly,
        out string source)
    {
        var codec = new StructuredPaperEditorCodec();
        if (appendOnly)
        {
            source = string.Empty;
            return codec;
        }

        var builder = new StringBuilder();
        foreach (var element in elements)
        {
            switch (element.Type)
            {
                case StructuredPaperElementType.StaticText:
                    builder.Append(element.Text);
                    break;
                case StructuredPaperElementType.HandwrittenText:
                    codec.AppendAliasedTag(builder, "w", element);
                    break;
                case StructuredPaperElementType.SingleLineField:
                case StructuredPaperElementType.SignatureField:
                    codec.AppendAliasedTag(builder, "f", element);
                    break;
                case StructuredPaperElementType.MultilineField:
                    codec.AppendAliasedTag(builder, "lf", element);
                    break;
                case StructuredPaperElementType.Signature:
                    codec.AppendAliasedTag(builder, "sign", element);
                    break;
            }

            if (element.NewLineAfter)
                builder.Append('\n');
        }

        source = builder.ToString();
        return codec;
    }

    public bool TryParse(
        string source,
        bool appendOnly,
        PaperHandwritingStyle handwritingStyle,
        out List<StructuredPaperElement> elements)
    {
        elements = new List<StructuredPaperElement>();
        var usedAliases = new HashSet<string>();
        var rawStart = 0;
        var index = 0;

        while (index < source.Length)
        {
            if (source[index] != '[')
            {
                index++;
                continue;
            }

            if (IsClosingEditorTag(source, index))
                return false;

            if (!TryReadOpeningTag(source, index, out var tag, out var openingEnd))
            {
                index++;
                continue;
            }

            AddRaw(elements, source[rawStart..index], appendOnly, handwritingStyle);

            var closingTag = $"[/{tag.Name}]";
            var closingStart = FindUnescaped(source, closingTag, openingEnd);
            var hasClosingTag = closingStart >= 0;
            if ((tag.Name == "w" || tag.Alias != null) && !hasClosingTag)
                return false;

            var value = hasClosingTag
                ? UnescapeValue(source[openingEnd..closingStart], closingTag)
                : string.Empty;
            var nextIndex = hasClosingTag
                ? closingStart + closingTag.Length
                : openingEnd;

            if (!TryCreateElement(tag, value, usedAliases, handwritingStyle, out var element))
                return false;

            elements.Add(element);
            index = nextIndex;
            rawStart = nextIndex;
        }

        AddRaw(elements, source[rawStart..], appendOnly, handwritingStyle);
        return true;
    }

    private void AppendAliasedTag(StringBuilder builder, string tag, StructuredPaperElement element)
    {
        var alias = (_aliases.Count + 1).ToString();
        _aliases.Add(alias, element.Copy());
        var closingTag = $"[/{tag}]";
        builder.Append('[').Append(tag).Append(':').Append(alias).Append(']');
        builder.Append(EscapeValue(element.Text, closingTag));
        builder.Append(closingTag);
    }

    private bool TryCreateElement(
        EditorTag tag,
        string value,
        HashSet<string> usedAliases,
        PaperHandwritingStyle handwritingStyle,
        out StructuredPaperElement element)
    {
        var type = tag.Name switch
        {
            "f" => StructuredPaperElementType.SingleLineField,
            "lf" => StructuredPaperElementType.MultilineField,
            "sign" => StructuredPaperElementType.Signature,
            "w" => StructuredPaperElementType.HandwrittenText,
            _ => throw new InvalidOperationException(),
        };

        if (type is (StructuredPaperElementType.SingleLineField or StructuredPaperElementType.Signature) &&
            (value.Contains('\n') || value.Contains('\r')))
        {
            element = default!;
            return false;
        }

        if (tag.Alias == null)
        {
            element = new StructuredPaperElement(string.Empty, type, value, newLineAfter: false)
            {
                HandwritingStyle = handwritingStyle,
            };
            return true;
        }

        if (!usedAliases.Add(tag.Alias) || !_aliases.TryGetValue(tag.Alias, out var original))
        {
            element = default!;
            return false;
        }

        element = original.Copy();
        var typeChanged = element.Type != type;
        element.Type = type;
        element.Text = value;
        element.NewLineAfter = false;
        element.LocId = null;
        if (typeChanged)
            element.MaxLength = 0;
        if (type == StructuredPaperElementType.HandwrittenText)
        {
            element.HandwritingStyle = original.HandwritingStyle;
            element.PreviousText = string.Empty;
            element.PreviousHandwritingStyle = PaperHandwritingStyle.Default;
            element.Revisions.Clear();
        }
        return true;
    }

    private static void AddRaw(
        List<StructuredPaperElement> elements,
        string text,
        bool appendOnly,
        PaperHandwritingStyle handwritingStyle)
    {
        if (text.Length == 0)
            return;

        var type = appendOnly
            ? StructuredPaperElementType.HandwrittenText
            : StructuredPaperElementType.StaticText;
        if (elements.Count > 0 && elements[^1].Type == type && string.IsNullOrEmpty(elements[^1].Id))
        {
            elements[^1].Text += text;
            return;
        }

        elements.Add(new StructuredPaperElement(string.Empty, type, text, newLineAfter: false)
        {
            HandwritingStyle = type == StructuredPaperElementType.HandwrittenText
                ? handwritingStyle
                : PaperHandwritingStyle.Default,
        });
    }

    private static bool TryReadOpeningTag(string source, int start, out EditorTag tag, out int end)
    {
        tag = default;
        end = start;
        var bracket = source.IndexOf(']', start + 1);
        if (bracket < 0)
            return false;

        var header = source[(start + 1)..bracket];
        var separator = header.IndexOf(':');
        var name = separator < 0 ? header : header[..separator];
        if (name is not ("f" or "lf" or "sign" or "w"))
            return false;

        string? alias = null;
        if (separator >= 0)
        {
            alias = header[(separator + 1)..];
            if (alias.Length == 0 || alias.Any(character => !char.IsAsciiDigit(character)))
                return false;
        }

        tag = new EditorTag(name, alias);
        end = bracket + 1;
        return true;
    }

    private static bool IsClosingEditorTag(string source, int index)
    {
        return source.AsSpan(index).StartsWith("[/f]") ||
            source.AsSpan(index).StartsWith("[/lf]") ||
            source.AsSpan(index).StartsWith("[/sign]") ||
            source.AsSpan(index).StartsWith("[/w]");
    }

    private static int FindUnescaped(string source, string value, int start)
    {
        var index = source.IndexOf(value, start, StringComparison.Ordinal);
        while (index >= 0)
        {
            var slashes = 0;
            for (var cursor = index - 1; cursor >= 0 && source[cursor] == '\\'; cursor--)
                slashes++;

            if (slashes % 2 == 0)
                return index;

            index = source.IndexOf(value, index + value.Length, StringComparison.Ordinal);
        }

        return -1;
    }

    private static string EscapeValue(string value, string closingTag)
    {
        return value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace(closingTag, $"\\{closingTag}", StringComparison.Ordinal);
    }

    private static string UnescapeValue(string value, string closingTag)
    {
        return value.Replace($"\\{closingTag}", closingTag, StringComparison.Ordinal)
            .Replace("\\\\", "\\", StringComparison.Ordinal);
    }

    private readonly record struct EditorTag(string Name, string? Alias);
}
