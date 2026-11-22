using System.Numerics;
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
