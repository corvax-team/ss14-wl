using System.Numerics;
using System.Linq;
using System.Runtime.InteropServices;
using Content.Shared.Paper;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Input;
using Robust.Shared.Prototypes;
using Robust.Shared.Graphics;
using Content.Shared.Input;
using Robust.Shared.Configuration;
using Content.Shared._WL.CCVars;
using System.Diagnostics.CodeAnalysis;

namespace Content.Client._WL.Paper.UI;

public sealed partial class PaperDrawingControl : Control
{
    private const int VerticesPerDraw = 510;
    private const int MinCapSegments = 12;
    private const int MaxCapSegments = 32;
    private const float MinRadius = 0.5f;

    [Dependency] private IClyde _clyde = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private IConfigurationManager _configurationManager = default!;

    private ShaderInstance _eraserShader = default!;

    private readonly List<StrokeRenderData> _strokes = new();
    private readonly List<StrokeRenderData> _redoStrokes = new();

    private StrokeRenderData? _activeStroke;
    private IRenderTexture? _drawingTexture;
    private IRenderTexture? _scratchTexture;

    private Vector2 _lastPoint;
    private Vector2 _startPoint;
    private int _pointCount;
    private float _lastUiScale = -1f;
    private bool _renderTextureNeedsRebuild = true;
    private bool _hasPendingVertices = true;
    private bool _activeIsShape;
    private bool _activeIsDeferred;
    private int _lockedStrokeCount;

    private int MaxStrokes => _configurationManager.GetCVar(WLCVars.PaperMaxStrokes);
    private int MaxPoints => _configurationManager.GetCVar(WLCVars.PaperMaxPoints);

    private static readonly ProtoId<ShaderPrototype> EraserShaderId = "PaintEraser";

    public event Action? HistoryChanged;
    public bool CanUndo => _activeStroke == null && _strokes.Count > _lockedStrokeCount;
    public bool CanRedo => _activeStroke == null && _redoStrokes.Count > 0;

    public int StrokeCount => _strokes.Count;

    public bool CanDraw
    {
        get => MouseFilter == MouseFilterMode.Stop;
        set => MouseFilter = value ? MouseFilterMode.Stop : MouseFilterMode.Ignore;
    }

    public Color BrushColor { get; set; } = Color.Black;
    public float BrushRadius { get; set; } = 3f;
    public bool Eraser { get; set; }

    public enum DrawTool { Freehand, Line, Circle, Pipette }
    public DrawTool Tool { get; set; }

    public PaperDrawingControl()
    {
        IoCManager.InjectDependencies(this);
        _eraserShader = _prototypeManager.Index(EraserShaderId).InstanceUnique();
    }

    public void SetStrokes(IEnumerable<PaperStroke> strokes)
    {
        var incoming = strokes.ToList();

        if (_activeStroke != null)
            return;

        if (SameStrokes(incoming))
        {
            _lockedStrokeCount = _strokes.Count;
            _redoStrokes.Clear();

            HistoryChanged?.Invoke();
            return;
        }

        _redoStrokes.Clear();
        _strokes.Clear();
        _pointCount = 0;

        foreach (var stroke in incoming)
        {
            _strokes.Add(new StrokeRenderData(CloneStroke(stroke)));
            _pointCount += stroke.Points.Count;
        }

        _lockedStrokeCount = _strokes.Count;
        _renderTextureNeedsRebuild = true;
        HistoryChanged?.Invoke();
    }
    private bool SameStrokes(List<PaperStroke> incoming)
    {
        if (incoming.Count != _strokes.Count)
            return false;

        for (var i = 0; i < incoming.Count; i++)
        {
            var a = incoming[i];
            var b = _strokes[i].Stroke;

            if (a.Eraser != b.Eraser || a.Color != b.Color || a.Radius != b.Radius
                || !a.Points.SequenceEqual(b.Points))
                return false;
        }

        return true;
    }

    public List<PaperStroke> GetStrokes()
    {
        return _strokes.Select(stroke => CloneStroke(stroke.Stroke)).ToList();
    }

