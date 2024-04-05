using Content.Server.GameTicking.Rules.Components;
using Content.Server.Power.Components;
using Content.Server._WL.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Server._WL.StationEvents.Events;

public sealed class PulseDemonSpawnRule : StationEventSystem<PulseDemonSpawnRuleComponent>
{
    protected override void Started(EntityUid uid, PulseDemonSpawnRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        var query = EntityQuery<CableComponent, TransformComponent>().ToList();

        if (query.Count == 0)
            return;

        var coords = IoCManager.Resolve<IRobustRandom>().Pick(query).Item2.Coordinates;

        Sawmill.Info($"Spawning {comp.Prototype} at {coords}");
        Spawn(comp.Prototype, coords);
    }
}
