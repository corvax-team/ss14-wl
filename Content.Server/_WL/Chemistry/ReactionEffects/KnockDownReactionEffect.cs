using Content.Shared.Chemistry.Reagent;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Chemistry.ReactionEffects;

public sealed partial class KnockDownReactionEffect : ReagentEffect
{
    private const string KnockedDownStatusEffect = "KnockedDown";

    private const string StunStatusEffect = "Stun";

    [DataField("minTime")]
    public float MinTime = 0.5f;

    [DataField("maxTime")]
    public float MaxTime = 5f;

    [DataField("minRadius")]
    public float MinRadius = 0.5f;

    [DataField("maxRadius")]
    public float MaxRadius = 5f;

    [DataField("radiusScaled")]
    public bool RadiusScaled = true;

    [DataField("timeScaled")]
    public bool TimeScaled = true;

    public override void Effect(ReagentEffectArgs args)
    {
        var entityManager = args.EntityManager;
        var _entLookup = entityManager.System<EntityLookupSystem>();
        var _statusEffect = entityManager.System<StatusEffectsSystem>();

        var radius = Math.Clamp(RadiusScaled ? MinRadius * MathF.Sqrt((float) args.Quantity) : MinRadius, MinRadius, MaxRadius);

        var entities = _entLookup.GetEntitiesInRange(args.SolutionEntity, radius, LookupFlags.Dynamic);

        var time = Math.Clamp(TimeScaled ? MinTime * MathF.Sqrt((float) args.Quantity) : MinTime, MinTime, MaxTime);
        foreach (var entity in entities)
        {
            if (_statusEffect.HasStatusEffect(entity, KnockedDownStatusEffect) || _statusEffect.HasStatusEffect(entity, StunStatusEffect))
                continue;

            _statusEffect.TryAddStatusEffect<StunnedComponent>(entity, StunStatusEffect, TimeSpan.FromSeconds(time), true);
            _statusEffect.TryAddStatusEffect<KnockedDownComponent>(entity, KnockedDownStatusEffect, TimeSpan.FromSeconds(time), true);
        }
    }
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-knock-down",
            ("chance", Probability));
}

