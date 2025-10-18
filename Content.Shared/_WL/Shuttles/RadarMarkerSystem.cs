using Robust.Shared.GameStates;

namespace Content.Shared._WL.Shuttles;

public sealed class RadarMarkerSystem : EntitySystem
{
    [Dependency] private readonly SharedPvsOverrideSystem _pvs = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RadarMarkerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<RadarMarkerComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(EntityUid uid, RadarMarkerComponent component, ComponentStartup args)
    {
        _pvs.AddGlobalOverride(uid);
    }

    private void OnShutdown(EntityUid uid, RadarMarkerComponent component, ComponentShutdown args)
    {
        _pvs.RemoveGlobalOverride(uid);
    }
}
