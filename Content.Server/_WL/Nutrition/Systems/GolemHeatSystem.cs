using Content.Server._WL.Nutrition.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.Temperature.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using JetBrains.Annotations;

namespace Content.Server._WL.Nutrition.Systems;

[UsedImplicitly]
public sealed partial class GolemHeatSystem : EntitySystem
{
    [Dependency] private SatiationSystem _satiation = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private TemperatureSystem _temperature = default!;

    private const float HeatChangePerSecond = 400f;
    private const float HungerBoostThreshold = 185f;
    private const float BaseSprintSpeed = 2.5f;
    private const float BaseWalkSpeed = 1.5f;
    private const float BoostedSprintSpeed = 3.0f;
    private const float BoostedWalkSpeed = 1.8f;
    private const int Acceleration = 20;

    private void ChangeGolemHeat(EntityUid uid, float frameTime)
    {
        if (!TryComp<SatiationComponent>(uid, out var satiation))
            return;

        var movementSpeed = EnsureComp<MovementSpeedModifierComponent>(uid);
        var hunger = _satiation.GetValueOrNull((uid, satiation), SatiationSystem.Hunger);

        if (hunger < HungerBoostThreshold)
        {
            _movement.ChangeBaseSpeed(uid, BaseWalkSpeed, BaseSprintSpeed, Acceleration, movementSpeed);
            return;
        }

        _temperature.ChangeHeat((uid, null), HeatChangePerSecond * frameTime, true);

        _movement.ChangeBaseSpeed(uid, BoostedWalkSpeed, BoostedSprintSpeed, Acceleration, movementSpeed);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<GolemHeatComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            ChangeGolemHeat(uid, frameTime);
        }
    }
}
