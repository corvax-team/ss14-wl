using System.Numerics;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Methods;

public sealed partial class DamageResearchConditionSystem : ResearchConditionSystem<DamageableComponent, DamageResearchCondition>
{
    [Dependency] private DamageableSystem _damageable = default!;

    protected override void Condition(Entity<DamageableComponent> entity, ref ResearchConditionEvent<DamageResearchCondition> args)
    {
        var damage = _damageable.GetPositiveDamage(entity);

        if (!damage.Empty && (args.Condition.Types.Count != 0 || args.Condition.Groups.Count != 0))
        {
            foreach (var type in args.Condition.Types)
            {
                if (damage.DamageDict.TryGetValue(type, out var value))
                        args.Result += value;
            }

            foreach (var group in args.Condition.Groups)
            {
                if (damage.TryGetDamageInGroup(ProtoMan.Index(group), out var value))
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
    public float MinExtrimal = 100f;

    [DataField]
    public float MaxDamage = 200f;

    [DataField]
    public List<ProtoId<DamageTypePrototype>> Types = new();

    [DataField]
    public List<ProtoId<DamageGroupPrototype>> Groups = new();
}
