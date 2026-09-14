using Content.Shared.Cuffs.Components;
using Content.Shared.Inventory;
using Content.Shared.Speech.Components;
using Content.Shared.Weapons.Melee;

namespace Content.Shared._WL.Weapons.Melee;

public sealed partial class CuffedMeleeWeaponSystem : EntitySystem
{
    [Dependency] private InventorySystem _inventorySystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CuffedMeleeWeaponComponent, GetMeleeWeaponEvent>(OnGetWeapon);
        SubscribeLocalEvent<CuffedMeleeWeaponComponent, CuffedStateChangeEvent>(OnCuffedChange);
    }

    private void OnCuffedChange(EntityUid uid, CuffedMeleeWeaponComponent comp, ref CuffedStateChangeEvent args)
    {
        if (!TryComp<CuffableComponent>(uid, out var cuffs))
            return;

        if (!cuffs.CanStillInteract && comp.WeaponUid == null)
        {
            if (_inventorySystem.TryGetSlotEntity(uid, "mask", out var mask)
                && HasComp<EmoteBlockerComponent>(mask.Value))
                return;

            comp.WeaponUid = PredictedSpawnAttachedTo(comp.WeaponId, Transform(uid).Coordinates);
            Dirty(uid, comp);
        }
        else if (cuffs.CanStillInteract && comp.WeaponUid != null)
        {
            QueueDel(comp.WeaponUid);

            comp.WeaponUid = null;
            Dirty(uid, comp);
        }
    }

    private void OnGetWeapon(EntityUid uid, CuffedMeleeWeaponComponent comp, GetMeleeWeaponEvent args)
    {
        if (!TryComp<CuffableComponent>(uid, out var cuffComp)
            || cuffComp.CanStillInteract)
            return;

        if (_inventorySystem.TryGetSlotEntity(uid, "mask", out var mask)
            && HasComp<EmoteBlockerComponent>(mask.Value))
            return;

        if (comp.WeaponUid == null)
            return;

        args.Weapon = comp.WeaponUid;
        args.Handled = true;
    }
}
