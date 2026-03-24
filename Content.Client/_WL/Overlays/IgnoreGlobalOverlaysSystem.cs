using Content.Shared._WL.Overlays;
using Robust.Shared.Graphics;

namespace Content.Client._WL.Overlays;

public sealed partial class IgnoreGlobalOverlaysSystem : EntitySystem
{
    public Dictionary<IEye, EntityUid> IgnoreEyes = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IgnoreGlobalOverlaysComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<IgnoreGlobalOverlaysComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnComponentInit(EntityUid uid, IgnoreGlobalOverlaysComponent component, ComponentInit args)
    {
        if (EntityManager.TryGetComponent<EyeComponent>(uid, out var eye) && !IgnoreEyes.ContainsKey(eye.Eye))
            IgnoreEyes.Add(eye.Eye, uid);
    }

    private void OnComponentShutdown(EntityUid uid, IgnoreGlobalOverlaysComponent component, ComponentShutdown args)
    {
        if (EntityManager.TryGetComponent<EyeComponent>(uid, out var eye) && IgnoreEyes.ContainsKey(eye.Eye))
            IgnoreEyes.Remove(eye.Eye);
    }

    public bool CheckIgnore(IEye? eye)
    {
        if (eye == null)
            return false;

        return IgnoreEyes.ContainsKey(eye);
    }
}
