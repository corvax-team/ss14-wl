using Content.Client._WL.Photo;
using Content.Shared._WL.Photo.Filters;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Client._WL.Overlays;
public sealed partial class GhostCameraOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> Shader = "CameraGhost";
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    private readonly PhotoSystem _photo;
    private readonly SpriteSystem _sprite;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;
    private readonly ShaderInstance _shader;

    public GhostCameraOverlay()
    {
        IoCManager.InjectDependencies(this);
        _shader = _prototypeManager.Index(Shader).InstanceUnique();
        ZIndex = 9;

        _photo = _entManager.System<PhotoSystem>();
        _sprite = _entManager.System<SpriteSystem>();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (args.Viewport.Eye == null || !_photo.ActiveEyes.TryGetValue(args.Viewport.Eye, out var uid))
            return false;

        return _entManager.HasComponent<PhotoGhostFilterComponent>(uid);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        if (args.Viewport.Eye == null || !_photo.ActiveEyes.TryGetValue(args.Viewport.Eye, out var uid) ||
            !_entManager.TryGetComponent<PhotoGhostFilterComponent>(uid, out var filter))
            return;

        const float scale = 1f;
        var scaleMatrix = Matrix3Helpers.CreateScale(new Vector2(scale, scale));
        var rotationMatrix = Matrix3Helpers.CreateRotation(-(args.Viewport.Eye?.Rotation ?? Angle.Zero));

        var handle = args.WorldHandle;

        foreach (var worldPos in filter.ViewedGhosts)
        {
            var worldMatrix = Matrix3Helpers.CreateTranslation(worldPos);

            var scaledWorld = Matrix3x2.Multiply(scaleMatrix, worldMatrix);
            var matty = Matrix3x2.Multiply(rotationMatrix, scaledWorld);
            handle.SetTransform(matty);
            handle.DrawTexture(_sprite.Frame0(filter.Visual), new Vector2(-0.5f, -0.5f));
        }

        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _shader.SetParameter("SCALE", args.Viewport.Eye?.Scale.Length() ?? 1f);

        handle.SetTransform(Matrix3x2.Identity);
        handle.UseShader(_shader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
