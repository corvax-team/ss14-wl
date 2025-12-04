using Content.Shared._Offbrand.Wounds;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityConditions.Conditions.Body;

/// <summary>
/// Returns true if this entity's current mob state matches the condition's specified mob state.
/// </summary>
/// <inheritdoc cref="EntityConditionSystem{T, TCondition}"/>
public sealed partial class MobStateEntityConditionSystem : EntityConditionSystem<MobStateComponent, MobStateCondition>
{
    [Dependency] private readonly HealthRankingSystem _healthRanking = default!; //Offbrand

    protected override void Condition(Entity<MobStateComponent> entity, ref EntityConditionEvent<MobStateCondition> args)
    {
        if (entity.Comp.CurrentState == args.Condition.Mobstate)
            args.Result = true;
        // Begin Offbrand
        if (args.Condition.Mobstate == MobState.Critical)
        {
            if (_healthRanking.IsCritical(entity))
            {
                args.Result = true;
            }
        }
        // End Offbrand
    }
}

/// <inheritdoc cref="EntityCondition"/>
public sealed partial class MobStateCondition : EntityConditionBase<MobStateCondition>
{
    [DataField]
    public MobState Mobstate = MobState.Alive;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype) =>
        Loc.GetString("entity-condition-guidebook-mob-state-condition", ("state", Mobstate));
}
