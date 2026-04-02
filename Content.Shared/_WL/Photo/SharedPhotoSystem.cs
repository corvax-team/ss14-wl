using Content.Shared._WL.Photo.Filters;
using Content.Shared.ActionBlocker;
using Content.Shared.Alert;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Examine;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Materials;
using Content.Shared.Movement.Events;
using Content.Shared.Popups;
using Content.Shared.UserInterface;

namespace Content.Shared._WL.Photo;

public abstract partial class SharedPhotoSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlockerSystem = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedMaterialStorageSystem _material = default!;
    [Dependency] private readonly ItemSlotsSystem _slots = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhotoCameraComponent, ActivatableUIOpenAttemptEvent>(OnOpenCameraInterfaceAttempt);
        SubscribeLocalEvent<PhotoCameraComponent, AfterActivatableUIOpenEvent>(OnOpenCameraInterface);

        SubscribeLocalEvent<PhotoCameraComponent, ExaminedEvent>(OnCameraExamined);

        SubscribeLocalEvent<PhotoCameraComponent, ItemSlotInsertAttemptEvent>(OnFilterInsertAttempt);

        SubscribeLocalEvent<PhotoCameraUserComponent, UpdateCanMoveEvent>(HandleMovementBlock);
        SubscribeLocalEvent<PhotoCameraUserComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<PhotoCameraUserComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnOpenCameraInterfaceAttempt(EntityUid uid, PhotoCameraComponent component, ActivatableUIOpenAttemptEvent args)
    {
        if (component.User != null)
        {
            args.Cancel();
            return;
        }

        if (!_hands.IsHolding(args.User, uid))
        {
            _popup.PopupEntity(Loc.GetString("photo-camera-not-holding"), uid, args.User);

            args.Cancel();
            return;
        }
    }

    private void OnOpenCameraInterface(EntityUid uid, PhotoCameraComponent component, AfterActivatableUIOpenEvent args)
    {
        UpdateCameraInterface(uid, component);

        component.User = args.User;
        EnsureComp<PhotoCameraUserComponent>(args.User);
    }

    protected virtual void UpdateCameraInterface(EntityUid uid, PhotoCameraComponent component, EntityUid? player = null)
    {
        bool hasPaper = _material.CanChangeMaterialAmount(uid, component.CardMaterial, -component.CardCost);

        var state = new PhotoCameraUiState(GetNetEntity(uid), hasPaper);
        _userInterface.SetUiState(uid, PhotoCameraUiKey.Key, state);
    }

    protected virtual void OnShutdown(EntityUid uid, PhotoCameraUserComponent component, ComponentShutdown args)
    {
        _actionBlockerSystem.UpdateCanMove(uid);
        _alerts.ClearAlert(uid, component.AlertPrototype);
    }

    private void OnStartup(EntityUid uid, PhotoCameraUserComponent component, ComponentStartup args)
    {
        _actionBlockerSystem.UpdateCanMove(uid);
        _alerts.ShowAlert(uid, component.AlertPrototype);
    }

    private void HandleMovementBlock(EntityUid uid, PhotoCameraUserComponent component, UpdateCanMoveEvent args)
    {
        if (component.LifeStage > ComponentLifeStage.Running)
            return;

        args.Cancel();
    }

    private void OnCameraExamined(EntityUid uid, PhotoCameraComponent component, ExaminedEvent args)
    {
        int paperLeft = (int)MathF.Ceiling(_material.GetMaterialAmount(uid, component.CardMaterial) / component.CardCost);
        string message = Loc.GetString("photo-camera-examined-paper-left", ("count", paperLeft));

        args.PushMarkup(message);
    }

    private void OnFilterInsertAttempt(EntityUid uid, PhotoCameraComponent component, ItemSlotInsertAttemptEvent args)
    {
        if (args.Slot.ID != component.FilterSlot)
            return;

        if (!HasComp<PhotoCameraFilterComponent>(args.Item))
            args.Cancelled = true;
    }
}
