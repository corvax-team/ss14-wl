using System.Linq;
using Content.Client.Damage;
using Content.Client.Movement.Systems;
using Content.Client.MouseRotator;
using Content.Shared._WL.Body;
using Content.Shared.Body;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Utility;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Utility;

namespace Content.Client._WL.Body;

public sealed partial class DirectionalBodyLayersSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private TransformSystem _transform = default!;
    [Dependency] private IEyeManager _eye = default!;
    [Dependency] private BodySystem _body = default!;
    [Dependency] private MarkingManager _markings = default!;

    private readonly Dictionary<EntityUid, OrderState> _applied = new();

    public override void Initialize()
    {
        base.Initialize();
        UpdatesAfter.Add(typeof(TransformSystem));
        UpdatesAfter.Add(typeof(EyeSystem));
        UpdatesAfter.Add(typeof(ContentEyeSystem));
        UpdatesAfter.Add(typeof(MouseRotatorSystem));
        SubscribeLocalEvent<DirectionalBodyLayersComponent, ComponentShutdown>((uid, _, _) => _applied.Remove(uid));
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);
        var query = EntityQueryEnumerator<DirectionalBodyLayersComponent, SpriteComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var order, out var sprite, out var transform))
        {
            var direction = sprite.EnableDirectionOverride
                ? sprite.DirectionOverride.Convert(RsiDirectionType.Dir4)
                : SpriteComponent.Layer.GetDirection(RsiDirectionType.Dir4,
                    (_transform.GetRenderWorldRotation((uid, transform)) + _eye.CurrentEye.Rotation).Reduced().FlipPositive());
            ApplyOrder((uid, sprite), order, direction);
        }
    }

    /// SpriteView can render a direction independently of the world's camera and transform.
    public void ApplyDirection(EntityUid entity, Direction direction)
    {
        if (TryComp<DirectionalBodyLayersComponent>(entity, out var order) &&
            TryComp<SpriteComponent>(entity, out var sprite))
            ApplyOrder((entity, sprite), order, direction.Convert(RsiDirectionType.Dir4));
    }

    private void ApplyOrder(Entity<SpriteComponent?> entity, DirectionalBodyLayersComponent component, RsiDirection direction)
    {
        var configured = component.Directions.FirstOrDefault(pair => pair.Key.Convert(RsiDirectionType.Dir4) == direction).Value;
        if (configured == null || configured.Count < 2)
            return;

        if (_applied.TryGetValue(entity.Owner, out var previous) && previous.Direction == direction &&
            configured.SequenceEqual(previous.Configuration) && entity.Comp!.AllLayers.SequenceEqual(previous.Layers))
            return;

        var groups = configured.Distinct().ToDictionary(layer => layer, layer => new List<object>
        {
            layer, $"{layer}-displacement",
        });
        if (TryComp<DamageVisualsComponent>(entity.Owner, out var damageVisuals) &&
            damageVisuals.DamageOverlayGroups is { } overlays)
        {
            foreach (var (layer, keys) in groups)
            {
                foreach (var group in overlays.Keys)
                {
                    var key = $"{layer}{group}";
                    keys.Add(key);
                    keys.Add($"{key}-displacement");
                }
            }
        }
        foreach (var organ in _body.EnumerateOrgans<VisualOrganMarkingsComponent>(entity.Owner))
        {
            foreach (var marking in organ.Comp2.AppliedMarkings)
            {
                if (!_markings.TryGetMarking(marking, out var prototype) ||
                    !groups.TryGetValue(prototype.BodyPart, out var keys))
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

        var ordered = new List<CapturedLayer>();
        var captured = new Dictionary<SpriteComponent.Layer, CapturedLayer>();
        foreach (var keys in groups.Values)
        {
            var bundle = new List<CapturedLayer>();
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
                    bundle.Add(entry);
                }
                entry.Keys.Add(key);
            }
            ordered.AddRange(bundle.OrderBy(entry => entry.Index));
        }

        var original = captured.Values.OrderBy(entry => entry.Index).ToArray();
        // Keep all unmanaged layers (especially clothing anchors) at their existing indices.
        // Reuse Layer objects so visibility, colour, shaders and animation state survive the move.
        if (!original.SequenceEqual(ordered))
        {
            foreach (var entry in original.Reverse())
                _sprite.RemoveLayer(entity, entry.Index, out _, false);
            for (var i = 0; i < ordered.Count; i++)
            {
                var entry = ordered[i];
                var index = _sprite.AddLayer(entity, entry.Layer, original[i].Index);
                foreach (var key in entry.Keys)
                {
                    if (key is Enum enumKey)
                        _sprite.LayerMapSet(entity, enumKey, index);
                    else
                        _sprite.LayerMapSet(entity, (string)key, index);
                }
            }
        }
        _applied[entity.Owner] = new OrderState(direction, configured.ToArray(), entity.Comp!.AllLayers.ToArray());
    }

    private sealed record OrderState(RsiDirection Direction, HumanoidVisualLayers[] Configuration, ISpriteLayer[] Layers);
    private sealed record CapturedLayer(int Index, SpriteComponent.Layer Layer, List<object> Keys);
}
