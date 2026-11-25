using System.Numerics;
using System.Linq;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Drawing.Components;


[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PaperDrawingComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<PaperStroke> Strokes = new();
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class PaperStroke : IEquatable<PaperStroke>
{
    [DataField]
    public List<Vector2> Points = new();

    [DataField]
    public Color Color;

    [DataField]
    public float Radius;

    [DataField]
    public bool Eraser;

    public const float MaxRadius = 25f;

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
    public bool IsValid()
    {
        return Points.Count > 0
               && float.IsFinite(Radius) && Radius is >= 1f and <= MaxRadius
               && float.IsFinite(Color.R) && Color.R is >= 0f and <= 1f
               && float.IsFinite(Color.G) && Color.G is >= 0f and <= 1f
               && float.IsFinite(Color.B) && Color.B is >= 0f and <= 1f
               && float.IsFinite(Color.A) && Color.A is >= 0f and <= 1f;
    }

    public bool Equals(PaperStroke? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Eraser == other.Eraser
               && Color == other.Color
               && Radius.Equals(other.Radius)
               && Points.SequenceEqual(other.Points);
    }

    public override bool Equals(object? obj) => Equals(obj as PaperStroke);

    public override int GetHashCode() => HashCode.Combine(Eraser, Color, Radius, Points.Count);
}
