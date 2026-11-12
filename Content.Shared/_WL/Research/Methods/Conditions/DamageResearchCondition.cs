using System.Numerics;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Methods;

public sealed partial class DamageResearchConditionSystem : ResearchConditionSystem<DamageableComponent, DamageResearchCondition>
{
    [Dependency] private DamageableSystem _damageable = default!;

    protected override void Condition(Entity<DamageableComponent> entity, ref ResearchConditionEvent<DamageResearchCondition> args)
    {
        var damage = _damageable.GetPositiveDamage(entity);

        if (!damage.Empty && args.Condition.DamageContainer is {} container)
        {
            foreach (var (type, value) in damage.DamageDict)
            {
                if (_damageable.SupportsType(container, type))
                        args.Result += value;
            }
        }
        else
        {
            args.Result = damage.GetTotal();
        }

        args.Points.PointsDict.Add(args.Condition.ExtrimalType, (args.Result - args.Condition.MinExtrimal) / (args.Condition.MaxDamage - args.Condition.MinExtrimal) / 100 * args.Condition.BaseValue);
    }
}

public sealed partial class DamageResearchCondition : ResearchConditionBase<DamageResearchCondition>
{
    [DataField]
    public FixedPoint4 MinExtrimal = 100;

    [DataField]
    public FixedPoint4 MaxDamage = 200;

    [DataField]
    public ProtoId<DamageContainerPrototype>? DamageContainer;
}
