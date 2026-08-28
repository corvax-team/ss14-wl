using Content.Shared._WL.Clothing.Components;
using Content.Shared.Actions;
using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Clothing.Systems;

public sealed partial class AdjustableNeckwearLayerSystem : EntitySystem
{
    [Dependency] private SharedItemSystem _itemSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AdjustableNeckwearLayerComponent, GetEquipmentVisualsEvent>(OnGetVisuals);
        SubscribeLocalEvent<AdjustableNeckwearLayerComponent,
            InventoryRelayedEvent<GetVerbsEvent<EquipmentVerb>>>(OnGetRelayedVerbs);
        SubscribeLocalEvent<AdjustableNeckwearLayerComponent, GetVerbsEvent<EquipmentVerb>>(OnGetVerbs);
        SubscribeLocalEvent<AdjustableNeckwearLayerComponent, ToggleNeckwearLayerEvent>(OnToggleLayer);
        SubscribeLocalEvent<AdjustableNeckwearLayerComponent, AfterAutoHandleStateEvent>(OnAfterHandleState);
    }

    private static void OnGetVisuals(
        Entity<AdjustableNeckwearLayerComponent> entity,
        ref GetEquipmentVisualsEvent args)
    {
        if (entity.Comp.AboveOuterClothing)
            return;

        args.InsertionSlot = "outerClothing";
        args.InsertBeforeSlot = true;
    }

    private void OnGetRelayedVerbs(
        Entity<AdjustableNeckwearLayerComponent> entity,
        ref InventoryRelayedEvent<GetVerbsEvent<EquipmentVerb>> args)
    {
        AddVerb(entity, args.Args);
    }

    private void OnGetVerbs(
        Entity<AdjustableNeckwearLayerComponent> entity,
        ref GetVerbsEvent<EquipmentVerb> args)
    {
        AddVerb(entity, args);
    }

    private void AddVerb(
        Entity<AdjustableNeckwearLayerComponent> entity,
        GetVerbsEvent<EquipmentVerb> verbArgs)
    {
        if (!verbArgs.CanAccess || !verbArgs.CanInteract ||
            !TryComp(entity, out ClothingComponent? clothing) ||
            clothing.InSlot != "neck")
        {
            return;
        }

        var wearer = Transform(entity).ParentUid;
        if (verbArgs.User != wearer)
            return;

        verbArgs.Verbs.Add(new EquipmentVerb
        {
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/outfit.svg.192dpi.png")),
            Text = Loc.GetString(entity.Comp.AboveOuterClothing
                ? "adjustable-neckwear-layer-verb-below"
                : "adjustable-neckwear-layer-verb-above"),
            EventTarget = entity,
            ExecutionEventArgs = new ToggleNeckwearLayerEvent { Performer = verbArgs.User },
        });
    }

    private void OnToggleLayer(
        Entity<AdjustableNeckwearLayerComponent> entity,
        ref ToggleNeckwearLayerEvent args)
    {
        if (args.Handled ||
            !TryComp(entity, out ClothingComponent? clothing) ||
            clothing.InSlot != "neck" ||
            Transform(entity).ParentUid != args.Performer)
        {
            return;
        }

        entity.Comp.AboveOuterClothing = !entity.Comp.AboveOuterClothing;
        Dirty(entity);
        _itemSystem.VisualsChanged(entity);
        args.Handled = true;
    }

    private void OnAfterHandleState(
        Entity<AdjustableNeckwearLayerComponent> entity,
        ref AfterAutoHandleStateEvent args)
    {
        _itemSystem.VisualsChanged(entity);
    }
}

public sealed partial class ToggleNeckwearLayerEvent : InstantActionEvent;
