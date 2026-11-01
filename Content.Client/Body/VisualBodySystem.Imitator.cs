using Content.Shared._WL.Implants;
using Content.Shared.Body;
using Content.Shared.Humanoid;
using System.Linq;
using Robust.Client.GameObjects;

namespace Content.Client.Body;

public sealed partial class VisualBodySystem
{
    private readonly HashSet<EntityUid> _imitatorDirty = new();
    private readonly Dictionary<EntityUid, RenderedImitation> _imitatorRendered = new();
    private bool _drawingImitator;

    private sealed record RenderedImitation(
        EntityUid Image,
        List<Entity<VisualOrganComponent>> Visuals,
        List<Entity<VisualOrganMarkingsComponent>> Markings);

    private void InitializeImitator()
    {
        SubscribeLocalEvent<ImitatorDisguiseComponent, AfterAutoHandleStateEvent>(OnImitatorState);
        SubscribeLocalEvent<ImitatorDisguiseComponent, ComponentShutdown>(OnImitatorShutdown);
        SubscribeLocalEvent<ImitatorImageComponent, AfterAutoHandleStateEvent>(OnImageState);
    }

    private void OnImitatorState(Entity<ImitatorDisguiseComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!_imitatorRendered.TryGetValue(ent, out var rendered) || rendered.Image != ent.Comp.Image)
            _imitatorDirty.Add(ent);
        EntityManager.System<Content.Client._WL.Implants.ImitatorDisguiseSystem>().RefreshShader(ent);
    }

    private void OnImitatorShutdown(Entity<ImitatorDisguiseComponent> ent, ref ComponentShutdown args)
    {
        _imitatorDirty.Add(ent);
        EntityManager.System<Content.Client._WL.Implants.ImitatorDisguiseSystem>().ClearShader(ent);
    }

    private void OnImageState(Entity<ImitatorImageComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (ent.Comp.Wearer is { } wearer)
            _imitatorDirty.Add(wearer);
    }

    private bool CanDrawImitatorTarget(EntityUid target)
    {
        if (_drawingImitator)
            return true;

        if (TryComp<ImitatorDisguiseComponent>(target, out _))
        {
            _imitatorDirty.Add(target);
            return false;
        }

        if (!TryComp<ImitatorImageComponent>(target, out var image))
            return true;

        if (image.Wearer is { } wearer)
            _imitatorDirty.Add(wearer);
        return false;
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);
        foreach (var wearer in _imitatorDirty.ToArray())
        {
            _imitatorDirty.Remove(wearer);
            RefreshImitator(wearer);
        }
    }

    private void RefreshImitator(EntityUid wearer)
    {
        if (TerminatingOrDeleted(wearer) || !HasComp<SpriteComponent>(wearer))
        {
            _imitatorRendered.Remove(wearer);
            return;
        }

        var source = wearer;
        if (TryComp<ImitatorDisguiseComponent>(wearer, out var disguise) && !disguise.Deleted)
        {
            if (disguise.Image is not { } image || !HasComp<BodyComponent>(image))
            {
                _imitatorDirty.Add(wearer);
                return;
            }
            source = image;
        }

        _drawingImitator = true;
        try
        {
            if (_imitatorRendered.Remove(wearer, out var previous))
            {
                foreach (var visual in previous.Visuals)
                    RemoveVisual(visual, wearer);
                foreach (var markings in previous.Markings)
                    RemoveMarkings(markings, wearer);
            }

            var body = Comp<BodyComponent>(wearer);
            foreach (var organ in body.Organs?.ContainedEntities ?? [])
            {
                if (TryComp<VisualOrganComponent>(organ, out var visual))
                    RemoveVisual((organ, visual), wearer);
                if (TryComp<VisualOrganMarkingsComponent>(organ, out var markings))
                    RemoveMarkings((organ, markings), wearer);
            }

            var visuals = new List<Entity<VisualOrganComponent>>();
            var markingSets = new List<Entity<VisualOrganMarkingsComponent>>();
            foreach (var organ in Comp<BodyComponent>(source).Organs?.ContainedEntities ?? [])
            {
                if (TryComp<VisualOrganComponent>(organ, out var visual))
                {
                    ApplyVisual((organ, visual), wearer);
                    visuals.Add((organ, visual));
                }
                if (TryComp<VisualOrganMarkingsComponent>(organ, out var markings))
                {
                    ApplyMarkings((organ, markings), wearer);
                    markingSets.Add((organ, markings));
                }
            }
            if (source != wearer)
                _imitatorRendered[wearer] = new(source, visuals, markingSets);

            EntityManager.System<Content.Client.Clothing.ClientClothingSystem>().RefreshImitatorClothing(wearer);
            EntityManager.System<Content.Client.Damage.DamageVisualsSystem>().RefreshImitator(wearer, source);

            if (TryComp<HideableHumanoidLayersComponent>(wearer, out var hidden))
            {
                foreach (var layer in hidden.HiddenLayers.Keys)
                {
                    var visibility = new BodyRelayedEvent<HumanoidLayerVisibilityChangedEvent>(
                        (wearer, body), new(layer, false));
                    foreach (var markings in markingSets)
                        OnMarkingsChangedVisibility(markings, ref visibility);
                    if (visibility.Args.ShouldHide && _sprite.LayerMapTryGet(wearer, layer, out var index, false))
                        _sprite.LayerSetVisible(wearer, index, false);
                }
            }
        }
        finally
        {
            _drawingImitator = false;
        }
    }

    private bool RelayImitatorVisibility(ref BodyRelayedEvent<HumanoidLayerVisibilityChangedEvent> args)
    {
        if (_drawingImitator || !TryComp<ImitatorDisguiseComponent>(args.Body, out var disguise) ||
            disguise.Image is not { } image || !TryComp<BodyComponent>(image, out var body))
            return false;

        _drawingImitator = true;
        try
        {
            foreach (var organ in body.Organs?.ContainedEntities ?? [])
            {
                if (TryComp<VisualOrganMarkingsComponent>(organ, out var markings))
                    OnMarkingsChangedVisibility((organ, markings), ref args);
            }
        }
        finally
        {
            _drawingImitator = false;
        }
        return true;
    }
}
