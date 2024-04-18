using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Slimes;

/// <summary>
/// Conditions under which a slime can split or mutate.
/// </summary>
[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
[Serializable, NetSerializable]
public abstract partial class SlimeTransformationCondition
{
    public abstract bool Condition(SlimeTransformationConditionArgs args);

    public abstract SlimeTransformationCondition GetRandomCondition(IEntityManager entMan, IPrototypeManager protoMan, IRobustRandom random);

    public abstract string GetDescriptionString(IEntityManager entityManager, IPrototypeManager protoMan);
}
public readonly record struct SlimeTransformationConditionArgs(EntityUid Slime, IEntityManager EntityManager);
