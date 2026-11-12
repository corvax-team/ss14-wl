using System.Numerics;
using Content.Shared._WL.Damage;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Methods;

public sealed partial class DamagePerTimeResearchConditionSystem : ResearchConditionSystem<DamageStatisticsComponent, DamagePerTimeResearchCondition>
{
    [Dependency] private DamageableSystem _damageable = default!;

    protected override void Condition(Entity<DamageStatisticsComponent> entity, ref ResearchConditionEvent<DamagePerTimeResearchCondition> args)
    {
        var damage = entity.Comp.DamagePerTime;

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

        if (args.Result == 0)
            args.Active = false;

        args.Points.PointsDict.Add(args.Condition.ExtrimalType, FixedPoint4.Max((args.Result - args.Condition.MinExtrimal) / (args.Condition.MaxDamage - args.Condition.MinExtrimal) / 100 * args.Condition.BaseValue, 0));
    }
}

public sealed partial class DamagePerTimeResearchCondition : ResearchConditionBase<DamagePerTimeResearchCondition>
{
    [DataField]
    public FixedPoint4 MinExtrimal = 35;

    [DataField]
    public FixedPoint4 MaxDamage = 50;

    [DataField("container")]
    public ProtoId<DamageContainerPrototype>? DamageContainer;
}
