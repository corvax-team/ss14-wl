using Content.Shared.Gravity;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Robust.Shared.Map;

namespace Content.Shared.Movement.Systems;

public sealed partial class SharedSwimSystem : EntitySystem
{
    [Dependency] private EntityQuery<SwimmerComponent> _swimmerQuery = default!;
    [Dependency] private EntityQuery<SwimmableMapComponent> _swimmableMapQuery = default!;
    [Dependency] private MovementSpeedModifierSystem _speedModifier = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SwimmerComponent, IsWeightlessEvent>(OnSwimmerIsWeightless);
        SubscribeLocalEvent<SwimmerComponent, CanWeightlessMoveEvent>(OnSwimmerCanWeightlessMove);
        SubscribeLocalEvent<SwimmerComponent, RefreshWeightlessModifiersEvent>(OnSwimmerRefreshWeightless);
    }

    private void OnSwimmerIsWeightless(Entity<SwimmerComponent> entity, ref IsWeightlessEvent args)
    {
        if (args.Handled || !CanSwim(entity.Owner))
            return;

        args.IsWeightless = true;
        args.Handled = true;

        _speedModifier.RefreshWeightlessModifiers(entity.Owner);
    }

    private void OnSwimmerCanWeightlessMove(Entity<SwimmerComponent> entity, ref CanWeightlessMoveEvent args)
    {
        if (CanSwim(entity.Owner))
            args.CanMove = true;
    }

    private void OnSwimmerRefreshWeightless(Entity<SwimmerComponent> entity, ref RefreshWeightlessModifiersEvent args)
    {
        if (!CanSwim(entity.Owner))
            return;

        args.ModifyAcceleration(entity.Comp.SwimAccelerationModifier, entity.Comp.SwimSpeedModifier);
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

        return _swimmableMapQuery.HasComp(mapUid.Value);
    }
}
