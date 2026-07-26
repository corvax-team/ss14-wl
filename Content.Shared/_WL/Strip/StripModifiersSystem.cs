using Content.Shared.Inventory;
using Content.Shared._WL.Strip.Components;
using Content.Shared.Strip.Components;

namespace Content.Shared._WL.Strip;

public sealed partial class StripModifiersSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StripModifiersComponent, BeforeStripEvent>(OnBeforeStrip);
        SubscribeLocalEvent<StripModifiersComponent, InventoryRelayedEvent<BeforeStripEvent>>((e, c, ev) =>
            OnBeforeStrip(e, c, ev.Args));
    }

    private void OnBeforeStrip(EntityUid uid, StripModifiersComponent component, BeforeStripEvent args)
    {
        args.Additive -= component.StripTimeReduction;
        args.Multiplier *= component.StripTimeMultiplier;
    }
}
