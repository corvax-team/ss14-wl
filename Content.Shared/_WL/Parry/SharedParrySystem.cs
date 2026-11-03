using Content.Shared.Input;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee;
using Content.Shared._WL.Weapons.Melee.Events;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage;

namespace Content.Shared._WL.Parry;

public abstract partial class SharedParrySystem : EntitySystem
{
    [Dependency] private SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    public override void Initialize()
    {
        base.Initialize();

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.Parry, InputCmdHandler.FromDelegate(enabled: InputParryPressed,
            disabled: InputParryReleased,
            handle: false,
            outsidePrediction: false))
            .Register<SharedParrySystem>();
    }

    public override void Shutdown()
    {
        base.Shutdown();

        CommandBinds.Unregister<SharedParrySystem>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ActiveParryComponent>();

        while (query.MoveNext(out var uid, out var activeParry))
        {
            if (!activeParry.IsParrying ||
                !TryComp<ParryComponent>(activeParry.Weapon, out var parry) ||
                _timing.CurTime - activeParry.ParryStartTime < parry.ParryDuration)
            {
                continue;
            }

            activeParry.IsParrying = false;
            Dirty(uid, activeParry);
        }
    }

    private void InputParryPressed(ICommonSession? session)
    {
        if (session is null || session.AttachedEntity is not { } user)
            return;

        if (!_melee.TryGetWeapon(user, out var weapon, out var _) || !HasComp<ParryComponent>(weapon))
            return;

        if (!TryStartParry(user, weapon))
            return;
    }

    private void InputParryReleased(ICommonSession? session)
    {
        if (session is null || session.AttachedEntity is not { } user)
            return;

        if (!TryComp<ActiveParryComponent>(user, out var activeParry))
            return;

        if (!TryComp<ParryComponent>(activeParry.Weapon, out var parry))
            return;

        parry.NextParryTime = _timing.CurTime + parry.ParryCooldown;

        RemComp<ActiveParryComponent>(user);

        _movementSpeed.RefreshMovementSpeedModifiers(user);
    }

    [SubscribeLocalEvent]
    private void OnRefreshMovementSpeed(Entity<ActiveParryComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!TryComp<ParryComponent>(ent.Comp.Weapon, out var parry))
            return;

        args.ModifySpeed(parry.MovementSpeedModifier);
    }

    [SubscribeLocalEvent]
    private void OnBeforeMeleeDamage(Entity<ActiveParryComponent> ent, ref BeforeMeleeDamageEvent args)
    {
        Log.Info($"BeforeMeleeDamageEvent: {args.User} -> {ent.Owner}, Damage: {args.Damage}");
        if (!TryComp<ParryComponent>(ent.Comp.Weapon, out var parry))
            return;
        Log.Info($"ParryComponent: {parry.MeleeDamageReduction}, {parry.ParryAngle}, {parry.ParryDuration}, {parry.ParryCooldown}, {parry.NextParryTime}, {parry.AngleOffset}, {parry.ForwardOffset}");

        var directionToAttacker = Angle.FromWorldVec(
            _transform.GetWorldPosition(args.User) -
            _transform.GetWorldPosition(ent.Owner));

        var angleDifference = (directionToAttacker - _transform.GetWorldRotation(ent.Owner)).Reduced();

        if (Math.Abs(angleDifference.Theta) > parry.ParryAngle.Theta / 2)
            return;

        //Log.Info($"Parry successful! Angle difference: {angleDifference.Theta}, Parry angle: {parry.ParryAngle.Theta}");

        if (ent.Comp.IsParrying)
        {
            _damageable.TryChangeDamage(args.User, args.Damage * 100f, true, true);
            args.Damage *= 0f;
            return;
        }

        args.Damage *= parry.MeleeDamageReduction;
        TryBreakParry(ent.Owner, ent.Comp);
    }

    private void TryBreakParry(EntityUid uid, ActiveParryComponent activeParry)
    {
        if (TryComp<ParryComponent>(activeParry.Weapon, out var parry))
        {
            parry.NextParryTime = _timing.CurTime + parry.ParryCooldown;
            Dirty(activeParry.Weapon, parry);
        }

        RemComp<ActiveParryComponent>(uid);
        _movementSpeed.RefreshMovementSpeedModifiers(uid);
    }

    private bool TryStartParry(EntityUid uid, EntityUid weapon)
    {
        if (!HasComp<ActiveParryComponent>(uid))
        {
            if (!TryComp<ParryComponent>(weapon, out var parry))
                return false;

            if (_timing.CurTime < parry.NextParryTime)
            {
                _popup.PopupEntity(Loc.GetString("parry-cooldown"), null, uid, uid);
                return false;
            }

            var activeParry = EnsureComp<ActiveParryComponent>(uid);

            activeParry.Weapon = weapon;
            activeParry.IsParrying = true;
            activeParry.ParryStartTime = _timing.CurTime;
            Dirty(uid, activeParry);

            _movementSpeed.RefreshMovementSpeedModifiers(uid);

            return true;
        }

        return false;
    }
}
