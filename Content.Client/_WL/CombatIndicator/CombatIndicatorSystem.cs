using System.Numerics;
using Content.Shared._WL.CombatIndicator;
using Robust.Shared.Map;

namespace Content.Client._WL.CombatIndicator;

public sealed partial class CombatIndicatorSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CombatIndicatorComponent, AfterAutoHandleStateEvent>(OnState);
        SubscribeLocalEvent<CombatIndicatorComponent, ComponentShutdown>(OnShutdown);
    }


    private void OnState(Entity<CombatIndicatorComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (ent.Comp.IndicatorEntity != null)
            return;

        var indicator = SpawnAttachedTo(ent.Comp.IndicatorProto,
            new EntityCoordinates(ent.Owner, Vector2.Zero));

        ent.Comp.IndicatorEntity = indicator;
    }



    private void OnShutdown(Entity<CombatIndicatorComponent> ent, ref ComponentShutdown args)
    {
        PredictedQueueDel(ent.Comp.IndicatorEntity);
        ent.Comp.IndicatorEntity = null;
    }
}
