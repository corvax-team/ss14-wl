using System.Numerics;
using Content.Shared._WL.CombatIndicator;
using Robust.Shared.Map;

namespace Content.Client._WL.CombatIndicator;

public sealed partial class CombatIndicatorVisualsSystem : EntitySystem
{
    [SubscribeLocalEvent]
    private void OnStartup(Entity<CombatIndicatorComponent> ent, ref ComponentStartup args)
    {
        if (ent.Comp.IndicatorEntity != null)
            return;

        var indicator = SpawnAttachedTo(ent.Comp.IndicatorProto,
            new EntityCoordinates(ent.Owner, Vector2.Zero));

        ent.Comp.IndicatorEntity = indicator;
    }

    [SubscribeLocalEvent]
    private void OnShutdown(Entity<CombatIndicatorComponent> ent, ref ComponentShutdown args)
    {
        QueueDel(ent.Comp.IndicatorEntity);
        ent.Comp.IndicatorEntity = null;
    }
}
