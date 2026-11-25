using Content.Shared._WL.Particles;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Client.Graphics;
using Robust.Client.Timing;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Client._WL.Particles;

/// <summary>
/// Handles the particle onevent cases that (from as far as I am aware) cannot be replaced by <see cref="SpawnParticleEffect"/> and the trigger system.
/// <see cref="ParticleOnThrownComponent"/>, continuous emission while in flight, the active emitter must be tracked and explicitly stopped on landing.</item>
/// All other event particle needs should use <see cref="SpawnParticleEffect"/> in <c>EntityEffectOnTrigger</c> component instead of dedicated components here.
/// </summary>
public sealed partial class ParticleOnEventSystem : EntitySystem
{
    [Dependency] private ParticleSystem _particles = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IEyeManager _eye = default!;

    // Track emitters spawned by OnThrown so we can stop them when the entity lands
    private readonly Dictionary<EntityUid, ActiveEmitter> _thrownEmitters = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ParticleOnThrownComponent, ThrownEvent>(OnThrown);
        SubscribeLocalEvent<ParticleOnThrownComponent, LandEvent>(OnThrownLanded);
        SubscribeLocalEvent<ParticleOnThrownComponent, ComponentShutdown>(OnThrownShutdown);

        SubscribeLocalEvent<ParticleOnGunShotProjectileComponent, GunShotEvent>(OnGunShot);
    }

    private void OnThrown(Entity<ParticleOnThrownComponent> ent, ref ThrownEvent args)
    {
        // Stop any existing emitter first to avoid orphaning it on a re-throw.
        StopThrownEmitter(ent.Owner);

        // Infinite-duration allowed: the emitter is stopped when the entity lands.
        var emitter = _particles.CreateParticle(ent.Comp.Effect, ent.Owner, ent.Comp.ColorOverride);
        if (emitter != null)
            _thrownEmitters[ent.Owner] = emitter;
    }

    private void OnThrownLanded(Entity<ParticleOnThrownComponent> ent, ref LandEvent args)
    {
        StopThrownEmitter(ent.Owner);
    }

    private void OnThrownShutdown(Entity<ParticleOnThrownComponent> ent, ref ComponentShutdown args)
    {
        StopThrownEmitter(ent.Owner);
    }

    private void StopThrownEmitter(EntityUid uid)
    {
        if (_thrownEmitters.Remove(uid, out var emitter))
            _particles.RemoveParticle(emitter);
    }

    private void OnGunShot(Entity<ParticleOnGunShotProjectileComponent> ent, ref GunShotEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        for (int i = 0; i < args.Ammo.Count; i++)
        {
            if (args.Direction == null)
                continue;
            var overrideParticles = new ParticleRuntimeOverrides
            {
                EmitAngle = _particles.GetEmitAngle(args.Direction.Value),
            };

            _particles.CreateParticle(ent.Comp.Effect, ent, ent.Comp.ColorOverride, overrides: overrideParticles);
        }
    }
}
