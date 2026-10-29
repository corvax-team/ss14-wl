using System.Linq;
using Content.Shared.Body;
using Content.Shared.Humanoid;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Utility;

namespace Content.Client._WL.Body;

public sealed partial class DirectionalBodyLayersSystem
{
    private enum TailAnchor
    {
        Original,
    }

    private void ApplyTailOrder(Entity<SpriteComponent?> entity, RsiDirection direction)
    {
        var keys = new List<object>
        {
            HumanoidVisualLayers.Tail, "Tail-displacement",
            HumanoidVisualLayers.TailOverlay, "TailOverlay-displacement",
        };
        foreach (var organ in _body.EnumerateOrgans<VisualOrganMarkingsComponent>(entity.Owner))
        {
            foreach (var marking in organ.Comp2.AppliedMarkings)
            {
                if (!_markings.TryGetMarking(marking, out var prototype) ||
                    prototype.BodyPart is not (HumanoidVisualLayers.Tail or HumanoidVisualLayers.TailOverlay))
                    continue;

                foreach (var source in prototype.Sprites)
                {
                    if (source is not SpriteSpecifier.Rsi rsi)
                        continue;

                    var key = $"{prototype.ID}-{rsi.RsiState}";
                    keys.Add(key);
                    keys.Add($"{key}-displacement");
                }
            }
        }

        var captured = new Dictionary<SpriteComponent.Layer, CapturedLayer>();
        foreach (var key in keys)
        {
            var exists = key is Enum enumKey
                ? _sprite.LayerMapTryGet(entity, enumKey, out var index, false)
                : _sprite.LayerMapTryGet(entity, (string)key, out index, false);
            if (!exists || !_sprite.TryGetLayer(entity, index, out var layer, false))
                continue;

            if (!captured.TryGetValue(layer, out var entry))
            {
                entry = new CapturedLayer(index, layer, new());
                captured.Add(layer, entry);
            }
            entry.Keys.Add(key);
        }

        var layers = captured.Values.OrderBy(entry => entry.Index).ToArray();
        if (layers.Length == 0)
            return;

        // Leave an invisible bookmark at the original tail position, independent of clothing changes.
        if (!_sprite.LayerMapTryGet(entity, TailAnchor.Original, out var anchor, false))
        {
            anchor = _sprite.AddLayer(entity, new PrototypeLayerData { Visible = false }, layers[0].Index);
            _sprite.LayerMapSet(entity, TailAnchor.Original, anchor);
            // Inserting the bookmark shifted all captured tail indices by one.
            layers = layers.Select(entry => entry with { Index = entry.Index + 1 }).ToArray();
        }

        var destination = direction == RsiDirection.South ? 0 : anchor + 1;
        if (layers.Select((entry, i) => entry.Index == destination + i).All(matches => matches))
            return;

        foreach (var entry in layers.Reverse())
            _sprite.RemoveLayer(entity, entry.Index, out _, false);

        // Removing a foreground tail shifts the bookmark; resolve its current index again.
        if (direction != RsiDirection.South)
            destination = _sprite.LayerMapGet(entity, TailAnchor.Original) + 1;

        foreach (var entry in layers)
        {
            var index = _sprite.AddLayer(entity, entry.Layer, destination++);
            foreach (var key in entry.Keys)
            {
                if (key is Enum enumKey)
                    _sprite.LayerMapSet(entity, enumKey, index);
                else
                    _sprite.LayerMapSet(entity, (string)key, index);
            }
        }

    }
}
