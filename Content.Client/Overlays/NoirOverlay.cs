using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Content.Client._WL.Overlays; //Corvax-WL-Changes

namespace Content.Client.Overlays;

public sealed partial class NoirOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> Shader = "Noir";

    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    [Dependency] private readonly IEntityManager _entManager = default!; //Corvax-WL-Changes
    private readonly IgnoreGlobalOverlaysSystem _ignore; //Corvax-WL-Changes

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;
    private readonly ShaderInstance _noirShader;

    public NoirOverlay()
    {
        IoCManager.InjectDependencies(this);
        _noirShader = _prototypeManager.Index(Shader).InstanceUnique();
        ZIndex = 9; // draw this over the DamageOverlay, RainbowOverlay etc, but before the black and white shader

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
        _noirShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        handle.UseShader(_noirShader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
