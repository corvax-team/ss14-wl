using System.Numerics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;
using Robust.Shared.Timing;

namespace Content.Client._WL.UI;

public sealed class AnimatedProgressBar : Robust.Client.UserInterface.Controls.Range
{
    private float _time;

    private readonly List<Vector2> _polygon = new(4);
    private readonly List<Vector2> _triangles = new(6);

    public Color BackgroundColor { get; set; } = new(0.12f, 0.12f, 0.12f, 1f);

    public Color FillColor { get; set; } = new(0.25f, 0.65f, 0.25f, 1f);

    public Color StripeColor { get; set; } = new(1f, 1f, 1f, 0.18f);

    public float StripeWidth { get; set; } = 14f;

    public float StripeGap { get; set; } = 14f;

    public float StripeSpeed { get; set; } = 40f;

    public float StripeSkew { get; set; } = 1f;

    public void SetTime(float time)
    {
        _time = time;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        // Накапливаем время для анимации.
        _time += args.DeltaSeconds;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        var size = PixelSize;

        if (size.X <= 0f || size.Y <= 0f)
            return;

        handle.DrawRect(new UIBox2(0f, 0f, size.X, size.Y), BackgroundColor);

        var fillWidth = size.X * GetAsRatio();

        if (fillWidth <= 0f)
            return;

        var fillRect = new UIBox2(0f, 0f, fillWidth, size.Y);
        handle.DrawRect(fillRect, FillColor);

        var period = StripeWidth + StripeGap;

        if (period <= 0f || StripeWidth <= 0f)
            return;

        var offset = _time * StripeSpeed % period;

        if (offset < 0f)
            offset += period;

        var skew = size.Y * StripeSkew;

        for (var x = -period + offset; x < skew + fillWidth + period; x += period)
        {
            _polygon.Clear();

            _polygon.Add(new Vector2(x, 0f));
            _polygon.Add(new Vector2(x + StripeWidth, 0f));
            _polygon.Add(new Vector2(x + StripeWidth - skew, size.Y));
            _polygon.Add(new Vector2(x - skew, size.Y));

            var clipped = ClipPolygon(_polygon, fillRect);

            if (clipped.Count < 3)
                continue;

            _triangles.Clear();

            for (var i = 1; i < clipped.Count - 1; i++)
            {
                _triangles.Add(clipped[0]);
                _triangles.Add(clipped[i]);
                _triangles.Add(clipped[i + 1]);
            }

            handle.DrawPrimitives(
                DrawPrimitiveTopology.TriangleList,
                CollectionsMarshal.AsSpan(_triangles),
                StripeColor
            );
        }
    }

    private static List<Vector2> ClipPolygon(List<Vector2> polygon, UIBox2 rect)
    {
        var result = polygon;

        result = ClipEdge(result, rect, 0);
        if (result.Count == 0)
            return result;

        result = ClipEdge(result, rect, 1);
        if (result.Count == 0)
            return result;

        result = ClipEdge(result, rect, 2);
        if (result.Count == 0)
            return result;

        result = ClipEdge(result, rect, 3);
        return result;
    }

    private static List<Vector2> ClipEdge(List<Vector2> polygon, UIBox2 rect, int edge)
    {
        var result = new List<Vector2>();

        if (polygon.Count == 0)
            return result;

        var prev = polygon[^1];
        var prevInside = IsInside(prev, rect, edge);

        foreach (var current in polygon)
        {
            var currentInside = IsInside(current, rect, edge);

            if (currentInside)
            {
                if (!prevInside)
                    result.Add(Intersect(prev, current, rect, edge));

                result.Add(current);
            }
            else if (prevInside)
            {
                result.Add(Intersect(prev, current, rect, edge));
            }

            prev = current;
            prevInside = currentInside;
        }

        return result;
    }

    private static bool IsInside(Vector2 p, UIBox2 rect, int edge)
    {
        return edge switch
        {
            0 => p.X >= rect.Left,
            1 => p.X <= rect.Right,
            2 => p.Y >= rect.Top,
            3 => p.Y <= rect.Bottom,
            _ => true
        };
    }

    private static Vector2 Intersect(Vector2 a, Vector2 b, UIBox2 rect, int edge)
    {
        var d = b - a;
        float t;

        switch (edge)
        {
            case 0: // left
                if (MathF.Abs(d.X) < 1e-6f)
                    return a;

                t = (rect.Left - a.X) / d.X;
                break;

            case 1: // right
                if (MathF.Abs(d.X) < 1e-6f)
                    return a;

                t = (rect.Right - a.X) / d.X;
                break;

            case 2: // top
                if (MathF.Abs(d.Y) < 1e-6f)
                    return a;

                t = (rect.Top - a.Y) / d.Y;
                break;

            case 3: // bottom
                if (MathF.Abs(d.Y) < 1e-6f)
                    return a;

                t = (rect.Bottom - a.Y) / d.Y;
                break;

            default:
                return a;
        }

        return a + d * t;
    }
}
