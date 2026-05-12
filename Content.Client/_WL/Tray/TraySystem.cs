using Content.Client.Items.Systems;
using Content.Shared._WL.Tray;
using Content.Shared.Hands;
using Robust.Client.GameObjects;
using System.Linq;

namespace Content.Client._WL.Tray;

public sealed partial class TraySystem : SharedTraySystem
{
    [Dependency] private AppearanceSystem _apperance = default!;
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private ItemSystem _item = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TrayComponent, GetInhandVisualsEvent>(OnGetHeldVisuals, after: [typeof(ItemSystem)]);
        SubscribeLocalEvent<TrayComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }

    private void OnGetHeldVisuals(EntityUid uid, TrayComponent component, GetInhandVisualsEvent args)
    {
        if (!TryComp(uid, out AppearanceComponent? appearance)
            || !_apperance.TryGetData<bool>(uid, TrayVisualState.HasEntities, out var hasEntities, appearance)
            || !_apperance.TryGetData<bool>(uid, TrayVisualState.Closed, out var closed, appearance))
            return;

        if (component.ItemsInhandVisuals.TryGetValue(args.Location, out var layers) && hasEntities)
        {
            var defaultKey = $"items-inhand-{args.Location.ToString().ToLowerInvariant()}";
            foreach (var layer in layers)
            {
                var key = layer.MapKeys?.FirstOrDefault();
                if (key == null)
                    key = defaultKey;

                args.Layers.Add((key, layer));
            }
        }

        if (component.CapInhandVisuals.TryGetValue(args.Location, out layers) && closed)
        {
            var defaultKey = $"cap-inhand-{args.Location.ToString().ToLowerInvariant()}";
            foreach (var layer in layers)
            {
                var key = layer.MapKeys?.FirstOrDefault();
                if (key == null)
                    key = defaultKey;

                args.Layers.Add((key, layer));
            }
        }
    }

    private void OnAppearanceChanged(EntityUid uid, TrayComponent component, AppearanceChangeEvent args)
    {
        _item.VisualsChanged(uid);

        if (!TryComp(uid, out AppearanceComponent? appearance)
            || !_apperance.TryGetData<bool>(uid, TrayVisualState.Closed, out var closed, appearance))
            return;

        _sprite.LayerSetVisible(uid, TrayVisualLayers.Cap, closed);
    }
}

