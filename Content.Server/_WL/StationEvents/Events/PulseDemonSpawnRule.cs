using Content.Server.Power.Components;
using Content.Server._WL.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Robust.Shared.Random;
using System.Linq;
using Content.Shared.GameTicking.Components;
using Content.Server.Station.Systems;
using Content.Server.Shuttles.Components;


namespace Content.Server._WL.StationEvents.Events;

public sealed class PulseDemonSpawnRule : StationEventSystem<PulseDemonSpawnRuleComponent>
{
    protected override void Started(EntityUid uid, PulseDemonSpawnRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        var entMan = IoCManager.Resolve<IEntityManager>();
        var stationSys = entMan.System<StationSystem>();

        var query = EntityQuery<CableComponent, TransformComponent>()
            .Where(x =>
            {
                var station = stationSys.GetOwningStation(x.Item1.Owner, x.Item2);
                if (station == null)
                    return false;

                //if (HasComp<StationCentcommComponent>(station.Value))
                //    return false;

                return true;
            })
            .ToList();

        if (query.Count == 0)
            return;

        var coords = IoCManager.Resolve<IRobustRandom>().Pick(query).Item2.Coordinates;

        Sawmill.Info($"Spawning {comp.Prototype} at {coords}");
        Spawn(comp.Prototype, coords);
    }
}
