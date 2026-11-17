using System.Numerics;
using Content.Server.Solar.EntitySystems;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Methods;
using Content.Shared.FixedPoint;

namespace Content.Server._WL.Research.Methods;

public sealed partial class SolarPanelsResearchConditionSystem : ResearchConditionSystem<ResearchSourceComponent, SolarPanelsResearchCondition>
{
    [Dependency] private PowerSolarSystem _powerSolarSystem = default!;

    protected override void Condition(Entity<ResearchSourceComponent> entity, ref ResearchConditionEvent<SolarPanelsResearchCondition> args)
    {
        var sunVec = _powerSolarSystem.TowardsSun.ToVec();
        var panelVec = _powerSolarSystem.TargetPanelRotation.ToVec();

        args.Result = Math.Max(sunVec.X*panelVec.X + sunVec.Y*panelVec.Y, 0) * args.Condition.Factor;
    }
}
