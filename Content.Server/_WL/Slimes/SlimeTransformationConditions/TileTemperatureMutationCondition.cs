using Content.Server.Atmos.EntitySystems;
using Content.Shared._WL.Slimes;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Server._WL.Slimes.SlimeTransformationConditions;

public sealed partial class TileTemperatureMutationCondition : SlimeTransformationCondition
{
    [DataField]
    public Gas? Gas = null;

    [DataField("min")]
    public FixedPoint2? MinTemperature = null;

    [DataField("max")]
    public FixedPoint2? MaxTemperature = null;

    public override bool Condition(SlimeTransformationConditionArgs args)
    {
        var entityManager = args.EntityManager;
        var atmosSystem = entityManager.System<AtmosphereSystem>();

        if (!entityManager.TryGetComponent<TransformComponent>(args.Slime, out var transform))
            return false;

        var gasMixture = atmosSystem.GetTileMixture((args.Slime, transform));

        if (gasMixture == null)
            return false;

        if (Gas != null)
            if (gasMixture.GetMoles(Gas.Value) == 0f)
                return false;

        if (MinTemperature != null)
            if (gasMixture.Temperature < MinTemperature.Value)
                return false;

        if (MaxTemperature != null)
            if (gasMixture.Temperature > MaxTemperature.Value)
                return false;

        return true;
    }

    public override SlimeTransformationCondition GetRandomCondition(IEntityManager entMan, IPrototypeManager protoMan, IRobustRandom random)
    {
        Gas? gas = random.Prob(0.5f)
            //Мы знаем что значение не выйдет за пределы Gas следовательно и short byte тоже
            //      vvvvvvvvvvvvvvv
            ? (Gas) Convert.ToSByte(random.Next(1, (int) Enum.GetValues<Gas>().Max() + 1))
            : null;

        FixedPoint2? min = random.Prob(0.5f)
            ? null
            : random.NextFloat(-100, 80);

        FixedPoint2? max = random.Prob(0.5f)
            ? null
            : min == null
                ? random.NextFloat(-100, 100)
                : random.NextFloat(min.Value.Float() + 20, 100);

        return new TileTemperatureMutationCondition()
        {
            Gas = gas,
            MaxTemperature = max,
            MinTemperature = min
        };
    }

    public override string GetDescriptionString(IEntityManager entityManager, IPrototypeManager protoMan)
            => Loc.GetString("slime-transformation-condition-tile-temperature",
                ("gas", Gas == null ? 0 : Loc.GetString(protoMan.Index<GasPrototype>(entityManager.System<AtmosphereSystem>().GetGas(Gas.Value).ID).Name)),
                ("min", MinTemperature == null ? 0 : MinTemperature),
                ("max", MaxTemperature == null ? 0 : MaxTemperature),
                ("state", MinTemperature != null && MaxTemperature != null
                        ? "both"
                        : MinTemperature == null && MaxTemperature == null
                            ? "bothnull"
                            : MaxTemperature == null
                                ? "maxnull"
                                : "minnull"));
}
