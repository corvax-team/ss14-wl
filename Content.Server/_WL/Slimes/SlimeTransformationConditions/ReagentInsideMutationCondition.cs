using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Shared._WL.Slimes;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.Reagent;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
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

        if (!EntityManager.TryGetComponent<SolutionContainerManagerComponent>(args.Slime, out var solutionContComp))
            return false;

        if (!_solution.TryGetSolution(solutionContComp, "chemicals", out var slimeSolution))
            return false;

        return RequiredAll
            ? Thresholds.TrueForAll(reagent =>
            {
                var quantity = slimeSolution.Contents.Where(reag => reag.Reagent.Prototype == reagent.Reagent).FirstOrNull()?.Quantity;
                if (quantity == null)
                    return false;

                return quantity >= reagent.Min && quantity <= reagent.Max;
            })
            : Thresholds.Any(reagent =>
            {
                var quantity = slimeSolution.Contents.Where(reag => reag.Reagent.Prototype == reagent.Reagent).FirstOrNull()?.Quantity;
                if (quantity == null)
                    return false;

                return quantity >= reagent.Min && quantity <= reagent.Max;
            });
    }

    public override string GetDescriptionString(IEntityManager entityManager, IPrototypeManager protoMan)
            => Loc.GetString("slime-transformation-condition-reagent-inside",
                    ("reagents", string.Join(", ", Thresholds.Select(threshold => threshold.ToString())).TrimEnd() + ';'),
                    ("all", RequiredAll == true ? 1 : 0));
}

[DataDefinition]
[UsedImplicitly]
public sealed partial class ReagentInsideMutationConditionData
{
    [DataField("reagent", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<ReagentPrototype>))]
    public string Reagent;

    [DataField("min")]
    public float Min = 1f;

    [DataField("max")]
    public float Max = 250;

    public override string ToString()
    {
        return Loc.GetString("slime-transformation-condition-reagent",
            ("min", Min),
            ("max", Max),
            ("name", Reagent.ToString()));
    }
}
