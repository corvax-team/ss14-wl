using Content.Server.Singularity.Components;
using Content.Server.Singularity.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;
using Robust.Shared.Spawners;

namespace Content.Server._WL.Chemistry.ReactionEffects;

public sealed partial class GravityWellReactionEffect : ReagentEffect
{
    [DataField("radialAcceleration")]
    public float BaseRadialAcceleration = 5f;

    [DataField("tangentialAcceleration")]
    public float BaseTangentialAcceleration = 5f;

    [DataField("scaled")]
    public bool Scaled = true;

    [DataField("pulsePeriod")]
    public float PulsePeriod = 0.01f;

    [DataField("radius")]
    public float Radius = 2.5f;

    [DataField("lifeTime")]
    public float LifeTime = 5f;

    public override void Effect(ReagentEffectArgs args)
    {
        var entityManager = args.EntityManager;
        var gravityWellSystem = entityManager.System<GravityWellSystem>();

        if (!entityManager.TryGetComponent<TransformComponent>(args.SolutionEntity, out var transformComp))
            return;

        var spawned = entityManager.SpawnEntity("AdminInstantEffectGravityWell", transformComp.Coordinates);

        if (!entityManager.TryGetComponent<GravityWellComponent>(spawned, out var gravityWellComp))
            return;

        gravityWellComp.BaseRadialAcceleration = Scaled ? BaseRadialAcceleration * MathF.Sqrt(args.Quantity.Float()) : BaseRadialAcceleration;
        gravityWellComp.BaseTangentialAcceleration = Scaled ? BaseTangentialAcceleration * MathF.Sqrt(args.Quantity.Float()) : BaseTangentialAcceleration;
        gravityWellComp.MaxRange = Radius;
        gravityWellComp.MinRange = 0.3f;
        gravityWellSystem.SetPulsePeriod(spawned, TimeSpan.FromSeconds(PulsePeriod), gravityWellComp);

        var timedDespawn = entityManager.EnsureComponent<TimedDespawnComponent>(spawned);
        timedDespawn.Lifetime = LifeTime;
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    => Loc.GetString("reagent-effect-guidebook-spawn-gravity-well",
        ("lifetime", LifeTime));
}
