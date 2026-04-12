using Content.Client._WL.Photo;
using Content.Shared._WL.Photo.Filters;
using Content.Shared.Humanoid;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using System.Numerics;

namespace Content.Client._WL.Overlays;
public sealed partial class FaceCameraOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private readonly PhotoSystem _photo;
    private readonly SpriteSystem _sprite;
    private readonly TransformSystem _transform;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    public FaceCameraOverlay()
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

        return _entManager.HasComponent<PhotoFaceFilterComponent>(uid);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        if (args.Viewport.Eye == null || !_photo.ActiveEyes.TryGetValue(args.Viewport.Eye, out var uid) ||
            !_entManager.TryGetComponent<PhotoFaceFilterComponent>(uid, out var filter))
            return;

        if (filter.Visual == null)
            return;

        const float scale = 1f;
        var scaleMatrix = Matrix3Helpers.CreateScale(new Vector2(scale, scale));
        var rotationMatrix = Matrix3Helpers.CreateRotation(-(args.Viewport.Eye?.Rotation ?? Angle.Zero));

        var handle = args.WorldHandle;

        var query = _entManager.EntityQueryEnumerator<HumanoidAppearanceComponent>();
        while (query.MoveNext(out var ent, out _))
        {
            Vector2 deltaDir = (_transform.GetWorldRotation(ent) - _transform.GetWorldRotation(uid).Opposite()).ToVec();
            if (MathF.Abs((float)deltaDir.ToAngle().Degrees) > 45f)
                continue;

            Vector2 worldPos = _transform.GetWorldPosition(ent);

            var worldMatrix = Matrix3Helpers.CreateTranslation(worldPos);

            var scaledWorld = Matrix3x2.Multiply(scaleMatrix, worldMatrix);
            var matty = Matrix3x2.Multiply(rotationMatrix, scaledWorld);
            handle.SetTransform(matty);
            handle.DrawTexture(_sprite.Frame0(filter.Visual), new Vector2(-0.5f, -0.5f));
        }
    }
}

