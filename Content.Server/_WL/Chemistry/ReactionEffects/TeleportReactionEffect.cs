using Content.Shared.Chemistry.Reagent;
using Content.Shared.Whitelist;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;
using System.Numerics;

namespace Content.Server._WL.Chemistry.ReactionEffects;

public sealed partial class TeleportReactionEffect : ReagentEffect
{
    private const string TeleportParticlePrototype = "EffectEmpDisabled";

    [DataField("searchRadius")]
    public float SearchRadius = 0.5f;

    [DataField("teleportRadius")]
    public float TeleportRadius = 1f;

    [DataField("whitelist")]
    public EntityWhitelist Whitelist;

    [DataField("scaled")]
    public bool Scaled = true;

    public override void Effect(ReagentEffectArgs args)
    {
        var _entityManager = args.EntityManager;
        var _lookup = _entityManager.System<EntityLookupSystem>();
        var _transform = _entityManager.System<TransformSystem>();
        var _random = IoCManager.Resolve<IRobustRandom>();

        var searchRadius = Scaled ? SearchRadius * MathF.Sqrt((float) args.Quantity) : SearchRadius;

        var entities = _lookup.GetEntitiesInRange(args.SolutionEntity, searchRadius, LookupFlags.Dynamic)
            .Where(entity => Whitelist.IsValid(entity));

        var teleportRadius = Scaled ? TeleportRadius * MathF.Sqrt((float) args.Quantity) : TeleportRadius;
        foreach (var entity in entities)
        {
            var entityCoords = _transform.GetMoverCoordinates(entity);
            entityCoords.Position.Deconstruct(out var x, out var y);

            var newX = _random.NextFloat(x - teleportRadius, x + teleportRadius);
            var magnitudeY = MathF.Sqrt(MathF.Pow(teleportRadius, 2f) - MathF.Pow(x - newX, 2f));
            var newY = _random.NextFloat(-magnitudeY, magnitudeY);

            var newCoords = entityCoords.WithPosition(new Vector2(newX, newY));

            _entityManager.SpawnEntity(TeleportParticlePrototype, entityCoords);
            _transform.SetCoordinates(entity, newCoords);
            _entityManager.SpawnEntity(TeleportParticlePrototype, newCoords);
        }
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    => Loc.GetString("reagent-effect-guidebook-teleport",
        ("chance", Probability));
}
