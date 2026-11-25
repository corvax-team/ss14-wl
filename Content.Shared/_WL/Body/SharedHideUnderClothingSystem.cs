using Content.Shared.Actions;
using Content.Shared.Body;
using Content.Shared.Clothing.Components;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;

namespace Content.Shared._WL.Body;

public abstract partial class SharedHideUnderClothingSystem : EntitySystem
{
    [Dependency] protected BodySystem Body = default!;
    [Dependency] protected SharedActionsSystem Actions = default!;
    [Dependency] private MarkingManager _markings = default!;
    [Dependency] private InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HideUnderClothingComponent, ToggleMarkingsUnderClothingEvent>(OnToggle);
        SubscribeLocalEvent<HideUnderClothingComponent, DidEquipEvent>(OnEquipped);
        SubscribeLocalEvent<HideUnderClothingComponent, DidUnequipEvent>(OnUnequipped);
    }

    protected bool HasEligibleMarkings(EntityUid body)
    {
        foreach (var organ in Body.EnumerateOrgans<VisualOrganMarkingsComponent>(body))
        foreach (var markings in organ.Comp2.Markings.Values)
        foreach (var marking in markings)
        {
            if (_markings.TryGetMarking(marking, out var proto) && proto.CanHideUnderClothing)
                return true;
        }
        return false;
    }

    private bool HasClothing(EntityUid body, string slot) =>
        _inventory.TryGetSlotEntity(body, slot, out var clothing) && HasComp<ClothingComponent>(clothing);

    private (bool Ears, bool Tail) GetAvailableMarkings(EntityUid body)
    {
        var ears = false;
        var tail = false;
        foreach (var organ in Body.EnumerateOrgans<VisualOrganMarkingsComponent>(body))
        foreach (var markings in organ.Comp2.Markings.Values)
        foreach (var marking in markings)
        {
            if (!_markings.TryGetMarking(marking, out var proto) || !proto.CanHideUnderClothing)
                continue;
            ears |= proto.BodyPart == HumanoidVisualLayers.HeadTop;
            tail |= proto.BodyPart is HumanoidVisualLayers.Tail or HumanoidVisualLayers.TailOverlay;
        }
        return (ears && HasClothing(body, "head"), tail && HasClothing(body, "jumpsuit"));
    }

    private void OnEquipped(Entity<HideUnderClothingComponent> ent, ref DidEquipEvent args)
    {
        if (args.Slot is "jumpsuit" or "head")
            UpdateClothing(ent);
    }

    private void OnUnequipped(Entity<HideUnderClothingComponent> ent, ref DidUnequipEvent args)
    {
        if (args.Slot is "jumpsuit" or "head")
            UpdateClothing(ent);
    }

    protected void UpdateClothing(Entity<HideUnderClothingComponent> ent)
    {
        var available = GetAvailableMarkings(ent);
        SetHidden(ent, ent.Comp.Hidden);
        Actions.SetEnabled(ent.Comp.Action, available.Ears || available.Tail);
    }

    private void OnToggle(Entity<HideUnderClothingComponent> ent, ref ToggleMarkingsUnderClothingEvent args)
    {
        var available = GetAvailableMarkings(ent);
        if (args.Handled || !(available.Ears || available.Tail))
            return;
        args.Handled = true;
        SetHidden(ent, !ent.Comp.Hidden);
    }

    protected void SetHidden(Entity<HideUnderClothingComponent> ent, bool hidden)
    {
        var available = GetAvailableMarkings(ent);
        hidden &= available.Ears || available.Tail;
        var earsHidden = hidden && available.Ears;
        var tailHidden = hidden && available.Tail;
        if (ent.Comp.Hidden == hidden && ent.Comp.EarsHidden == earsHidden && ent.Comp.TailHidden == tailHidden)
            return;
        ent.Comp.Hidden = hidden;
        ent.Comp.EarsHidden = earsHidden;
        ent.Comp.TailHidden = tailHidden;
        Dirty(ent);
        Actions.SetToggled(ent.Comp.Action, hidden);
        var ev = new MarkingsUnderClothingChangedEvent();
        RaiseLocalEvent(ent, ref ev);
    }
}
