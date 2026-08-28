using Content.Shared.Gravity;
using Content.Shared.Movement.Components;
using Content.Shared.Station.Components;
using Robust.Shared.Map;

namespace Content.Shared.Movement.Systems;

public sealed partial class SharedSwimSystem : EntitySystem
{
    [Dependency] private EntityQuery<SwimmerComponent> _swimmerQuery = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SwimmerComponent, IsWeightlessEvent>(OnSwimmerIsWeightless);
    }

    private void OnSwimmerIsWeightless(Entity<SwimmerComponent> entity, ref IsWeightlessEvent args)
    {
        if (args.Handled || !CanSwim(entity.Owner))
            return;

        args.IsWeightless = true;
        args.Handled = true;
    }

    public bool CanSwim(EntityUid uid, TransformComponent? xform = null)
    {
        if (!_swimmerQuery.HasComp(uid))
            return false;

        xform ??= Transform(uid);

        if (xform.GridUid != null)
            return false;

        var mapUid = xform.MapUid;
        if (mapUid == null || mapUid == EntityUid.Invalid || xform.MapID == MapId.Nullspace)
            return false;

        var stations = EntityQueryEnumerator<PlanetaryStationComponent, TransformComponent>();
        while (stations.MoveNext(out _, out var stationXform))
        {
            if (stationXform.MapUid == mapUid)
                return true;
        }

        return false;
    }
}
