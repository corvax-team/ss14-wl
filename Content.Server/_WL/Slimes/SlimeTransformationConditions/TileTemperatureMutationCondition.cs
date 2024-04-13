using Content.Server.Atmos.EntitySystems;
using Content.Shared._WL.Slimes;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Slimes.SlimeTransformationConditions;

public sealed partial class TileTemperatureMutationCondition : SlimeTransformationCondition
{
    [DataField]
    public Gas? Gas = null;

    [DataField("min")]
    public float? MinTemperature = null;

    [DataField("max")]
    public float? MaxTemperature = null;

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

    public override string GetDescriptionString(IEntityManager entityManager, IPrototypeManager protoMan)
            => Loc.GetString("slime-transformation-condition-tile-temperature",
                ("separator", ""),
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
