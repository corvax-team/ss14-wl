using Content.Shared._WL.Photo.Filters;
using Content.Shared.Coordinates;
using Content.Shared.Ghost;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server._WL.Photo.Filter;

public sealed partial class PhotoGhostFilterSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<PhotoGhostFilterComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (now < comp.NextUpdateTime)
                continue;

            comp.ViewedGhosts.Clear();
            comp.NextUpdateTime = now + comp.UpdateTime;

            var entities = _lookup.GetEntitiesInRange(uid, comp.ViewRange);
            foreach (var entity in entities)
            {
                if (HasComp<GhostComponent>(entity))
                    comp.ViewedGhosts.Add(_transform.ToWorldPosition(entity.ToCoordinates()));
            }
            Dirty(uid, comp);
        }
    }
}
