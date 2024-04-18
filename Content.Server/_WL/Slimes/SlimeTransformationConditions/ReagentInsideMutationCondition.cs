using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Shared._WL.Slimes;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Server._WL.Slimes.SlimeTransformationConditions;

public sealed partial class ReagentInsideMutationCondition : SlimeTransformationCondition
{
    [DataField("thresholds", required: true)]
    public List<ReagentInsideMutationConditionData> Thresholds = new();

    [DataField("requiredAll")]
    public bool RequiredAll = true;

    public override bool Condition(SlimeTransformationConditionArgs args)
    {
        var EntityManager = args.EntityManager;
        var _solution = EntityManager.System<SolutionContainerSystem>();
        var _protoMan = IoCManager.Resolve<IPrototypeManager>();

        Logger.Debug("1");
        if (!EntityManager.TryGetComponent<SolutionContainerManagerComponent>(args.Slime, out var solutionContComp))
            return false;
        Logger.Debug("2");

        var slimeSolution = _solution.EnsureSolution((args.Slime, null), "chemicals");
        Logger.Debug("3");
        return RequiredAll
            ? Thresholds.TrueForAll(reagent =>
            {
                var quantity = slimeSolution.GetReagentQuantity(new(reagent.Reagent, null));
                if (quantity == 0 || quantity == FixedPoint2.Zero)
                    return false;

                return quantity >= reagent.Min && quantity <= reagent.Max;
            })
            : Thresholds.Any(reagent =>
            {
                var quantity = slimeSolution.GetReagentQuantity(new(reagent.Reagent, null));
                if (quantity == 0 || quantity == FixedPoint2.Zero)
                    return false;

                return quantity >= reagent.Min && quantity <= reagent.Max;
            });
    }

    public override SlimeTransformationCondition GetRandomCondition(IEntityManager entMan, IPrototypeManager protoMan, IRobustRandom random)
    {
        var reagents = protoMan.EnumeratePrototypes<ReagentPrototype>()
            .Where(x => !x.Abstract)
            .ToList();

        var reagentThresholds = new List<ReagentInsideMutationConditionData>();

        var min = random.NextFloat(0, 45);
        var max = random.NextFloat(min + 5, 50);
        reagentThresholds.Add(new ReagentInsideMutationConditionData()
        {
            Reagent = random.PickAndTake(reagents).ID,
            Min = min,
            Max = max
        });

        return new ReagentInsideMutationCondition()
        {
            RequiredAll = random.Prob(0.5f),
            Thresholds = reagentThresholds
        };

    }

    public override string GetDescriptionString(IEntityManager entityManager, IPrototypeManager protoMan)
            => Loc.GetString("slime-transformation-condition-reagent-inside",
                    ("reagents", string.Join(", ", Thresholds.Select(threshold => threshold.ToString(protoMan))).TrimEnd()),
                    ("all", RequiredAll == true ? 1 : 0));
}

[DataDefinition]
[UsedImplicitly]
public sealed partial class ReagentInsideMutationConditionData
{
    [DataField("reagent", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<ReagentPrototype>))]
    public string Reagent;

    [DataField("min")]
    public FixedPoint2 Min = 1;

    [DataField("max")]
    public FixedPoint2 Max = 50;

    public string ToString(IPrototypeManager protoMan)
    {
        return Loc.GetString("slime-transformation-condition-reagent",
            ("min", Min),
            ("max", Max),
            ("name", protoMan.Index<ReagentPrototype>(Reagent).LocalizedName));
    }
}
