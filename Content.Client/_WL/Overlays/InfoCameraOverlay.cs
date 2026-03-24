using Content.Client._WL.Photo;
using Content.Shared._WL.Photo.Filters;
using Content.Shared.Humanoid;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace Content.Client._WL.Overlays;
public sealed partial class InfoCameraOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private readonly PhotoSystem _photo;
    private readonly SpriteSystem _sprite;
    private readonly TransformSystem _transform;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    public InfoCameraOverlay()
    {
        IoCManager.InjectDependencies(this);
        ZIndex = 9;

        _photo = _entManager.System<PhotoSystem>();
        _sprite = _entManager.System<SpriteSystem>();
        _transform = _entManager.System<TransformSystem>();
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

        const float scale = 1f;
        var scaleMatrix = Matrix3Helpers.CreateScale(new Vector2(scale, scale));
        var rotationMatrix = Matrix3Helpers.CreateRotation(-(args.Viewport.Eye?.Rotation ?? Angle.Zero));

        var handle = args.WorldHandle;

        handle.DrawRect(new Box2(32f, 32f, 42f, 96f), Color.White);
        //args.ScreenHandle.DrawString()
    }
}

