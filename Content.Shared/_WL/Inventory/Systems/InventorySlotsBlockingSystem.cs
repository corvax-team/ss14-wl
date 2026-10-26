using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Shared._WL.Inventory.Systems;

public sealed partial class InventorySlotsBlockingSystem : EntitySystem
{
    [Dependency] private InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InventoryComponent, IsEquippingAttemptEvent>(OnEquip);
        SubscribeLocalEvent<InventoryComponent, IsUnequippingAttemptEvent>(OnUnequip);
    }

    private void OnEquip(EntityUid entity, InventoryComponent _, IsEquippingAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryComp<InventoryComponent>(args.EquipTarget, out var comp))
            return;

        if (!IsSlotBlocked((args.EquipTarget, comp), args.SlotFlags, out var reasons, args.Equipment))
            return;

        args.Reason = Loc.GetString("isb-system-reason", ("entities", string.Join(", ", reasons.Select(e => Identity.Name(e, EntityManager)))));
        args.Cancel();
    }

    private void OnUnequip(EntityUid entity, InventoryComponent _, IsUnequippingAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryComp<InventoryComponent>(args.UnEquipTarget, out var comp))
            return;

        if (!IsSlotBlocked((args.UnEquipTarget, comp), args.Slot, out var reasons, args.Equipment))
            return;

        args.Reason = Loc.GetString("isb-system-reason", ("entities", string.Join(", ", reasons.Select(e => Identity.Name(e, EntityManager)))));
        args.Cancel();
    }

    public bool IsSlotBlocked(Entity<InventoryComponent> entityWithInventoryComp, SlotDefinition slotDef, [NotNullWhen(true)] out List<EntityUid>? reasons, EntityUid? equipment = null)
    {
        var blocked = IsSlotBlocked(entityWithInventoryComp, slotDef.SlotFlags, out reasons, equipment);
        return blocked;
    }

    public bool IsSlotBlocked(Entity<InventoryComponent> entityWithInventoryComp, SlotFlags slotFlags, [NotNullWhen(true)] out List<EntityUid>? reasons, EntityUid? equipment = null)
    {
        reasons = new();

        var entity = entityWithInventoryComp.Owner;
        var inventoryComp = entityWithInventoryComp.Comp;
        ClothingSlotBlockingBypassComponent? bypass = null;
        if (equipment != null)
            TryComp(equipment, out bypass);

        for (var indexer = 0; indexer < inventoryComp.Slots.Length; indexer++)
        {
            var slotEntity = inventoryComp.Containers[indexer].ContainedEntity;
            if (slotEntity == null)
                continue;

            var inventorySlotDef = inventoryComp.Slots[indexer];
            if (bypass != null && bypass.Blockers.Contains(inventorySlotDef.Name))
                continue;

            var extraSlots = SlotFlags.NONE;
            if (TryComp<ExtraBlockingInventorySlotsComponent>(slotEntity, out var extraBlockingSlotsComp))
                extraBlockingSlotsComp.Slots.ForEach(s => extraSlots |= s);

            if (inventorySlotDef.BlockSlots.Any(s => s.HasFlag(slotFlags)) || extraSlots.HasFlag(slotFlags))
            {
                reasons.Add(slotEntity.Value);
            }
        }

        return reasons.Count > 0;
    }

    public bool IsSlotBlocked(Entity<InventoryComponent> entityWithInventoryComp, string slot, [NotNullWhen(true)] out List<EntityUid>? reasons, EntityUid? equipment = null)
    {
        reasons = new();

        if (!_inventory.TryGetSlot(entityWithInventoryComp.Owner, slot, out var slotDef, entityWithInventoryComp.Comp))
            return false;

        var blocked = IsSlotBlocked(entityWithInventoryComp, slotDef, out reasons, equipment);
        return blocked;
    }
}
