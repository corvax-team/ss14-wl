using Content.Shared.Pinpointer;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Collections;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using System.Numerics;
using Content.Shared.Atmos;
using System.Linq;
using Robust.Shared.Utility;

namespace Content.Client._WL.Silicon;

[Virtual]
public partial class AiNavMapOverlay
{
    [Dependency] public IEntityManager _entManager = default!;
    [Dependency] public IResourceCache _cache = default!;

    public EntityUid? Owner;
    public EntityUid? MapUid;

    // Tracked data
    public Dictionary<EntityCoordinates, (bool Visible, Color Color)> TrackedCoordinates = new();
    public Dictionary<NetEntity, NavMapBlip> TrackedEntities = new(); //TODO add tracked entities blip

    private readonly List<(Vector2, Vector2)> _tileLines = new();
    private readonly List<(Vector2, Vector2)> _tileRects = new();
    private readonly List<(Vector2[], Color, Box2)> _tilePolygons = new();

    // Default colors
    private readonly Color _wallColor = new(102, 164, 217);
    private readonly Color _tileColor = new(30, 57, 67);

    // Constants
    private const float UpdateTime = 1.0f;
    private const float FullWallInstep = 0.165f;
    private const float ThinWallThickness = 0.165f;
    private const float ThinDoorThickness = 0.30f;

    // Local variables
    private float _updateTimer = 1.0f;
    private readonly Dictionary<Color, Color> _srgbLookUp = new();
    private readonly List<Vector2> _tileLinesToDraw = [];
    private readonly List<Vector2> _tileRectsToDraw = [];

    private readonly Dictionary<Vector2i, Vector2i> _horizLines = new();
    private readonly Dictionary<Vector2i, Vector2i> _horizLinesReversed = new();
    private readonly Dictionary<Vector2i, Vector2i> _vertLines = new();
    private readonly Dictionary<Vector2i, Vector2i> _vertLinesReversed = new();

 // These lines need to be offset by one pixel inward
    private readonly Dictionary<Vector2i, Vector2i> _horizLinesSouth = new();
    private readonly Dictionary<Vector2i, Vector2i> _horizLinesSouthReversed = new();
    private readonly Dictionary<Vector2i, Vector2i> _vertLinesWest = new();
    private readonly Dictionary<Vector2i, Vector2i> _vertLinesWestReversed = new();

    // Components
    private NavMapComponent? _navMap;
    private MapGridComponent? _grid;
    private FixturesComponent? _fixtures;

    public AiNavMapOverlay()
    {
        IoCManager.InjectDependencies(this);
        ForceNavMapUpdate();
    }

    public void ForceNavMapUpdate()
    {
        _entManager.TryGetComponent(MapUid, out _navMap);
        _entManager.TryGetComponent(MapUid, out _grid);
        _entManager.TryGetComponent(MapUid, out _fixtures);

        UpdateNavMap();
    }

    private void UpdateNavMap()
    {
        // Clear stale values
        _tilePolygons.Clear();
        _tileLines.Clear();
        _tileRects.Clear();

        UpdateNavMapFloorTiles();
        UpdateNavMapWallLines();
        UpdateNavMapAirlocks();
    }

    private void UpdateNavMapFloorTiles()
    {
        if (_fixtures == null)
            return;

        var verts = new Vector2[8];

        foreach (var fixture in _fixtures.Fixtures.Values)
        {
            if (fixture.Shape is not PolygonShape poly)
                continue;

            for (var i = 0; i < poly.VertexCount; i++)
            {
                var vert = poly.Vertices[i];
                verts[i] = new Vector2(MathF.Round(vert.X), MathF.Round(vert.Y));
            }

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            for (int i = 0; i < poly.VertexCount; i++)
            {
                if (verts[i].X < minX) minX = verts[i].X;
                if (verts[i].Y < minY) minY = verts[i].Y;
                if (verts[i].X > maxX) maxX = verts[i].X;
                if (verts[i].Y > maxY) maxY = verts[i].Y;
            }
            var polyBounds = Box2.FromTwoPoints(new Vector2(minX, minY), new Vector2(maxX, maxY));
            _tilePolygons.Add((verts[..poly.VertexCount], _tileColor, polyBounds));
        }
    }

