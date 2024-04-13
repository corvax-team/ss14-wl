using Content.Server._WL.StatusEffects.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map;

namespace Content.Server._WL.StatusEffects.Systems;

public sealed partial class BluespaceTeleportSystem : EntitySystem
{
    [Dependency] private readonly TransformSystem _transform = default!;

    private const string BluespaceParticlePrototype = "EffectFlashBluespace";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BluespaceTeleportComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<BluespaceTeleportComponent, ComponentShutdown>(OnShutdown);
    }

    public void OnInit(EntityUid uid, BluespaceTeleportComponent comp, ComponentInit args)
    {
        var coords = Transform(uid).Coordinates;
        comp.Coordinates = coords;
    }

    public void OnShutdown(EntityUid uid, BluespaceTeleportComponent comp, ComponentShutdown args)
    {
        if (Deleted(uid))
            return;

        var transform = Transform(uid);
        if (transform.GridUid is null)
            return;

        EntityManager.SpawnEntity(BluespaceParticlePrototype, new EntityCoordinates(transform.GridUid.Value, transform.Coordinates.Position));

        _transform.SetCoordinates(uid, comp.Coordinates);

        EntityManager.SpawnEntity(BluespaceParticlePrototype, new EntityCoordinates(transform.GridUid.Value, comp.Coordinates.Position));
    }
}
