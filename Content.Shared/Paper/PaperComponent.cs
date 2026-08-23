using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Content.Shared._WL.Paper; // WL-Changes: Structured paper forms

namespace Content.Shared.Paper;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PaperComponent : Component
{
    public PaperAction Mode;
    [DataField("content"), AutoNetworkedField]
    public string Content { get; set; } = "";

    [DataField("contentSize")]
    public int ContentSize { get; set; } = 10000;

    [DataField("stampedBy"), AutoNetworkedField]
    public List<StampDisplayInfo> StampedBy { get; set; } = new();

    /// <summary>
    ///     Stamp to be displayed on the paper, state from bureaucracy.rsi
    /// </summary>
    [DataField("stampState"), AutoNetworkedField]
    public string? StampState { get; set; }

    [DataField, AutoNetworkedField]
    public bool EditingDisabled;

    /// <summary>
    /// Sound played after writing to the paper.
    /// </summary>
    [DataField("sound")]
    public SoundSpecifier? Sound { get; private set; } = new SoundCollectionSpecifier("PaperScribbles", AudioParams.Default.WithVariation(0.1f));

    [Serializable, NetSerializable]
    public sealed class PaperBoundUserInterfaceState : BoundUserInterfaceState
    {
        public readonly string Text;
        public readonly List<StampDisplayInfo> StampedBy;
        public readonly PaperAction Mode;
        // WL-Changes-StructuredPaper-Start
        public readonly List<StructuredPaperElement>? Elements;
        public readonly PaperEditAccess EditAccess;
        public readonly NetEntity? Editor;
        // WL-Changes-StructuredPaper-End

        public PaperBoundUserInterfaceState(
            string text,
            List<StampDisplayInfo> stampedBy,
            PaperAction mode = PaperAction.Read,
            List<StructuredPaperElement>? elements = null,
            PaperEditAccess editAccess = PaperEditAccess.None,
            NetEntity? editor = null)
        {
            Text = text;
            StampedBy = stampedBy;
            Mode = mode;
            // WL-Changes-StructuredPaper-Start
            Elements = elements;
            EditAccess = editAccess;
            Editor = editor;
            // WL-Changes-StructuredPaper-End
        }
    }

    [Serializable, NetSerializable]
    public sealed class PaperInputTextMessage : BoundUserInterfaceMessage
    {
        public readonly string Text;

        public PaperInputTextMessage(string text)
        {
            Text = text;
        }
    }

    // WL-Changes-StructuredPaper-Start
    [Serializable, NetSerializable]
    public sealed class PaperInputFieldMessage : BoundUserInterfaceMessage
    {
        public readonly string FieldId;
        public readonly string Text;

        public PaperInputFieldMessage(string fieldId, string text)
        {
            FieldId = fieldId;
            Text = text;
        }
    }

    [Serializable, NetSerializable]
    public sealed class PaperRequestFieldEditMessage : BoundUserInterfaceMessage
    {
        public readonly string FieldId;

        public PaperRequestFieldEditMessage(string fieldId)
        {
            FieldId = fieldId;
        }
    }

    [Serializable, NetSerializable]
    public sealed class PaperSignFieldMessage : BoundUserInterfaceMessage
    {
        public readonly string FieldId;

        public PaperSignFieldMessage(string fieldId)
        {
            FieldId = fieldId;
        }
    }

    [Serializable, NetSerializable]
    public sealed class PaperAppendTextMessage : BoundUserInterfaceMessage
    {
        public readonly string Text;

        public PaperAppendTextMessage(string text)
        {
            Text = text;
        }
    }

    [Serializable, NetSerializable]
    public sealed class PaperAppendElementsMessage : BoundUserInterfaceMessage
    {
        public readonly List<StructuredPaperElement> Elements;

        public PaperAppendElementsMessage(List<StructuredPaperElement> elements)
        {
            Elements = elements;
        }
    }

    [Serializable, NetSerializable]
    public sealed class PaperInputStructureMessage : BoundUserInterfaceMessage
    {
        public readonly List<StructuredPaperElement> Elements;

        public PaperInputStructureMessage(List<StructuredPaperElement> elements)
        {
            Elements = elements;
        }
    }
    // WL-Changes-StructuredPaper-End

    [Serializable, NetSerializable]
    public enum PaperUiKey
    {
        Key
    }

    [Serializable, NetSerializable]
    public enum PaperAction
    {
        Read,
        Write,
    }

    [Serializable, NetSerializable]
    public enum PaperVisuals : byte
    {
        Status,
        Stamp
    }

    [Serializable, NetSerializable]
    public enum PaperStatus : byte
    {
        Blank,
        Written
    }
}