    private void UpdateNavMapWallLines()
    {
        if (_navMap == null || _grid == null)
            return;

        // We'll use the following dictionaries to combine collinear wall lines
        _horizLines.Clear();
        _horizLinesReversed.Clear();
        _vertLines.Clear();
        _vertLinesReversed.Clear();

        _horizLinesSouth.Clear();
        _horizLinesSouthReversed.Clear();
        _vertLinesWest.Clear();
        _vertLinesWestReversed.Clear();

        const int southMask = (int)AtmosDirection.South << (int)NavMapChunkType.Wall;
        const int eastMask = (int)AtmosDirection.East << (int)NavMapChunkType.Wall;
        const int westMask = (int)AtmosDirection.West << (int)NavMapChunkType.Wall;
        const int northMask = (int)AtmosDirection.North << (int)NavMapChunkType.Wall;

        foreach (var (chunkOrigin, chunk) in _navMap.Chunks)
        {
            for (var i = 0; i < SharedNavMapSystem.ArraySize; i++)
            {
                var tileData = chunk.TileData[i] & SharedNavMapSystem.WallMask;
                if (tileData == 0)
                    continue;

                tileData >>= (int)NavMapChunkType.Wall;

                var relativeTile = SharedNavMapSystem.GetTileFromIndex(i);
                var tile = (chunk.Origin * SharedNavMapSystem.ChunkSize + relativeTile) * _grid.TileSize;

                if (tileData != SharedNavMapSystem.AllDirMask)
                {
                    AddRectForThinWall(tileData, tile);
                    continue;
                }

                tile = tile with { Y = -tile.Y };
                NavMapChunk? neighborChunk;

                // North edge
                var neighborData = 0;
                if (relativeTile.Y != SharedNavMapSystem.ChunkSize - 1)
                    neighborData = chunk.TileData[i + 1];
                else if (_navMap.Chunks.TryGetValue(chunkOrigin + Vector2i.Up, out neighborChunk))
                    neighborData = neighborChunk.TileData[i + 1 - SharedNavMapSystem.ChunkSize];

                if ((neighborData & southMask) == 0)
                {
                    AddOrUpdateNavMapLine(tile + new Vector2i(0, -_grid.TileSize),
                        tile + new Vector2i(_grid.TileSize, -_grid.TileSize),
                        _horizLines,
                        _horizLinesReversed);
                }

                // East edge
                neighborData = 0;
                if (relativeTile.X != SharedNavMapSystem.ChunkSize - 1)
                    neighborData = chunk.TileData[i + SharedNavMapSystem.ChunkSize];
                else if (_navMap.Chunks.TryGetValue(chunkOrigin + Vector2i.Right, out neighborChunk))
                {
                    neighborData =
                        neighborChunk.TileData[i + SharedNavMapSystem.ChunkSize - SharedNavMapSystem.ArraySize];
                }

                if ((neighborData & westMask) == 0)
                {
                    AddOrUpdateNavMapLine(tile + new Vector2i(_grid.TileSize, -_grid.TileSize),
                        tile + new Vector2i(_grid.TileSize, 0),
                        _vertLines,
                        _vertLinesReversed);
                }

                // South edge
                neighborData = 0;
                if (relativeTile.Y != 0)
                    neighborData = chunk.TileData[i - 1];
                else if (_navMap.Chunks.TryGetValue(chunkOrigin + Vector2i.Down, out neighborChunk))
                    neighborData = neighborChunk.TileData[i - 1 + SharedNavMapSystem.ChunkSize];

                if ((neighborData & northMask) == 0)
                {
                    AddOrUpdateNavMapLine(tile,
                        tile + new Vector2i(_grid.TileSize, 0),
                        _horizLinesSouth,
                        _horizLinesSouthReversed);
                }

                // West edge
                neighborData = 0;
                if (relativeTile.X != 0)
                    neighborData = chunk.TileData[i - SharedNavMapSystem.ChunkSize];
                else if (_navMap.Chunks.TryGetValue(chunkOrigin + Vector2i.Left, out neighborChunk))
                {
                    neighborData =
                        neighborChunk.TileData[i - SharedNavMapSystem.ChunkSize + SharedNavMapSystem.ArraySize];
                }

                if ((neighborData & eastMask) == 0)
                {
                    AddOrUpdateNavMapLine(tile + new Vector2i(0, -_grid.TileSize),
                        tile,
                        _vertLinesWest,
                        _vertLinesWestReversed);
                }

                // Add a diagonal line for interiors. Unless there are a lot of double walls, there is no point combining these
                _tileLines.Add((tile + new Vector2(1 / 32f, -_grid.TileSize), tile + new Vector2(_grid.TileSize, -1 / 32f)));
            }
        }

        // Record the combined lines
        foreach (var (origin, terminal) in _horizLines)
        {
            var offset = new Vector2(1 / 32f, 0);
            _tileLines.Add((origin + offset, terminal));
        }

        foreach (var (origin, terminal) in _vertLines)
        {
            var offset = new Vector2(0, -1 / 32f);
            _tileLines.Add((origin , terminal + offset));
        }

        foreach (var (origin, terminal) in _horizLinesSouth)
        {
            var offset = new Vector2(0, -1 / 32f);
            _tileLines.Add((origin + offset + new Vector2(1 / 32f, 0), terminal + offset));
        }

        foreach (var (origin, terminal) in _vertLinesWest)
        {
            var offset = new Vector2(1 / 32f, 0);
            _tileLines.Add((origin + offset, terminal + offset + new Vector2(0,-1 / 32f)));
        }
    }

