using Content.Shared._WL.Research.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Research.Methods;

public sealed partial class SharedResearchConditionsSystem : EntitySystem, IResearchConditionRaiser
{
    // ResearchValue, dict of points
    public (FixedPoint2, ResearchPointsSpecifier) GetCondition<T>(EntityUid target, T condition, EntityUid? sourceEnt = null) where T : ResearchCondition
    {
        return condition.RaiseEvent(target, this, sourceEnt);
    }

    public (FixedPoint2, ResearchPointsSpecifier) RaiseConditionEvent<T>(EntityUid target, T effect, EntityUid? sourceEnt) where T : ResearchConditionBase<T>
    {
        var conditionEv = new ResearchConditionEvent<T>(effect, sourceEnt);
        RaiseLocalEvent(target, ref conditionEv);
        return (conditionEv.Result, conditionEv.Points);
    }
}

public abstract partial class ResearchConditionSystem<T, TCon> : EntitySystem where T : Component where TCon : ResearchConditionBase<TCon>
{
    public override void Initialize()
    {
        SubscribeLocalEvent<T, ResearchConditionEvent<TCon>>(Condition);
    }

    protected abstract void Condition(Entity<T> entity, ref ResearchConditionEvent<TCon> args);
}

public interface IResearchConditionRaiser
{
    (FixedPoint2, ResearchPointsSpecifier) RaiseConditionEvent<T>(EntityUid target, T condition, EntityUid? sourceEnt) where T : ResearchConditionBase<T>;
}

public abstract partial class ResearchConditionBase<T> : ResearchCondition where T : ResearchConditionBase<T>
{
    public override (FixedPoint2, ResearchPointsSpecifier) RaiseEvent(EntityUid target, IResearchConditionRaiser raiser, EntityUid? sourceEnt)
    {
        if (this is not T type)
            return (0, new ResearchPointsSpecifier());

        return raiser.RaiseConditionEvent(target, type, sourceEnt);
    }
}

[ImplicitDataDefinitionForInheritors]
public abstract partial class ResearchCondition
{
    [DataField]
    public ProtoId<ResearchPointsTypePrototype> BaseType;

    [DataField]
    public ProtoId<ResearchPointsTypePrototype> ExtrimalType = "Experimental";

    public abstract (FixedPoint2, ResearchPointsSpecifier) RaiseEvent(EntityUid target, IResearchConditionRaiser raiser, EntityUid? sourceEnt);
}

[ByRefEvent]
[DataRecord]
public partial record struct ResearchConditionEvent<T>(T Condition, EntityUid? SourceEnt) where T : ResearchConditionBase<T>
{
    [DataField]
    public FixedPoint2 Result;

    [DataField]
    public ResearchPointsSpecifier Points = new();

    public readonly T Condition = Condition;

    public readonly EntityUid? SourceEnt = SourceEnt;
}
