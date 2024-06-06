using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Content.Shared._WL.Inventory
{
    public sealed partial class InventorySlotsBlockingSystem : EntitySystem
    {
        [Dependency] private readonly InventorySystem _inventory = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<InventoryComponent, IsEquippingAttemptEvent>(OnEquip);
            SubscribeLocalEvent<InventoryComponent, IsUnequippingAttemptEvent>(OnUnequip);
        }

        private void OnEquip(EntityUid entity, InventoryComponent comp, IsEquippingAttemptEvent args)
        {
            if (args.Cancelled)
                return;

            if (!IsSlotBlocked((entity, comp), args.SlotFlags, out var reasons))
                return;

            var reason = $"Для начала нужно снять ";

            var stringReasons = reasons.Select(e => Identity.Name(e, EntityManager));
            reason += string.Join(" и ", stringReasons);

            args.Reason = reason;
            args.Cancel();
        }

        private void OnUnequip(EntityUid entity, InventoryComponent comp, IsUnequippingAttemptEvent args)
        {
            if (args.Cancelled)
                return;

            if (!IsSlotBlocked((entity, comp), args.Slot, out var reasons))
                return;

            var reason = $"Для начала нужно снять ";

            var stringReasons = reasons.Select(e => Identity.Name(e, EntityManager));
            reason += string.Join(" и ", stringReasons);

            args.Reason = reason;
            args.Cancel();
        }

        public bool IsSlotBlocked(Entity<InventoryComponent> entityWithInventoryComp, SlotDefinition slotDef, [NotNullWhen(true)] out List<EntityUid>? reasons)
        {
            var blocked = IsSlotBlocked(entityWithInventoryComp, slotDef.SlotFlags, out var reass);
            reasons = reass;
            return blocked;
        }

        public bool IsSlotBlocked(Entity<InventoryComponent> entityWithInventoryComp, SlotFlags slotFlags, [NotNullWhen(true)] out List<EntityUid>? reasons)
        {
            reasons = new();

            var entity = entityWithInventoryComp.Owner;
            var inventoryComp = entityWithInventoryComp.Comp;

            for (var indexer = 0; indexer < inventoryComp.Slots.Length; indexer++)
            {
                var slotEntity = inventoryComp.Containers[indexer].ContainedEntity;
                if (slotEntity == null)
                    continue;

                var extraSlots = SlotFlags.NONE;
                if (TryComp<ExtraBlockingInventorySlotsComponent>(slotEntity, out var extraBlockingSlotsComp))
                    extraBlockingSlotsComp.Slots.ForEach(s => extraSlots |= s);

                var inventorySlotDef = inventoryComp.Slots[indexer];
                if (inventorySlotDef.BlockSlots.Any(s => s.HasFlag(slotFlags)) || extraSlots.HasFlag(slotFlags))
                {
                    reasons.Add(slotEntity.Value);
                }
            }

            return reasons.Count > 0;
        }

        public bool IsSlotBlocked(Entity<InventoryComponent> entityWithInventoryComp, string slot, [NotNullWhen(true)] out List<EntityUid>? reasons)
        {
            reasons = new();

            if (!_inventory.TryGetSlot(entityWithInventoryComp.Owner, slot, out var slotDef, entityWithInventoryComp.Comp))
                return false;

            var blocked = IsSlotBlocked(entityWithInventoryComp, slotDef, out var reass);
            reasons = reass;
            return blocked;
        }
    }
}