    private void UpdateNavMapAirlocks()
    {
        if (_navMap == null || _grid == null)
            return;

        foreach (var chunk in _navMap.Chunks.Values)
        {
            for (var i = 0; i < SharedNavMapSystem.ArraySize; i++)
            {
                var tileData = chunk.TileData[i] & SharedNavMapSystem.AirlockMask;
                if (tileData == 0)
                    continue;

                tileData >>= (int)NavMapChunkType.Airlock;

                var relative = SharedNavMapSystem.GetTileFromIndex(i);
                var tile = (chunk.Origin * SharedNavMapSystem.ChunkSize + relative) * _grid.TileSize;

                // If the edges of an airlock tile are not all occupied, draw a thin airlock for each edge
                if (tileData != SharedNavMapSystem.AllDirMask)
                {
                    AddRectForThinAirlock(tileData, tile);
                    continue;
                }

                // Otherwise add a single full tile airlock
                _tileRects.Add((new Vector2(tile.X + FullWallInstep, -tile.Y - FullWallInstep),
                    new Vector2(tile.X - FullWallInstep + 1f, -tile.Y + FullWallInstep - 1)));

                _tileLines.Add((new Vector2(tile.X + 0.5f, -tile.Y - FullWallInstep),
                    new Vector2(tile.X + 0.5f, -tile.Y + FullWallInstep - 1)));
            }
        }
    }

    private void AddRectForThinWall(int tileData, Vector2i tile)
    {
        var leftTop = new Vector2(-0.5f, 0.5f - ThinWallThickness);
        var rightBottom = new Vector2(0.5f, 0.5f);

        for (var i = 0; i < SharedNavMapSystem.Directions; i++)
        {
            var dirMask = 1 << i;
            if ((tileData & dirMask) == 0)
                continue;

            var tilePosition = new Vector2(tile.X + 0.5f, -tile.Y - 0.5f);

            // TODO NAVMAP
            // Consider using faster rotation operations, given that these are always 90 degree increments
            var angle = -((AtmosDirection)dirMask).ToAngle();
            _tileRects.Add((angle.RotateVec(leftTop) + tilePosition, angle.RotateVec(rightBottom) + tilePosition));
        }
    }

    private void AddRectForThinAirlock(int tileData, Vector2i tile)
    {
        var leftTop = new Vector2(-0.5f + FullWallInstep, 0.5f - FullWallInstep - ThinDoorThickness);
        var rightBottom = new Vector2(0.5f - FullWallInstep, 0.5f - FullWallInstep);
        var centreTop = new Vector2(0f, 0.5f - FullWallInstep - ThinDoorThickness);
        var centreBottom = new Vector2(0f, 0.5f - FullWallInstep);

        for (var i = 0; i < SharedNavMapSystem.Directions; i++)
        {
            var dirMask = 1 << i;
            if ((tileData & dirMask) == 0)
                continue;

            var tilePosition = new Vector2(tile.X + 0.5f, -tile.Y - 0.5f);
            var angle = -((AtmosDirection)dirMask).ToAngle();
            _tileRects.Add((angle.RotateVec(leftTop) + tilePosition, angle.RotateVec(rightBottom) + tilePosition));
            _tileLines.Add((angle.RotateVec(centreTop) + tilePosition, angle.RotateVec(centreBottom) + tilePosition));
        }
    }

