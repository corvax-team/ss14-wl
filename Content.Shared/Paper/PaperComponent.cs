using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using System.Numerics; // WL-Changes

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

    // WL-Changes-Start
    [DataField, AutoNetworkedField]
    public List<PaperStroke> Drawings { get; set; } = new();
    // WL-Changes-End

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
        public readonly List<PaperStroke> Drawings; // WL-Changes
        public PaperBoundUserInterfaceState(string text, List<StampDisplayInfo> stampedBy, PaperAction mode = PaperAction.Read, List<PaperStroke>? drawings = null) // WL-Changes
        {
            Text = text;
            StampedBy = stampedBy;
            Mode = mode;
            Drawings = drawings ?? new List<PaperStroke>(); // WL-Changes
        }
    }

    [Serializable, NetSerializable]
    public sealed class PaperInputTextMessage : BoundUserInterfaceMessage
    {
        public readonly string Text;
        public readonly List<PaperStroke> Drawings; // WL-Changes

        public PaperInputTextMessage(string text, List<PaperStroke> drawings) // WL-Changes
        {
            Text = text;
            Drawings = drawings; // WL-Changes
        }
    }

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

// WL-Changes-Start
[DataDefinition, Serializable, NetSerializable]
public sealed partial class PaperStroke
{
    [DataField]
    public List<Vector2> Points = new();

    [DataField]
    public Color Color;

    [DataField]
    public float Radius;

    [DataField]
    public bool Eraser;

    public PaperStroke()
    {
    }

    public PaperStroke(List<Vector2> points, Color color, float radius, bool eraser)
    {
        Points = points;
        Color = color;
        Radius = radius;
        Eraser = eraser;
    }
}
// WL-Changes-End
