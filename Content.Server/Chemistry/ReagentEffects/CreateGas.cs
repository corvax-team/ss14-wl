using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Database;
using Robust.Shared.Prototypes;

namespace Content.Server.Chemistry.ReagentEffects;

public sealed partial class CreateGas : ReagentEffect
{
    [DataField(required: true)]
    public Gas Gas = default!;

    /// <summary>
    ///     For each unit consumed, how many moles of gas should be created?
    /// </summary>
    [DataField]
    public float Factor = 3f;

    [DataField]
    public float Temperature = 2.7f;

    public override bool ShouldLog => true;
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        var atmos = entSys.GetEntitySystem<AtmosphereSystem>();
        var gasProto = atmos.GetGas(Gas);

        return Loc.GetString("reagent-effect-guidebook-create-gas",
            ("chance", Probability),
            ("moles", Factor),
            ("gas", gasProto.Name),
            ("temp", Temperature));
    }

    public override LogImpact LogImpact => LogImpact.High;

    public override void Effect(ReagentEffectArgs args)
    {
        var atmosSys = args.EntityManager.EntitySysManager.GetEntitySystem<AtmosphereSystem>();

        var tileMix = atmosSys.GetContainingMixture(args.SolutionEntity, false, true);

        tileMix?.AdjustMoles(Gas, args.Quantity.Float() * Factor);

        if (tileMix != null)
            tileMix.Temperature = Temperature;
    }
}
