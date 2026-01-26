using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Map;

namespace Content.Shared._WL.Tray;

public abstract class SharedTraySystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public void UpdateVisuals(EntityUid uid, TrayComponent component)
    {
        _appearance.SetData(uid, TrayVisualState.HasEntities, component.ConnectedEntities.Count > 0);
        _appearance.SetData(uid, TrayVisualState.Closed, component.Closed);
    }

    public void UpdateSize(EntityUid uid, TrayComponent component)
    {
        if (component.ConnectedEntities.Count > 0)
            _item.SetSize(uid, component.FilledSize);
        else
            _item.SetSize(uid, component.DefaultSize);
    }

    public void PutItemOnTray(EntityUid uid, TrayComponent component, EntityUid item, EntityCoordinates position)
    {
        var offsetCoordinates = _transform.ToCoordinates(uid, _transform.ToMapCoordinates(position));
        _transform.SetCoordinates(item, offsetCoordinates);
        _transform.SetLocalRotation(item, 0);

        component.ConnectedEntities.Add(item, offsetCoordinates);

        var onTray = EnsureComp<OnTrayComponent>(item);
        onTray.TrayEntity = uid;

        UpdateVisuals(uid, component);
        UpdateSize(uid, component);
    }

    public void RemoveItemFromTray(EntityUid uid, TrayComponent component, EntityUid item)
    {
        component.ConnectedEntities.Remove(item);

        UpdateVisuals(uid, component);
        UpdateSize(uid, component);

        RemComp<OnTrayComponent>(item);
    }
}
