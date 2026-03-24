using Content.Client._WL.Photo;
using Content.Shared._WL.Photo.Filters;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client._WL.Overlays;
public sealed partial class ShaderCameraOverlay : Overlay
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    [Dependency] private readonly IEntityManager _entManager = default!;
    private readonly PhotoSystem _photo;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    public ShaderCameraOverlay()
    {
        IoCManager.InjectDependencies(this);
        ZIndex = 9;

        _photo = _entManager.System<PhotoSystem>();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (args.Viewport.Eye == null || !_photo.ActiveEyes.TryGetValue(args.Viewport.Eye, out var uid))
            return false;

        return _entManager.HasComponent<PhotoShaderFilterComponent>(uid);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        if (args.Viewport.Eye == null || !_photo.ActiveEyes.TryGetValue(args.Viewport.Eye, out var uid) ||
            !_entManager.TryGetComponent<PhotoShaderFilterComponent>(uid, out var filter))
            return;

        if (filter.Shader == null)
            return;

        ShaderInstance shader = _prototypeManager.Index((ProtoId<ShaderPrototype>)filter.Shader).InstanceUnique();

        var handle = args.WorldHandle;
        shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        handle.UseShader(shader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
