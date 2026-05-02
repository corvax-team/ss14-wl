using Content.Client._WL.Photo;
using Content.Client.GameTicking.Managers;
using Content.Shared._WL.Photo.Filters;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Client._WL.Overlays;
public sealed partial class InfoCameraOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IResourceCache _cache = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    private readonly PhotoSystem _photo;
    private readonly ClientGameTicker _gameTicker;

    private readonly VectorFont _baseFont;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    public InfoCameraOverlay()
    {
        IoCManager.InjectDependencies(this);
        ZIndex = 9;

        _photo = _entManager.System<PhotoSystem>();
        _gameTicker = _entManager.System<ClientGameTicker>();

        _baseFont = new VectorFont(_cache.GetResource<FontResource>("/Fonts/NotoSansDisplay/NotoSansDisplay-Regular.ttf"), 20);
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (args.Viewport.Eye == null || !_photo.ActiveEyes.TryGetValue(args.Viewport.Eye, out var uid))
            return false;

        return _entManager.HasComponent<PhotoInfoFilterComponent>(uid);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        if (args.Viewport.Eye == null || !_photo.ActiveEyes.TryGetValue(args.Viewport.Eye, out var uid) ||
            !_entManager.TryGetComponent<PhotoInfoFilterComponent>(uid, out var filter))
            return;

        var handle = args.WorldHandle;

        var worldMatrix = Matrix3Helpers.CreateTranslation(-args.WorldBounds.TopLeft);

        Angle angle = args.Viewport.Eye?.Rotation ?? Angle.Zero;
        Vector2 zoom = (args.Viewport.Eye?.Zoom ?? Vector2.One);
        handle.SetTransform(args.WorldBounds.BottomLeft, -angle, zoom);

        DrawSizeBar(handle, args.WorldBounds, zoom);

        var stationTime = _timing.CurTime.Subtract(_gameTicker.RoundStartTimeSpan);
        var timeString = stationTime.ToString("hh\\:mm\\:ss");

        handle.SetTransform(args.WorldBounds.BottomLeft, -angle, zoom / 80f);

        DrawString("1m", new Vector2(30f, 120f), handle);
        DrawString(timeString, new Vector2(30f, 220f), handle);
    }

    private void DrawString(string text, Vector2 pos, DrawingHandleWorld handle)
    {
        float offset = 0;
        foreach (var rune in text.EnumerateRunes())
            offset += _baseFont.DrawChar(handle, rune, pos + new Vector2(offset, 0f), 2f, Color.White);
    }

    private void DrawSizeBar(DrawingHandleWorld handle, Box2Rotated worldBox, Vector2 zoom)
    {
        const float thickness = 0.1f;
        handle.DrawRect(Box2.FromDimensions(0.4f, 0.5f, 6f, thickness), Color.White);

        float screenWidth = worldBox.Box.Width / zoom.X;

        float pos = 0f;
        float cellSize = screenWidth / worldBox.Box.Width;
        while (pos < 6f)
        {
            handle.DrawRect(Box2.FromDimensions(0.4f + pos, 0.2f + thickness / 2f, thickness, 0.6f), Color.White);
            pos += cellSize;
        }
    }
}

