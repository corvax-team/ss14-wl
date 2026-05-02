using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Content.Client._WL.Overlays; //Corvax-WL-Changes

namespace Content.Client.Overlays;

public sealed partial class BlackAndWhiteOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> Shader = "GreyscaleFullscreen";

    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    [Dependency] private readonly IEntityManager _entManager = default!; //Corvax-WL-Changes
    private readonly IgnoreGlobalOverlaysSystem _ignore; //Corvax-WL-Changes

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;
    private readonly ShaderInstance _greyscaleShader;

    public BlackAndWhiteOverlay()
    {
        IoCManager.InjectDependencies(this);
        _greyscaleShader = _prototypeManager.Index(Shader).InstanceUnique();
        ZIndex = 10; // draw this over the DamageOverlay, RainbowOverlay etc.

        _ignore = _entManager.System<IgnoreGlobalOverlaysSystem>(); //Corvax-WL-Changes
    }

    //Corvax-WL-Changes-start
    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        return !_ignore.CheckIgnore(args.Viewport.Eye);
    }
    //Corvax-WL-Changes-end

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var handle = args.WorldHandle;
        _greyscaleShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        handle.UseShader(_greyscaleShader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
