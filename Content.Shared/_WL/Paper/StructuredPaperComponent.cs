using System.Linq;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Paper;

/// <summary>
/// Stores an ordered paper form separately from legacy free-form paper content.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StructuredPaperComponent : Component
{
    /// <summary>
    /// Localization key for a prototype-authored form template. It is parsed into <see cref="Elements"/> on map init
    /// and then cleared so runtime state and saved entities use only the element list.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public string? TemplateLocId;

    [DataField]
    [AutoNetworkedField]
    public List<StructuredPaperElement> Elements = new();
}

[Serializable, NetSerializable, DataDefinition]
public sealed partial class StructuredPaperElement
{
    public const int DefaultSingleLineMaxLength = 128;
    public const int DefaultMultilineMaxLength = 2048;

    [DataField]
    public string Id = string.Empty;

    [DataField]
    public StructuredPaperElementType Type;

    [DataField]
    public string Text = string.Empty;

    [DataField]
    public string PreviousText = string.Empty;

    [DataField]
    public PaperHandwritingStyle PreviousHandwritingStyle;

    /// <summary>
    /// Earlier physical entries kept on the sheet when this field is corrected.
    /// </summary>
    [DataField]
    public List<PaperFieldRevision> Revisions = new();

    /// <summary>
    /// Localization key used to initialize printed prototype text.
    /// Runtime field values are stored only in <see cref="Text"/>.
    /// </summary>
    [DataField]
    public string? LocId;

    /// <summary>
    /// Ends the current visual line after this element. Elements with this disabled are laid out inline.
    /// </summary>
    [DataField]
    public bool NewLineAfter = true;

    /// <summary>
    /// Maximum field value length. Zero uses the default for the field type.
    /// </summary>
    [DataField]
    public int MaxLength;

    /// <summary>
    /// Stored with the written element so the writer's character preference is rendered consistently
    /// after trading, copying, faxing, or saving the paper.
    /// </summary>
    [DataField]
    public PaperHandwritingStyle HandwritingStyle;

    public StructuredPaperElement()
    {
    }

    public StructuredPaperElement(
        string id,
        StructuredPaperElementType type,
        string text,
        string? locId = null,
        bool newLineAfter = true,
        int maxLength = 0)
    {
        Id = id;
        Type = type;
        Text = text;
        LocId = locId;
        NewLineAfter = newLineAfter;
        MaxLength = maxLength;
    }

    public StructuredPaperElement Copy()
    {
        return new StructuredPaperElement(Id, Type, Text, LocId, NewLineAfter, MaxLength)
        {
            PreviousText = this.PreviousText,
            PreviousHandwritingStyle = this.PreviousHandwritingStyle,
            HandwritingStyle = this.HandwritingStyle,
            Revisions = Revisions.Select(revision => revision.Copy()).ToList(),
        };
    }

    public int GetMaxLength()
    {
        if (MaxLength > 0)
            return MaxLength;

        return Type switch
        {
            StructuredPaperElementType.SingleLineField => DefaultSingleLineMaxLength,
            StructuredPaperElementType.MultilineField => DefaultMultilineMaxLength,
            StructuredPaperElementType.SignatureField => DefaultSingleLineMaxLength,
            StructuredPaperElementType.Signature => DefaultSingleLineMaxLength,
            _ => int.MaxValue,
        };
    }
}

[Serializable, NetSerializable, DataDefinition]
public sealed partial class PaperFieldRevision
{
    [DataField]
    public string Text = string.Empty;

    [DataField]
    public PaperHandwritingStyle HandwritingStyle;

    public PaperFieldRevision()
    {
    }

    public PaperFieldRevision(string text, PaperHandwritingStyle handwritingStyle)
    {
        Text = text;
        HandwritingStyle = handwritingStyle;
    }

    public PaperFieldRevision Copy()
    {
        return new PaperFieldRevision(Text, HandwritingStyle);
    }
}

[Serializable, NetSerializable]
public enum StructuredPaperElementType : byte
{
    StaticText,
    SingleLineField,
    MultilineField,
    /// <summary>
    /// Legacy serialized form slot. It is intentionally edited like a normal single-line field.
    /// </summary>
    SignatureField,
    HandwrittenText,
    Signature,
}

[Serializable, NetSerializable]
public enum PaperEditAccess : byte
{
    None,
    FreeText,
    Fields,
    Full,
}

/// <summary>
/// Per-character handwriting selection stored on every written field.
/// </summary>
[Serializable, NetSerializable]
public enum PaperHandwritingStyle : byte
{
    Default,
    Neat,
    Quick,
    Formal,
    Heavy,
    Messy,
}
