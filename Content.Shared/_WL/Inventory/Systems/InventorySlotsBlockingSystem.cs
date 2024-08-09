using Content.Shared.Hands.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Shared._WL.Inventory.Systems
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

        private void OnEquip(EntityUid entity, InventoryComponent _, IsEquippingAttemptEvent args)
        {
            if (args.Cancelled)
                return;

            if (!TryComp<InventoryComponent>(args.EquipTarget, out var comp))
                return;

            if (!IsSlotBlocked((args.EquipTarget, comp), args.SlotFlags, out var reasons))
                return;

            var reason = $"Для начала нужно снять ";

            var stringReasons = reasons.Select(e => Identity.Name(e.Key, EntityManager));
            reason += string.Join(" и ", stringReasons);

            args.Reason = reason;
            args.Cancel();
        }

        private void OnUnequip(EntityUid entity, InventoryComponent _, IsUnequippingAttemptEvent args)
        {
            if (args.Cancelled)
                return;

            if (!TryComp<InventoryComponent>(args.UnEquipTarget, out var comp))
                return;

            if (!IsSlotBlocked((args.UnEquipTarget, comp), args.Slot, out var reasons))
                return;

            var reason = $"Для начала нужно снять ";

            var stringReasons = reasons.Select(e => Identity.Name(e.Key, EntityManager));
            reason += string.Join(" и ", stringReasons);

            args.Reason = reason;
            args.Cancel();
        }

        public bool IsSlotBlocked(Entity<InventoryComponent> entityWithInventoryComp, SlotDefinition slotDef, [NotNullWhen(true)] out Dictionary<EntityUid, SlotFlags>? reasons)
        {
            var blocked = IsSlotBlocked(entityWithInventoryComp, slotDef.SlotFlags, out var reass);
            reasons = reass;
            return blocked;
        }

        public bool IsSlotBlocked(Entity<InventoryComponent> entityWithInventoryComp, SlotFlags slotFlags, [NotNullWhen(true)] out Dictionary<EntityUid, SlotFlags>? reasons)
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
                    reasons.Add(slotEntity.Value, inventorySlotDef.SlotFlags);
                }
            }

            return reasons.Count > 0;
        }

        public bool IsSlotBlocked(Entity<InventoryComponent> entityWithInventoryComp, string slot, [NotNullWhen(true)] out Dictionary<EntityUid, SlotFlags>? reasons)
        {
            reasons = new();

            if (!_inventory.TryGetSlot(entityWithInventoryComp.Owner, slot, out var slotDef, entityWithInventoryComp.Comp))
                return false;

            return IsSlotBlocked(entityWithInventoryComp, slotDef, out reasons);
        }

        public Dictionary<EntityUid, SlotFlags> GetClothes(Entity<InventoryComponent> ent, bool blocked)
        {
            var dict = new Dictionary<EntityUid, SlotFlags>();
            var comp = ent.Comp;
            var slots = comp.Slots;

            foreach (var slot in slots)
            {
                if (IsSlotBlocked(ent, slot, out var blockedClothes) != blocked)
                    continue;

                if (blockedClothes == null || blockedClothes.Count == 0)
                    continue;

                dict = dict.Union(blockedClothes).ToDictionary();
            }

            return dict;
        }

        /// <summary>
        /// Ищет и возвращает всю незаблокированную другой одеждой одежду с определенным компонентом, в которую одета указанная сущность.
        /// </summary>
        /// <typeparam name="T">Компонент, который должен быть у вещи, для поиска</typeparam>
        /// <param name="ent">Сущность, одежду с которой надо найти</param>
        /// <param name="searchFlags">Флаги для поиска одежды</param>
        /// <param name="excludeFlags">Исключающие флаги для поиска одежды</param>
        /// <returns></returns>
        public IEnumerable<Entity<T>> GetAvailableWornClothes<T>(
            Entity<HandsComponent?, InventoryComponent?> ent,
            SlotFlags searchFlags = SlotFlags.All,
            SlotFlags excludeFlags = SlotFlags.NONE)
                where T : IComponent
        {
            var list = new List<Entity<T>>();

            if (!Resolve(ent.Owner, ref ent.Comp2, false))
                return list;

            var blocked = GetClothes(
                    ent: (ent.Owner, ent.Comp2),
                    blocked: true
                );

            foreach (var e in _inventory.GetHandOrInventoryEntities(ent, searchFlags))
            {
                if (!TryComp<T>(e, out var comp))
                    continue;

                if (blocked.TryGetValue(e, out var flags) || flags.HasFlag(excludeFlags))
                    continue;

                list.Add((e, comp));
            }

            return list;
        }

        public IEnumerable<EntityUid> GetAvailableWornClothes(
            Entity<HandsComponent?, InventoryComponent?> ent,
            SlotFlags searchFlags = SlotFlags.All,
            SlotFlags excludeFlags = SlotFlags.NONE)
        {
            return GetAvailableWornClothes<MetaDataComponent>(ent, searchFlags, excludeFlags)
                .Select(e => e.Owner);
        }
    }
}
