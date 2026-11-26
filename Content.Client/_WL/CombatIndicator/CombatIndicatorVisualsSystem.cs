using System.Numerics;
using Content.Shared._WL.CombatIndicator;
using Robust.Shared.Map;

namespace Content.Client._WL.CombatIndicator;

public sealed partial class CombatIndicatorVisualsSystem : EntitySystem
{
    [SubscribeLocalEvent]
    private void OnStartup(Entity<CombatIndicatorComponent> ent, ref ComponentStartup args) =>
        SetIndicatorVisuals(ent, ent.Comp.Enabled, ent.Comp);

    [SubscribeLocalEvent]
    private void OnShutdown(Entity<CombatIndicatorComponent> ent, ref ComponentShutdown args) =>
        SetIndicatorVisuals(ent, false, ent.Comp);

    [SubscribeLocalEvent]
    private void OnChanged(Entity<CombatIndicatorComponent> ent, ref CombatIndicatorChangedEvent args) =>
        SetIndicatorVisuals(ent, args.Enabled, ent.Comp);

    private void SetIndicatorVisuals(EntityUid uid, bool value, CombatIndicatorComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        if (value)
        {
            if (comp.IndicatorEntity != null)
                return;

            var indicator = SpawnAttachedTo(comp.IndicatorProto,
                new EntityCoordinates(uid, Vector2.Zero));

            comp.IndicatorEntity = indicator;
        }
        else
        {
            QueueDel(comp.IndicatorEntity);
            comp.IndicatorEntity = null;
        }
    }
}
