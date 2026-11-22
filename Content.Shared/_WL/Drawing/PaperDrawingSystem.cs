using Content.Shared._WL.CCVars;
using Content.Shared._WL.Drawing.Components;
using Content.Shared.Paper;
using Robust.Shared.Configuration;
using System.Linq;
using System.Numerics;

namespace Content.Shared._WL.Drawing;

public sealed partial class PaperDrawingSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _configurationManager = default!;

    private const float MaxStrokeRadius = 25f; // WL-Changes

    public List<PaperStroke> SanitizeDrawings(List<PaperStroke> drawings)
    {
        var (maxStrokes, maxPoints) = GetDrawingLimits();
        var result = new List<PaperStroke>();
        var pointCount = 0;

        foreach (var stroke in drawings)
        {
            if (result.Count >= maxStrokes || pointCount >= maxPoints)
                break;

            if (stroke.Points.Count == 0
                || !float.IsFinite(stroke.Radius) || stroke.Radius is < 1f or > MaxStrokeRadius
                || !float.IsFinite(stroke.Color.R) || stroke.Color.R is < 0f or > 1f
                || !float.IsFinite(stroke.Color.G) || stroke.Color.G is < 0f or > 1f
                || !float.IsFinite(stroke.Color.B) || stroke.Color.B is < 0f or > 1f
                || !float.IsFinite(stroke.Color.A) || stroke.Color.A is < 0f or > 1f)
                continue;

            var room = maxPoints - pointCount;
            var points = new List<Vector2>(Math.Min(stroke.Points.Count, room));

            foreach (var point in stroke.Points)
            {
                if (points.Count >= room)
                    break;

                if (!float.IsFinite(point.X) || !float.IsFinite(point.Y))
                    continue;

                points.Add(Vector2.Clamp(point, Vector2.Zero, Vector2.One));
            }

            if (points.Count == 0)
                continue;

            pointCount += points.Count;

            result.Add(new PaperStroke(points, stroke.Color, stroke.Radius, stroke.Eraser));
        }

        return result;
    }

    public (int MaxStrokes, int MaxPoints) GetDrawingLimits()
    {
        return (_configurationManager.GetCVar(WLCVars.PaperMaxStrokes), _configurationManager.GetCVar(WLCVars.PaperMaxPoints));
    }

    public void SetDrawings(Entity<PaperDrawingComponent> entity, List<PaperStroke>? drawings)
    {
        entity.Comp.Strokes = drawings == null
            ? new List<PaperStroke>()
            : SanitizeDrawings(drawings);

        Dirty(entity);
    }

    public static bool IsAppendOnly(List<PaperStroke> old, List<PaperStroke> updated)
    {
        if (updated.Count < old.Count)
            return false;

        for (var i = 0; i < old.Count; i++)
        {
            var a = old[i];
            var b = updated[i];

            if (a.Eraser != b.Eraser
                || a.Color != b.Color
                || a.Radius != b.Radius
                || !a.Points.SequenceEqual(b.Points))
                return false;
        }

        return true;
    }
}
