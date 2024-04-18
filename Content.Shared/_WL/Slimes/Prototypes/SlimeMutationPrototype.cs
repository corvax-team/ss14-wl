using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using System.Linq;

namespace Content.Shared._WL.Slimes.Prototypes;

/// <summary>
/// Class containing the slime group and possible mutations for this group.
/// </summary>
[Serializable, NetSerializable]
[DataDefinition]
[Prototype("slimeMutation")]
public sealed partial class SlimeMutationPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The slime group. Check SlimeComponent.
    /// </summary>
    [DataField("slimeGroup", required: true)]
    public string SlimeGroup { get; private set; }


    [DataField("mutations", required: true)]
    public List<SlimeMutationPrototypeData> MutationsData { get; private set; } = new();

    /// <summary>
    /// Selects a random mutation based on the "weight" of the mutation.
    /// </summary>
    /// <param name="slime">For which slime the mutation is selected. It is necessary for <see cref="SlimeTransformationConditionArgs.Slime"/>.</param>
    /// <param name="random"></param>
    /// <param name="entMan"></param>
    /// <returns><see cref="EntityPrototype.ID"/></returns>
    /// <exception cref="InvalidOperationException">If the "weights" in the prototype are placed incorrectly.</exception>
    public string? GetRandomMutation(
        EntityUid slime,
        IRobustRandom? random = null,
        IEntityManager? entMan = null)
    {
        IoCManager.Resolve(ref random);
        IoCManager.Resolve(ref entMan);

        var args = new SlimeTransformationConditionArgs(slime, entMan);

        var picked = MutationsData.Where(data =>
        {
            var conditions = data.MutationConditions;

            if (conditions.Count == 0)
                return true;

            return data.RequiredAll
                ? conditions.TrueForAll(condition => condition.Condition(args))
                : conditions.Any(condition => condition.Condition(args));
        });

        if (!picked.Any())
            return null;

        var sum = picked.Sum(data => data.Weight);
        var accumulated = 0f;

        var rand = random.NextFloat() * sum;

        foreach (var mutation in picked)
        {
            accumulated += mutation.Weight;

            if (accumulated >= rand)
            {
                return mutation.Prototype;
            }
        }

        throw new InvalidOperationException($"Invalid weighted pick for slime mutation prototype for {SlimeGroup} slime group!");
    }
}

/// <summary>
/// Class that stores data on slime mutations for <see cref="SlimeMutationPrototype"/>.
/// </summary>
[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class SlimeMutationPrototypeData
{
    /// <summary>
    /// A prototype of a creature that will have to spawn during mutation.
    /// </summary>
    [DataField("proto", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string Prototype;

    /// <summary>
    /// The "weight" of the mutation. The lower the value, the less likely it is to fall out.
    /// </summary>
    [DataField("weight")]
    public float Weight = 1f;

    [NonSerialized]
    [DataField("conditions", serverOnly: true)]
    public List<SlimeTransformationCondition> MutationConditions = new();

    /// <summary>
    /// Do all the conditions have to be fulfilled or is one enough.
    /// </summary>
    [DataField("requiredAll")]
    public bool RequiredAll = true;
}
