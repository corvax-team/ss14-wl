using Content.Shared.Gravity;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Systems;
using Robust.Shared.Map;

namespace Content.Shared.Movement.Systems;

public sealed partial class SharedSwimSystem : EntitySystem
{
    [Dependency] private EntityQuery<SwimmerComponent> _swimmerQuery = default!;
    [Dependency] private EntityQuery<SwimmableMapComponent> _swimmableMapQuery = default!;
    [Dependency] private EntityQuery<TransformComponent> _xformQuery = default!;
    [Dependency] private MovementSpeedModifierSystem _speedModifier = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GravityAffectedComponent, IsWeightlessEvent>(OnIsWeightless);

        SubscribeLocalEvent<SwimmerComponent, WeightlessnessChangedEvent>(OnSwimmerWeightlessnessChanged);

        SubscribeLocalEvent<SwimmerComponent, CanWeightlessMoveEvent>(OnSwimmerCanWeightlessMove);
        SubscribeLocalEvent<SwimmerComponent, RefreshWeightlessModifiersEvent>(OnSwimmerRefreshWeightless);
    }

    private void OnIsWeightless(Entity<GravityAffectedComponent> entity, ref IsWeightlessEvent args)
    {
        if (args.IsWeightless)
            return;

        if (!_xformQuery.TryComp(entity.Owner, out var xform))
            return;

        if (!IsInWater(xform))
            return;

        args.IsWeightless = true;
    }

    private void OnSwimmerWeightlessnessChanged(Entity<SwimmerComponent> entity, ref WeightlessnessChangedEvent args)
    {
        if (!_xformQuery.TryComp(entity.Owner, out var xform) || !IsInWater(xform))
            return;

        _speedModifier.RefreshWeightlessModifiers(entity.Owner);
    }

    private void OnSwimmerCanWeightlessMove(Entity<SwimmerComponent> entity, ref CanWeightlessMoveEvent args)
    {
        if (_xformQuery.TryComp(entity.Owner, out var xform) && IsInWater(xform))
            args.CanMove = true;
    }

    private void OnSwimmerRefreshWeightless(Entity<SwimmerComponent> entity, ref RefreshWeightlessModifiersEvent args)
    {
        if (!_xformQuery.TryComp(entity.Owner, out var xform) || !IsInWater(xform))
            return;

        args.ModifyAcceleration(entity.Comp.SwimAccelerationModifier, entity.Comp.SwimSpeedModifier);
    }

    public bool IsInWater(TransformComponent xform)
    {
        if (xform.GridUid != null)
            return false;

        var mapUid = xform.MapUid;
        if (mapUid is not { Valid: true } || xform.MapID == MapId.Nullspace)
            return false;

        return _swimmableMapQuery.HasComp(mapUid);
    }

    public bool IsInWater(EntityUid uid)
    {
        return _xformQuery.TryComp(uid, out var xform) && IsInWater(xform);
    }

    public float? TryGetWaterResistance(TransformComponent xform)
    {
        if (xform.GridUid != null)
            return null;

        var mapUid = xform.MapUid;
        if (mapUid is not { Valid: true } || xform.MapID == MapId.Nullspace)
            return null;

        return _swimmableMapQuery.TryComp(mapUid, out var swimmableMap)
            ? swimmableMap.WaterResistance
            : null;
    }
}
