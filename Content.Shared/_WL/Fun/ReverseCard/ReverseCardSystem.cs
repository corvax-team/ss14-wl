using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Charges.Systems;
using Content.Shared.Charges.Components;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Robust.Shared.GameStates;
using Robust.Shared.Random;
using Robust.Shared.Network;

namespace Content.Shared.CombatStand;

public sealed partial class ReverseCardSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedChargesSystem _sharedCharges = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReverseCardComponent, ExaminedEvent>(OnExamineRequires);
        SubscribeLocalEvent<ReverseCardComponent, GotEquippedHandEvent>(OnHandEquipped);
        SubscribeLocalEvent<ReverseCardComponent, GotUnequippedHandEvent>(OnHandUnequipped);

        Subs.SubscribeWithRelay<ReverseCardComponent, ProjectileReflectAttemptEvent>(OnUserCollide, baseEvent: false);
        Subs.SubscribeWithRelay<ReverseCardComponent, HitScanReflectAttemptEvent>(OnUserHitscan, baseEvent: false);
    }

    private void OnUserCollide(Entity<ReverseCardComponent> ent, ref ProjectileReflectAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!ent.Comp.InRightPlace ||
                !ent.Comp.OnProjectile)
            return;

        if (args.Component.Weapon is not { } weapon)
            return;

        if (TryMakeSwap(ent, weapon))
        {
            QueueDel(args.ProjUid);
            args.Cancelled = true;
        }
    }

    private void OnUserHitscan(Entity<ReverseCardComponent> ent, ref HitScanReflectAttemptEvent args)
    {
        if (!ent.Comp.InRightPlace ||
                !ent.Comp.OnHitscan)
            return;

        if (TryMakeSwap(ent, args.SourceItem))
        {
            args.Reflected = true;
        }
    }

    private void OnHandEquipped(Entity<ReverseCardComponent> ent, ref GotEquippedHandEvent args)
    {
        ent.Comp.InRightPlace = ent.Comp.SwapInHands;
        Dirty(ent);
    }

    private void OnHandUnequipped(Entity<ReverseCardComponent> ent, ref GotUnequippedHandEvent args)
    {
        ent.Comp.InRightPlace = false;
        Dirty(ent);
    }

    private bool TryMakeSwap(Entity<ReverseCardComponent> card, EntityUid weapon)
    {
        if (!_random.Prob(card.Comp.Probability) ||
                !HasComp<ItemComponent>(weapon))
            return false;

        if (TryComp<LimitedChargesComponent>(card.Owner, out var charges)
            && !_sharedCharges.TryUseCharge((card.Owner, charges)))
            return false;

        if (card.Comp.ReverseTargetMessage is not null)
        {
            _popup.PopupCoordinates(Loc.GetString(card.Comp.ReverseTargetMessage),
                Transform(weapon).Coordinates,
                PopupType.MediumCaution);
        }

        if (_net.IsServer)
        {
            _audio.PlayPvs(card.Comp.SoundOnSwapWeapon, Transform(weapon).Coordinates);
            _audio.PlayPvs(card.Comp.SoundOnSwapCard, Transform(card).Coordinates);
        }

        return _transform.SwapPositions(card.Owner, weapon);
    }

    private void OnExamineRequires(Entity<ReverseCardComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.CanBeExamined && ent.Comp.ExamineMessage != null)
            args.PushText(Loc.GetString(ent.Comp.ExamineMessage));
    }

}


[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ReverseCardComponent : Component
{
    [DataField]
    public LocId? ExamineMessage = "reverse-card-examine";

    [DataField]
    public LocId? ReverseTargetMessage = "reverse-card-target-popup";

    [DataField]
    public bool CanBeExamined = true;

    [DataField]
    public bool OnMelee = true;

    [DataField]
    public bool OnProjectile = true;

    [DataField]
    public bool OnHitscan = true;

    [DataField]
    public bool SwapInHands = true;

    [DataField]
    public bool SwapInInventory = true;

    [DataField, AutoNetworkedField]
    public SoundSpecifier SoundOnSwapCard = new SoundPathSpecifier("/Audio/Weapons/Guns/Hits/laser_sear_wall.ogg")
    {
        Params = AudioParams.Default.WithVariation(0.05f)
    };

    [DataField, AutoNetworkedField]
    public SoundSpecifier SoundOnSwapWeapon = new SoundPathSpecifier("/Audio/Weapons/flash.ogg")
    {
        Params = AudioParams.Default.WithVolume(2f).WithMaxDistance(3f)
    };

    [DataField, AutoNetworkedField]
    public float Probability = 1f;

    [DataField]
    public bool InRightPlace = false;
}

