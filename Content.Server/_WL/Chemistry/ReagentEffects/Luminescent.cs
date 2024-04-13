using Content.Shared.Chemistry.Reagent;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Chemistry.ReagentEffects;

public sealed partial class Luminescent : ReagentEffect
{
    [DataField("energy")]
    public float Energy = 0.05f;

    [DataField("softness")]
    public float Softness = 0f;

    [DataField("radius")]
    public float Radius = 0.07f;

    [DataField("color")]
    public Color LightColor = Color.White;

    public override void Effect(ReagentEffectArgs args)
    {
        var _entityManager = args.EntityManager;
        var _pointLight = _entityManager.System<PointLightSystem>();

        if (!_entityManager.TryGetComponent<PointLightComponent>(args.SolutionEntity, out var pointLightComp))
        {
            pointLightComp = _entityManager.EnsureComponent<PointLightComponent>(args.SolutionEntity);
            _pointLight.SetEnergy(args.SolutionEntity, 0);
            _pointLight.SetRadius(args.SolutionEntity, 0);
        }

        _pointLight.SetColor(args.SolutionEntity, LightColor, pointLightComp);

        _pointLight.SetEnergy(args.SolutionEntity, pointLightComp.Energy + Energy);
        _pointLight.SetRadius(args.SolutionEntity, pointLightComp.Radius + Radius);
        _pointLight.SetSoftness(args.SolutionEntity, pointLightComp.Softness + Softness);
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-luminescent",
          ("chance", Probability),
          ("color", LightColor.ToHex()));
}
