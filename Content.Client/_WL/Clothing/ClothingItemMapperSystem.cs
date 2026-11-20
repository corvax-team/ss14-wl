using Content.Client.Clothing;
using Content.Shared._WL.Clothing.Components;
using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Storage.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Serialization.Manager;

namespace Content.Client._WL.Clothing;

public sealed partial class ClothingItemMapperSystem : EntitySystem
{
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedItemSystem _items = default!;
    [Dependency] private ISerializationManager _serialization = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ClothingItemMapperComponent, AppearanceChangeEvent>(OnAppearanceChanged);
        SubscribeLocalEvent<ClothingItemMapperComponent, GetEquipmentVisualsEvent>(OnGetVisuals,
            after: new[] { typeof(ClientClothingSystem) });
    }

    private void OnAppearanceChanged(Entity<ClothingItemMapperComponent> ent, ref AppearanceChangeEvent args)
    {
        _items.VisualsChanged(ent);
    }

    private void OnGetVisuals(Entity<ClothingItemMapperComponent> ent, ref GetEquipmentVisualsEvent args)
    {
        if (!_appearance.TryGetData<ShowLayerData>(ent, StorageMapVisuals.LayerChanged, out var active)
            || !TryComp<ClothingComponent>(ent, out var clothing))
            return;

        TryComp<InventoryComponent>(args.Equipee, out var inventory);
        TryComp<SpriteComponent>(ent, out var sprite);

        foreach (var key in active.QueuedEntities)
        {
            if (!ent.Comp.MapLayers.TryGetValue(key, out var slots))
                continue;

            List<PrototypeLayerData>? layers = null;
            if (inventory?.SpeciesId != null)
                slots.TryGetValue($"{args.Slot}-{inventory.SpeciesId}", out layers);

            if (layers == null && !slots.TryGetValue(args.Slot, out layers))
                continue;

            for (var i = 0; i < layers.Count; i++)
            {
                var layer = new PrototypeLayerData();
                _serialization.CopyTo(layers[i], ref layer, notNullableOverride: true);
                layer.RsiPath ??= clothing.RsiPath ?? sprite?.BaseRSI?.Path.ToString();
                args.Layers.Add(($"clothing-item-mapper-{args.Slot}-{key}-{i}", layer));
            }
        }
    }
}