    public bool TryGetStroke(Vector2 coords, [NotNullWhen(true)] out PaperStroke? resultStroke)
    {
        resultStroke = null;

        for (var i = _strokes.Count - 1; i >= 0; i--)
        {
            var stroke = _strokes[i].Stroke;
            var points = stroke.Points;

            if (points.Count == 0)
                continue;

            var radius = stroke.Radius / Math.Min(PixelWidth, PixelHeight);

            if (points.Count == 1)
            {
                if (Vector2.DistanceSquared(coords, points[0]) <= radius * radius)
                {
                    resultStroke = CloneStroke(stroke);
                    return true;
                }

                continue;
            }

            for (var j = 1; j < points.Count; j++)
            {
                var a = points[j - 1];
                var b = points[j];

                var ab = b - a;
                var lengthSquared = ab.LengthSquared();

                var t = lengthSquared > 0f
                    ? Math.Clamp(Vector2.Dot(coords - a, ab) / lengthSquared, 0f, 1f)
                    : 0f;

                var closest = a + ab * t;

                if (Vector2.DistanceSquared(coords, closest) <= radius * radius)
                {
                    resultStroke = CloneStroke(stroke);
                    return true;
                }
            }
        }

        return false;
    }

    public bool Undo()
    {
        if (_activeStroke != null || _strokes.Count <= _lockedStrokeCount)
            return false;

        var last = _strokes[^1];
        _strokes.RemoveAt(_strokes.Count - 1);
        _redoStrokes.Add(last);
        _pointCount -= last.Stroke.Points.Count;
        _renderTextureNeedsRebuild = true;

        HistoryChanged?.Invoke();
        return true;
    }

    public bool Redo()
    {
        if (_activeStroke != null || _redoStrokes.Count == 0)
            return false;

        var stroke = _redoStrokes[^1];
        _redoStrokes.RemoveAt(_redoStrokes.Count - 1);
        _strokes.Add(stroke);
        _pointCount += stroke.Stroke.Points.Count;
        _renderTextureNeedsRebuild = true;

        HistoryChanged?.Invoke();
        return true;
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);

        if (CanDraw)
        {
            if (args.Function == ContentKeyFunctions.PaperDrawingUndo)
            {
                Undo();
                args.Handle();
                return;
            }

            if (args.Function == ContentKeyFunctions.PaperDrawingRedo)
            {
                Redo();
                args.Handle();
                return;
            }
        }

        if (!CanDraw
            || args.Function != EngineKeyFunctions.Use
            || PixelWidth <= 0
            || PixelHeight <= 0
            || _strokes.Count >= MaxStrokes
            || _pointCount >= MaxPoints)
            return;

        _redoStrokes.Clear();

        var point = Normalize(args.PointerLocation.Position - GlobalPixelPosition);

        if (Tool == DrawTool.Pipette)
        {
            if (!TryGetStroke(point, out var stroke) || stroke is null)
                return;

            BrushColor = stroke.Color;
            return;
        }

        _activeStroke = new StrokeRenderData(new PaperStroke(new List<Vector2> { point }, BrushColor, BrushRadius, Eraser));
        _strokes.Add(_activeStroke);
        _activeIsShape = Tool != DrawTool.Freehand;
        _activeIsDeferred = _activeIsShape || (!Eraser && BrushColor.A < 0.999f);
        _startPoint = point;
        _pointCount++;
        _lastPoint = point;
        _activeStroke.AddDot(ToControlPoint(point, new Vector2(PixelWidth, PixelHeight)), PixelWidth, PixelHeight, UIScale);
        _hasPendingVertices = true;

        HistoryChanged?.Invoke();
        args.Handle();
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        if (_activeStroke != null && args.Function == EngineKeyFunctions.Use)
        {
            _activeStroke = null;
            _activeIsShape = false;
            _activeIsDeferred = false;
            _renderTextureNeedsRebuild = true;
            _hasPendingVertices = true;

            HistoryChanged?.Invoke();
            args.Handle();
        }

