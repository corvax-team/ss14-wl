using System.Linq;
using Content.Shared._WL.Clothing.Components;
using Content.Shared.Clothing;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Inventory;
using Robust.Shared.Containers;

namespace Content.Client.Clothing;

public sealed partial class ClientClothingSystem
{
    [Dependency] private ItemSlotsSystem _itemSlots = default!;

    [SubscribeLocalEvent]
    private void OnGetClothingAccessoryVisuals(Entity<ClothingAccessoriesComponent> ent, ref GetEquipmentVisualsEvent args)
    {
        if (!ClothingQuery.TryComp(ent, out var hostClothing)
            || hostClothing.InSlot != args.Slot
            || !TryComp(ent, out ItemSlotsComponent? itemSlots)
            || !_itemSlots.TryGetSlot((ent.Owner, itemSlots), ent.Comp.Slot, out var slot)
            || slot.Item is not { } accessory
            || !ClothingQuery.TryComp(accessory, out var clothing))
            return;

        if (!TryComp<InventoryComponent>(args.Equipee, out var inventory))
            return;

        var visualSlot = inventory.Slots
            .FirstOrDefault(definition => (definition.SlotFlags & clothing.Slots) != SlotFlags.NONE)
            ?.Name;

        if (visualSlot == null)
            return;

        var accessoryVisuals = new GetEquipmentVisualsEvent(args.Equipee, visualSlot);
        RaiseLocalEvent(accessory, accessoryVisuals);

        if (accessoryVisuals.Layers.Count == 0)
            return;

        _spriteQuery.TryComp(accessory, out var accessorySprite);
        var speciesId = inventory.SpeciesId;
        if (speciesId != null)
            SetSpeciesSpecificLayers((accessory, clothing, accessorySprite), speciesId, accessoryVisuals.Layers);

        var index = 0;
        foreach (var (_, sourceLayer) in accessoryVisuals.Layers)
        {
            var layer = new PrototypeLayerData();
            _seriMan.CopyTo(sourceLayer, ref layer, notNullableOverride: true);
            layer.RsiPath ??= clothing.RsiPath ?? accessorySprite?.BaseRSI?.Path.ToString();
            args.Layers.Add(($"clothing-accessory-{ent.Owner}-{accessory}-{index++}", layer));
        }
    }

    [SubscribeLocalEvent]
    private void OnClothingAccessoryInserted(
        Entity<ClothingAccessoriesComponent> ent,
        ref EntInsertedIntoContainerMessage args)
    {
        RefreshClothingAccessories(ent, args.Container);
    }

    [SubscribeLocalEvent]
    private void OnClothingAccessoryRemoved(
        Entity<ClothingAccessoriesComponent> ent,
        ref EntRemovedFromContainerMessage args)
    {
        RefreshClothingAccessories(ent, args.Container);
    }

    private void RefreshClothingAccessories(Entity<ClothingAccessoriesComponent> ent, BaseContainer container)
    {
        if (container.ID != ent.Comp.Slot
            || !ClothingQuery.TryComp(ent, out var clothing)
            || clothing.InSlot is not { } slot)
            return;

        RenderEquipment(Transform(ent).ParentUid, ent, slot);
    }
}
