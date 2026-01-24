using Content.Server.Item;
using Content.Server.Popups;
using Content.Shared._WL.Tray;
using Content.Shared.Destructible;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Throwing;
using Content.Shared.Verbs;
using Robust.Server.Audio;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Server._WL.Tray;

public sealed partial class TraySystem : SharedTraySystem
{
    [Dependency] private readonly ItemSystem _item = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly ThrowingSystem _throw = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private List<EntityUid> _itemsToThrow = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TrayComponent, ComponentInit>(OnInit);

        SubscribeLocalEvent<TrayComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<OnTrayComponent, EntParentChangedMessage>(OnItemParentChanged);
        SubscribeLocalEvent<OnTrayComponent, ComponentRemove>(OnItemComponentRemove);
        SubscribeLocalEvent<TrayComponent, GetVerbsEvent<AlternativeVerb>>(OnVerb);

        SubscribeLocalEvent<TrayComponent, LandEvent>(OnLand);
        SubscribeLocalEvent<TrayComponent, DestructionEventArgs>(OnDestruction);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var item in _itemsToThrow.ToArray())
        {
            _throw.TryThrow(item, _random.NextVector2());
            _itemsToThrow.Remove(item);
        }
    }

    private void OnInit(EntityUid uid, TrayComponent component, ComponentInit args)
    {
        if (!TryComp<ItemComponent>(uid, out var item))
            return;

        component.DefaultSize = item.Size;
    }

    private void OnInteractUsing(EntityUid uid, TrayComponent component, InteractUsingEvent args)
    {
        if (args.Handled || !TryComp<ItemComponent>(args.Used, out var item) || component.ConnectedEntities.ContainsKey(args.Used))
            return;

        if (component.Closed || _container.IsEntityInContainer(uid))
            return;

        if (_item.GetSizePrototype(item.Size) > _item.GetSizePrototype(component.MaxItemSize))
        {
            _popup.PopupEntity(Loc.GetString("tray-item-too-big"), args.Used, args.User);
            return;
        }

        if (component.ConnectedEntities.Count >= component.Capacity)
        {
            _popup.PopupEntity(Loc.GetString("tray-max-capacity"), args.Used, args.User);
            return;
        }

        PutItemOnTray(uid, component, args.Used, args.ClickLocation);

        args.Handled = true;
    }

    private void OnItemParentChanged(EntityUid uid, OnTrayComponent component, EntParentChangedMessage args)
    {
        if (args.Transform.ParentUid == component.TrayEntity || component.Storaged)
            return;

        if (!TryComp<TrayComponent>(component.TrayEntity, out var tray))
            return;

        RemoveItemFromTray(component.TrayEntity.Value, tray, uid);
    }

    private void OnVerb(EntityUid uid, TrayComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!component.HasCap)
            return;

        if (!args.CanInteract)
            return;

        var verb = new AlternativeVerb();
        if (component.Closed)
        {
            verb.Text = Loc.GetString("verb-tray-open");
            verb.Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/open.svg.192dpi.png"));
            verb.Act = () => OpenTray(uid, component);
        }
        else
        {
            verb.Text = Loc.GetString("verb-tray-close");
            verb.Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/close.svg.192dpi.png"));
            verb.Act = () => CloseTray(uid, component);
        }
        args.Verbs.Add(verb);
    }

    private void CloseTray(EntityUid uid, TrayComponent component)
    {
        if (component.Closed)
            return;

        var test = Transform(uid).Coordinates;

        foreach (var item in component.ConnectedEntities)
        {
            if (TryComp<OnTrayComponent>(item.Key, out var tray))
                tray.Storaged = true;

            _transform.DetachEntity(item.Key);
        }

        _transform.SetCoordinates(uid, test);

        component.Closed = true;
        _audio.PlayPvs(component.CloseSound, uid);
        UpdateVisuals(uid, component);
    }

    private void OpenTray(EntityUid uid, TrayComponent component)
    {
        if (!component.Closed)
            return;

        foreach (var item in component.ConnectedEntities)
        {
            _transform.SetCoordinates(item.Key, item.Value);

            if (TryComp<OnTrayComponent>(item.Key, out var tray))
                tray.Storaged = false;
        }

        component.Closed = false;
        _audio.PlayPvs(component.OpenSound, uid);
        UpdateVisuals(uid, component);
    }

    private void OnLand(EntityUid uid, TrayComponent component, ref LandEvent args)
    {
        if (component.Closed)
            return;

        ThrowItemsOnTray(uid, component);
    }

    private void OnDestruction(EntityUid uid, TrayComponent component, DestructionEventArgs args)
    {
        ThrowItemsOnTray(uid, component);
    }

    private void OnItemComponentRemove(EntityUid uid, OnTrayComponent component, ComponentRemove args)
    {
        if (component.LifeStage >= ComponentLifeStage.Stopping)
            return;

        if (!TryComp<TrayComponent>(component.TrayEntity, out var tray))
            return;

        RemoveItemFromTray(component.TrayEntity.Value, tray, uid);
    }

    public void ThrowItemsOnTray(EntityUid uid, TrayComponent component)
    {
        var itemsArray = component.ConnectedEntities.Keys.ToArray();

        foreach (var item in itemsArray)
        {
            RemoveItemFromTray(uid, component, item);
            _transform.SetCoordinates(item, Transform(uid).Coordinates);
            _itemsToThrow.Add(item);
        }
    }
}
