using System.Diagnostics.CodeAnalysis;
using Content.Server._WL.Security.Components;
using Content.Shared._WL.Clothing.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Examine;
using Content.Shared.Inventory;

namespace Content.Server._WL.Security.Systems;

public sealed partial class SecurityBadgeSystem : EntitySystem
{
    [Dependency] private InventorySystem _inventorySystem = default!;
    [Dependency] private ItemSlotsSystem _itemSlots = default!;


    [SubscribeLocalEvent]
    private void OnExamine(Entity<InventoryComponent> ent, ref ExaminedEvent ev)
    {
        if (!TryFindBadge(ent.Owner, out var badge))
            return;

        ev.PushMarkup(Loc.GetString("security-badge-component-rank-description",
            ("loc", Loc.GetString(badge.Value.Comp.RankLoc))), -2);
    }

    private bool TryFindBadge(EntityUid equipee, [NotNullWhen(true)] out Entity<SecurityBadgeComponent>? badge)
    {
        var enumerator = _inventorySystem.GetSlotEnumerator(equipee);
        while (enumerator.NextItem(out var item))
        {
            if (TryComp<SecurityBadgeComponent>(item, out var badgeComp))
            {
                badge = (item, badgeComp);
                return true;
            }

            if (!TryComp<ClothingAccessoriesComponent>(item, out var accessories)
                || !TryComp<ItemSlotsComponent>(item, out var itemSlots)
                || !_itemSlots.TryGetSlot((item, itemSlots), accessories.Slot, out var slot)
                || slot.Item is not { } accessory
                || !TryComp<SecurityBadgeComponent>(accessory, out var accessoryBadge))
                continue;

            badge = (accessory, accessoryBadge);

            return true;
        }

        badge = null;

        return false;
    }
}
