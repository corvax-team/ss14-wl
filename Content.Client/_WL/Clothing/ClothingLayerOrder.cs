using Content.Shared._WL.Clothing.Components;
using Content.Shared.Clothing;
using Content.Shared.Inventory;
using Robust.Client.GameObjects;

namespace Content.Client.Clothing;

public sealed partial class ClientClothingSystem
{
    private readonly HashSet<EntityUid> _updatingLayerOrder = new();

    [SubscribeLocalEvent]
    private void OnGetLayerOrder(Entity<ClothingLayerOrderComponent> ent, ref GetEquipmentVisualsEvent args)
    {
        args.AboveSlots = ent.Comp.Above;
    }

    private Dictionary<string, List<string>> GetLayerDependencies(EntityUid wearer)
    {
        var dependencies = new Dictionary<string, List<string>>();
        if (!TryComp<InventoryComponent>(wearer, out var inventory))
            return dependencies;

        var slots = _inventorySystem.GetSlotEnumerator((wearer, inventory));
        while (slots.NextItem(out var item, out var slot))
        {
            if (TryComp<ClothingLayerOrderComponent>(item, out var order))
                dependencies[slot.Name] = order.Above;
        }

        return dependencies;
    }

    private static bool HasLayerDependency(
        Dictionary<string, List<string>> dependencies, string start, string target, HashSet<string> visited)
    {
        if (start == target)
            return true;
        if (!visited.Add(start) || !dependencies.TryGetValue(start, out var above))
            return false;

        foreach (var slot in above)
        {
            if (HasLayerDependency(dependencies, slot, target, visited))
                return true;
        }
        return false;
    }

    private void RefreshLayerOrder(EntityUid wearer)
    {
        if (!_updatingLayerOrder.Add(wearer))
            return;

        try
        {
            var dependencies = GetLayerDependencies(wearer);
            var visited = new HashSet<string>();
            foreach (var slot in dependencies.Keys)
                Refresh(slot);

            void Refresh(string slot)
            {
                if (!visited.Add(slot) || !dependencies.TryGetValue(slot, out var above))
                    return;

                foreach (var target in above)
                {
                    // Ignore cyclic constraints rather than repeatedly moving the same layers.
                    if (!HasLayerDependency(dependencies, target, slot, new()))
                        Refresh(target);
                }

                if (_inventorySystem.TryGetSlotEntity(wearer, slot, out var item))
                    RenderEquipment(wearer, item.Value, slot);
            }
        }
        finally
        {
            _updatingLayerOrder.Remove(wearer);
        }
    }
}