    private void AddOrUpdateNavMapLine(
        Vector2i origin,
        Vector2i terminus,
        Dictionary<Vector2i, Vector2i> lookup,
        Dictionary<Vector2i, Vector2i> lookupReversed)
    {
        Vector2i foundTermius;
        Vector2i foundOrigin;

        if (origin == terminus)
            return;

        // Does our new line end at the beginning of an existing line?
        if (lookup.Remove(terminus, out foundTermius))
        {
            DebugTools.Assert(lookupReversed[foundTermius] == terminus);

            // Does our new line start at the end of an existing line?
            if (lookupReversed.Remove(origin, out foundOrigin))
            {
                // Our new line just connects two existing lines
                DebugTools.Assert(lookup[foundOrigin] == origin);
                lookup[foundOrigin] = foundTermius;
                lookupReversed[foundTermius] = foundOrigin;
            }
            else
            {
                // Our new line precedes an existing line, extending it further to the left
                lookup[origin] = foundTermius;
                lookupReversed[foundTermius] = origin;
            }

            return;
        }

        // Does our new line start at the end of an existing line?
        if (lookupReversed.Remove(origin, out foundOrigin))
        {
            // Our new line just extends an existing line further to the right
            DebugTools.Assert(lookup[foundOrigin] == origin);
            lookup[foundOrigin] = terminus;
            lookupReversed[terminus] = foundOrigin;
            return;
        }

        // Completely disconnected line segment.
        lookup.Add(origin, terminus);
        lookupReversed.Add(terminus, origin);
    }

    public void AiFrameUpdate(float seconds, EntityUid? newMapUid)
    {
        if (MapUid != newMapUid)
        {
            MapUid = newMapUid;
            ForceNavMapUpdate();
        }
        else
        {
            // Update the timer
            _updateTimer += seconds;
            if (_updateTimer >= UpdateTime)
            {
                _updateTimer -= UpdateTime;
                UpdateNavMap();
            }
        }
    }
    public void Draw(DrawingHandleWorld handle, Box2 aabb)
    {
        if (!_srgbLookUp.TryGetValue(_wallColor, out var wallsRgb))
        {
            wallsRgb = Color.ToSrgb(_wallColor);
            _srgbLookUp[_wallColor] = wallsRgb;
        }

        // Draw floor tiles
        if (_tilePolygons.Count != 0)
        {
            foreach (var (polygonVerts, polygonColor, polyBounds) in _tilePolygons)
            {
                if (!aabb.Intersects(polyBounds))
                    continue;
                handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, polygonVerts.AsSpan()[..], polygonColor);
            }
        }

        // Draw map lines
        if (_tileLines.Count != 0)
        {
            _tileLinesToDraw.Clear();
            _tileLinesToDraw.EnsureCapacity(_tileLines.Count * 2);

            foreach (var (o, t) in _tileLines)
            {
                var origin = o with { Y = -o.Y };
                var terminus = t with { Y = -t.Y };
                if (!aabb.Intersects(Box2.FromTwoPoints(origin, terminus)))
                    continue;
                _tileLinesToDraw.Add(origin);
                _tileLinesToDraw.Add(terminus);
            }

            if (_tileLinesToDraw.Count > 0)
                handle.DrawPrimitives(DrawPrimitiveTopology.LineList, _tileLinesToDraw, wallsRgb);
        }

        // Draw map rects
        if (_tileRects.Count != 0)
        {
            _tileRectsToDraw.Clear();
            _tileRectsToDraw.EnsureCapacity(_tileRects.Count * 8);

            foreach (var (lt, rb) in _tileRects)
            {
                var leftTop = lt with { Y = -lt.Y };
                var rightBottom = rb with { Y = -rb.Y };
                if (!aabb.Intersects(Box2.FromTwoPoints(leftTop, rightBottom)))
                    continue;
                var rightTop = new Vector2(rightBottom.X, leftTop.Y);
                var leftBottom = new Vector2(leftTop.X, rightBottom.Y);

                _tileRectsToDraw.Add(leftTop);
                _tileRectsToDraw.Add(rightTop);
                _tileRectsToDraw.Add(rightTop);
                _tileRectsToDraw.Add(rightBottom);
                _tileRectsToDraw.Add(rightBottom);
                _tileRectsToDraw.Add(leftBottom);
                _tileRectsToDraw.Add(leftBottom);
                _tileRectsToDraw.Add(leftTop);
            }

            if (_tileRectsToDraw.Count > 0)
                handle.DrawPrimitives(DrawPrimitiveTopology.LineList, _tileRectsToDraw, wallsRgb);
        }
    }
}

public struct NavMapBlip
{
    public EntityCoordinates Coordinates;
    public Texture Texture;
    public Color Color;
    public bool Blinks;
    public bool Selectable;
    public float Scale;

    public NavMapBlip(EntityCoordinates coordinates,
        Texture texture,
        Color color,
        bool blinks,
        bool selectable = true,
        float scale = 1f)
    {
        Coordinates = coordinates;
        Texture = texture;
        Color = color;
        Blinks = blinks;
        Selectable = selectable;
        Scale = scale;
    }
}