        base.KeyBindUp(args);
    }

    protected override void MouseMove(GUIMouseMoveEventArgs args)
    {
        base.MouseMove(args);

        if (_activeStroke == null || PixelWidth <= 0 || PixelHeight <= 0)
            return;

        var point = Normalize(args.GlobalPixelPosition.Position - GlobalPixelPosition);

        if (Vector2.DistanceSquared(point, _lastPoint) < 0.000004f)
            return;

        if (_activeIsShape)
        {
            var size = new Vector2(PixelWidth, PixelHeight);
            var newPoints = Tool == DrawTool.Circle
                ? BuildCircle(_startPoint, point, size)
                : new List<Vector2> { _startPoint, point };

            var points = _activeStroke.Stroke.Points;
            var delta = newPoints.Count - points.Count;

            if (_pointCount + delta > MaxPoints)
                return;

            points.Clear();
            points.AddRange(newPoints);
            _pointCount += delta;
            _lastPoint = point;
            _activeStroke.UpdateLineGeometry(PixelWidth, PixelHeight, UIScale);

            return;
        }

        if (_pointCount >= MaxPoints)
            return;

        _activeStroke.Stroke.Points.Add(point);
        _pointCount++;
        _lastPoint = point;
        _activeStroke.AddPoint(ToControlPoint(point, new Vector2(PixelWidth, PixelHeight)), PixelWidth, PixelHeight, UIScale);
        _hasPendingVertices = true;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        var pixelWidth = PixelWidth;
        var pixelHeight = PixelHeight;

        if (pixelWidth <= 0 || pixelHeight <= 0)
            return;

        var uiScale = UIScale;
        var oldTransform = handle.GetTransform();

        var ss = uiScale >= 2f ? 1 : 2;
        var targetSize = new Vector2i(pixelWidth * ss, pixelHeight * ss);

        if (_drawingTexture?.Texture.Size != targetSize)
        {
            _drawingTexture?.Dispose();
            _drawingTexture = _clyde.CreateRenderTarget(
                targetSize,
                new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb),
                new TextureSampleParameters { Filter = true },
                name: "paper-drawing");
            _renderTextureNeedsRebuild = true;
        }

        if (_lastUiScale != uiScale)
        {
            _lastUiScale = uiScale;
            _renderTextureNeedsRebuild = true;
        }

        foreach (var data in _strokes)
            data.EnsureGeometry(pixelWidth, pixelHeight, uiScale);

        if (_renderTextureNeedsRebuild)
        {
            foreach (var data in _strokes)
            {
                data.RenderedVertexCount = 0;
                data.Baked = false;
            }
        }

        if (_renderTextureNeedsRebuild || _hasPendingVertices)
        {
            if (_renderTextureNeedsRebuild)
                handle.RenderInRenderTarget(_drawingTexture!, () => { }, new Color(0f, 0f, 0f, 0f));

            var i = 0;
            while (i < _strokes.Count)
            {
                var current = _strokes[i];

                if (_activeIsDeferred && current == _activeStroke)
                {
                    i++;
                    continue;
                }

                if (NeedsLayer(current))
                {
                    if (!current.Baked)
                    {
                        EnsureScratch(targetSize);
                        RenderToScratch(handle, current, ss);

                        var alpha = new Color(1f, 1f, 1f, current.Stroke.Color.A);

                        handle.RenderInRenderTarget(_drawingTexture!, () =>
                        {
                            handle.SetTransform(Matrix3x2.Identity);
                            handle.DrawTextureRect(_scratchTexture!.Texture,
                                UIBox2.FromDimensions(Vector2.Zero, new Vector2(targetSize.X, targetSize.Y)), alpha);
                        }, null);

                        current.Baked = true;
                    }

                    i++;
                    continue;
                }

                var from = i;

                while (i < _strokes.Count
                       && !NeedsLayer(_strokes[i])
                       && !(_activeIsDeferred && _strokes[i] == _activeStroke))
                    i++;

                var to = i;

                handle.RenderInRenderTarget(_drawingTexture!, () =>
                {
                    handle.SetTransform(Matrix3x2.CreateScale(ss));
                    for (var k = from; k < to; k++)
                        DrawDirect(handle, _strokes[k]);
                }, null);
            }

            _renderTextureNeedsRebuild = false;
            _hasPendingVertices = false;
        }

        handle.SetTransform(oldTransform);

        handle.DrawTextureRect(_drawingTexture!.Texture,
            UIBox2.FromDimensions(Vector2.Zero, new Vector2(pixelWidth, pixelHeight)));

        if (_activeIsDeferred && _activeStroke is { } active)
        {
            if (NeedsLayer(active))
            {
                EnsureScratch(targetSize);
                RenderToScratch(handle, active, ss);
                handle.SetTransform(oldTransform);

                handle.DrawTextureRect(_scratchTexture!.Texture,
                    UIBox2.FromDimensions(Vector2.Zero, new Vector2(pixelWidth, pixelHeight)),
                    new Color(1f, 1f, 1f, active.Stroke.Color.A));
            }
            else
            {
                var color = active.Stroke.Eraser ? new Color(1f, 1f, 1f, 0.5f) : active.Stroke.Color;
                DrawVertices(handle, active, 0, color);
                active.RenderedVertexCount = 0;
            }
        }
    }

    protected override void ExitedTree()
    {
        _drawingTexture?.Dispose();
        _drawingTexture = null;

        _scratchTexture?.Dispose();
        _scratchTexture = null;

        _renderTextureNeedsRebuild = true;
        _hasPendingVertices = true;

        base.ExitedTree();
    }

    private Vector2 Normalize(Vector2 position)
    {
        return Vector2.Clamp(position / new Vector2(PixelWidth, PixelHeight), Vector2.Zero, Vector2.One);
    }

    private static Vector2 ToControlPoint(Vector2 point, Vector2 size)
    {
        return point * size;
    }

    private static PaperStroke CloneStroke(PaperStroke stroke)
    {
        return new PaperStroke(new List<Vector2>(stroke.Points), stroke.Color, stroke.Radius, stroke.Eraser);
    }

    private static bool NeedsLayer(StrokeRenderData data)
    {
        return !data.Stroke.Eraser && data.Stroke.Color.A < 0.999f;
    }

    private void EnsureScratch(Vector2i size)
    {
        if (_scratchTexture?.Texture.Size == size)
            return;

        _scratchTexture?.Dispose();
        _scratchTexture = _clyde.CreateRenderTarget(
            size,
            new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb),
            new TextureSampleParameters { Filter = true },
            name: "paper-drawing-scratch");
    }

    private void DrawVertices(DrawingHandleScreen handle, StrokeRenderData data, int from, Color color)
    {
        var vertices = CollectionsMarshal.AsSpan(data.Vertices);

        for (var offset = from; offset < vertices.Length; offset += VerticesPerDraw)
        {
            var count = Math.Min(VerticesPerDraw, vertices.Length - offset);
            count -= count % 3;

            if (count == 0)
                break;

            handle.DrawPrimitives(DrawPrimitiveTopology.TriangleList, vertices.Slice(offset, count), color);
        }

        data.RenderedVertexCount = vertices.Length;
    }

    private void DrawDirect(DrawingHandleScreen handle, StrokeRenderData data)
    {
        var eraser = data.Stroke.Eraser;
        var color = eraser ? Color.White : Color.ToSrgb(data.Stroke.Color);

        if (eraser)
            handle.UseShader(_eraserShader);

        DrawVertices(handle, data, data.RenderedVertexCount, color);

        if (eraser)
            handle.UseShader(null);
    }

    private void RenderToScratch(DrawingHandleScreen handle, StrokeRenderData data, int ss)
    {
        var opaque = Color.ToSrgb(data.Stroke.Color.WithAlpha(1f));

        handle.RenderInRenderTarget(_scratchTexture!, () =>
        {
            handle.SetTransform(Matrix3x2.CreateScale(ss));
            DrawVertices(handle, data, 0, opaque);
        }, new Color(0f, 0f, 0f, 0f));

        handle.SetTransform(Matrix3x2.Identity);
    }

    private static List<Vector2> BuildCircle(Vector2 centerN, Vector2 edgeN, Vector2 size)
    {
        const int segments = 32;

        var center = centerN * size;
        var radius = Vector2.Distance(center, edgeN * size);
        var result = new List<Vector2>(segments + 1);

        for (var i = 0; i <= segments; i++)
        {
            var a = MathF.Tau * i / segments;
            var p = center + new Vector2(MathF.Cos(a), MathF.Sin(a)) * radius;
            result.Add(Vector2.Clamp(p / size, Vector2.Zero, Vector2.One));
        }

        return result;
    }

    private sealed class StrokeRenderData
    {
        public readonly PaperStroke Stroke;
        public readonly List<Vector2> Vertices = new();
        public int RenderedVertexCount;
        public bool Baked;

        private int _builtPointCount;
        private int _pixelWidth = -1;
        private int _pixelHeight = -1;
        private float _uiScale = -1f;

        public StrokeRenderData(PaperStroke stroke)
        {
            Stroke = stroke;
        }

        private float ScaledRadius(float uiScale)
        {
            return Math.Max(Stroke.Radius * uiScale, MinRadius);
        }

        public void AddDot(Vector2 point, int pixelWidth, int pixelHeight, float uiScale)
        {
            _pixelWidth = pixelWidth;
            _pixelHeight = pixelHeight;
            _uiScale = uiScale;
            AddCircle(point, ScaledRadius(uiScale));
            _builtPointCount = 1;
        }

        public void AddPoint(Vector2 point, int pixelWidth, int pixelHeight, float uiScale)
        {
            if (_pixelWidth != pixelWidth
                || _pixelHeight != pixelHeight
                || _uiScale != uiScale
                || _builtPointCount == 0)
            {
                EnsureGeometry(pixelWidth, pixelHeight, uiScale);
                return;
            }

            var radius = ScaledRadius(uiScale);
            var size = new Vector2(pixelWidth, pixelHeight);
            var previous = ToControlPoint(Stroke.Points[^2], size);
            var delta = point - previous;

            if (delta.LengthSquared() <= float.Epsilon)
            {
                AddCircle(point, radius);
                _builtPointCount = Stroke.Points.Count;
                return;
            }

            var direction = Vector2.Normalize(delta);
            var normal = new Vector2(-direction.Y, direction.X) * radius;

            Vertices.Add(previous + normal);
            Vertices.Add(previous - normal);
            Vertices.Add(point + normal);
            Vertices.Add(point + normal);
            Vertices.Add(previous - normal);
            Vertices.Add(point - normal);
            AddCircle(point, radius);
            _builtPointCount = Stroke.Points.Count;
        }

        public void UpdateLineGeometry(int pixelWidth, int pixelHeight, float uiScale)
        {
            _builtPointCount = 0;
            EnsureGeometry(pixelWidth, pixelHeight, uiScale);
        }

        public void EnsureGeometry(int pixelWidth, int pixelHeight, float uiScale)
        {
            if (_builtPointCount == Stroke.Points.Count
                && _pixelWidth == pixelWidth
                && _pixelHeight == pixelHeight
                && _uiScale == uiScale)
                return;

            Vertices.Clear();
            _pixelWidth = pixelWidth;
            _pixelHeight = pixelHeight;
            _uiScale = uiScale;
            _builtPointCount = 0;
            RenderedVertexCount = 0;
            Baked = false;

            if (Stroke.Points.Count == 0)
                return;

            var radius = ScaledRadius(uiScale);
            var size = new Vector2(pixelWidth, pixelHeight);
            var previous = ToControlPoint(Stroke.Points[0], size);
            AddCircle(previous, radius);

            for (var i = 1; i < Stroke.Points.Count; i++)
            {
                var current = ToControlPoint(Stroke.Points[i], size);
                var delta = current - previous;

                if (delta.LengthSquared() > float.Epsilon)
                {
                    var direction = Vector2.Normalize(delta);
                    var normal = new Vector2(-direction.Y, direction.X) * radius;

                    Vertices.Add(previous + normal);
                    Vertices.Add(previous - normal);
                    Vertices.Add(current + normal);
                    Vertices.Add(current + normal);
                    Vertices.Add(previous - normal);
                    Vertices.Add(current - normal);
                }

                AddCircle(current, radius);
                previous = current;
            }

            _builtPointCount = Stroke.Points.Count;
        }

        private void AddCircle(Vector2 center, float radius)
        {
            var segments = Math.Clamp((int)(radius * 2f), MinCapSegments, MaxCapSegments);
            var step = MathF.Tau / segments;
            var prev = new Vector2(1f, 0f) * radius;

            for (var i = 1; i <= segments; i++)
            {
                var angle = step * i;
                var next = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;

                Vertices.Add(center);
                Vertices.Add(center + prev);
                Vertices.Add(center + next);
                prev = next;
            }
        }
    }
}
