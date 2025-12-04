using Content.Server._WL.Security.Components;
using Content.Shared.Examine;
using Content.Shared.Inventory;

namespace Content.Server._WL.Security.Systems;

public sealed partial class SecurityBadgeSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InventoryComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(EntityUid ent, InventoryComponent comp, ExaminedEvent ev)
    {
        if (!_inventorySystem.TryGetInventoryEntity<SecurityBadgeComponent>((ent, comp), out var badgeEnt) || badgeEnt.Comp == null)
            return;

        ev.PushMarkup(Loc.GetString("security-badge-component-rank-description", ("loc", Loc.GetString(badgeEnt.Comp.RankLoc))), -2);
    }
}
