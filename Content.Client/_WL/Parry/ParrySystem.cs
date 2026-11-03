using Content.Shared._WL.Parry;
using Robust.Client.Graphics;

namespace Content.Client._WL.Parry;

public sealed partial class ParrySystem : SharedParrySystem
{
    [Dependency] private IOverlayManager _overlayManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlayManager.AddOverlay(new ParryOverlay());
    }

    public override void Shutdown()
    {
        _overlayManager.RemoveOverlay<ParryOverlay>();
        base.Shutdown();
    }
}
