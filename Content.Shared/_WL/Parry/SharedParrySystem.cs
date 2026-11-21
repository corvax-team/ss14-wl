using Content.Shared._WL.Weapons.Melee.Events;
using Content.Shared.GameTicking;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Audio;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Shared._WL.Parry;

public abstract partial class SharedParrySystem : EntitySystem
{
    [Dependency] private SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private StatusEffectsSystem _statusEffects = default!;

    private readonly List<PendingHit> _pending = new();
    private readonly List<PendingHit> _scratch = new();

    private readonly record struct PendingHit(
        EntityUid Target,
        EntityUid Attacker,
        EntityUid Weapon,
        DamageSpecifier Damage,
        bool ResistanceBypass,
        SoundSpecifier? HitSoundOverride,
        bool Heavy,
        Vector2 AttackerPos,
        Vector2 TargetPos,
        TimeSpan ExpiresAt);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<ParryStartEvent>(OnParryStart);
        SubscribeAllEvent<ParryStopEvent>(OnParryStop);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(_ => _pending.Clear());
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

        if (_net.IsServer)
            FlushExpiredHits();
    }

    #region Input

    private void OnParryStart(ParryStartEvent ev, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } user)
            return;

        if (_mobState.IsIncapacitated(user))
            return;

        if (!_melee.TryGetWeapon(user, out var weapon, out _) || !HasComp<ParryComponent>(weapon))
            return;

        if (!TryStartParry(user, weapon))
            return;

        if (_net.IsServer && TryComp<ActiveParryComponent>(user, out var active))
            ResolvePendingHits((user, active));
    }

    private void OnParryStop(ParryStopEvent ev, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } user)
            return;

        if (!TryComp<ActiveParryComponent>(user, out var activeParry))
            return;

        if (!TryComp<ParryComponent>(activeParry.Weapon, out var parry))
            return;

        parry.NextParryTime = _timing.CurTime + parry.ParryCooldown;

        RemComp<ActiveParryComponent>(user);

        _movementSpeed.RefreshMovementSpeedModifiers(user);
    }

    #endregion

    #region Parry state

    [SubscribeLocalEvent]
    private void OnRefreshMovementSpeed(Entity<ActiveParryComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!TryComp<ParryComponent>(ent.Comp.Weapon, out var parry))
            return;

        args.ModifySpeed(parry.MovementSpeedModifier);
    }

    private bool TryStartParry(EntityUid uid, EntityUid weapon)
    {
        if (HasComp<ActiveParryComponent>(uid))
            return false;

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

    #endregion

    #region Melee damage

    [SubscribeLocalEvent]
    private void OnBeforeMeleeDamage(Entity<ActiveParryComponent> ent, ref BeforeMeleeDamageEvent args)
    {
        if (args.Deferred)
            return;

        if (TryParry(ent,
                args.User,
                _transform.GetWorldPosition(args.User),
                _transform.GetWorldPosition(ent.Owner),
                args.Damage,
                out var result))
        {
            args.Damage = result;
        }
    }

    [SubscribeLocalEvent]
    private void OnBeforeMeleeDamageDefer(Entity<ActorComponent> ent, ref BeforeMeleeDamageEvent args)
    {
        if (!_net.IsServer ||
            args.Deferred ||
            HasComp<ActiveParryComponent>(ent) ||
            HasComp<ActorComponent>(args.User) || // player vs player is already ordered by client ticks
            _mobState.IsIncapacitated(ent))
        {
            return;
        }

        if (!_melee.TryGetWeapon(ent, out var weapon, out _) ||
            !TryComp<ParryComponent>(weapon, out var parry) ||
            _timing.CurTime < parry.NextParryTime)
        {
            return;
        }

        var ping = TimeSpan.FromMilliseconds(ent.Comp.PlayerSession.Channel.Ping);
        var window = ping + _timing.TickPeriod * 2;
        window = TimeSpan.FromTicks(Math.Clamp(window.Ticks, parry.MinCompensation.Ticks, parry.MaxCompensation.Ticks));

        _pending.Add(new PendingHit(
            ent.Owner,
            args.User,
            args.Weapon,
            new DamageSpecifier(args.Damage),
            args.ResistanceBypass,
            args.HitSoundOverride,
            args.Heavy,
            _transform.GetWorldPosition(args.User),
            _transform.GetWorldPosition(ent.Owner),
            _timing.CurTime + window));

        args.Deferred = true;
    }

    private bool TryParry(
        Entity<ActiveParryComponent> ent,
        EntityUid attacker,
        Vector2 attackerPos,
        Vector2 targetPos,
        DamageSpecifier damage,
        out DamageSpecifier result)
    {
        result = damage;

        if (!TryComp<ParryComponent>(ent.Comp.Weapon, out var parry))
            return false;

        var directionToAttacker = Angle.FromWorldVec(attackerPos - targetPos);
        var angleDifference = (directionToAttacker - _transform.GetWorldRotation(ent.Owner)).Reduced();

        if (Math.Abs(angleDifference.Theta) > parry.ParryAngle.Theta / 2)
            return false;

        result = ent.Comp.IsParrying ? damage * 0f : damage * parry.MeleeDamageReduction;

        if (ent.Comp.IsParrying)
        {
            _statusEffects.TryUpdateStatusEffectDuration(attacker, parry.ParriedStatusEffect, parry.AttackProhibitionDuration);
        }

        TryBreakParry(ent.Owner, ent.Comp);

        return true;
    }

    #endregion

    #region Deferred hits

    private void ResolvePendingHits(Entity<ActiveParryComponent> ent)
    {
        _scratch.Clear();

        for (var i = 0; i < _pending.Count; i++)
        {
            if (_pending[i].Target == ent.Owner)
                _scratch.Add(_pending[i]);
        }

        if (_scratch.Count == 0)
            return;

        _pending.RemoveAll(h => h.Target == ent.Owner);

        var hits = _scratch.ToArray();

        foreach (var hit in hits)
        {
            var damage = hit.Damage;

            if (TryComp<ActiveParryComponent>(ent.Owner, out var active))
                TryParry((ent.Owner, active), hit.Attacker, hit.AttackerPos, hit.TargetPos, hit.Damage, out damage);

            ApplyPending(hit, damage);
        }
    }

    private void FlushExpiredHits()
    {
        if (_pending.Count == 0)
            return;

        var now = _timing.CurTime;
        _scratch.Clear();

        for (var i = 0; i < _pending.Count; i++)
        {
            if (_pending[i].ExpiresAt <= now)
                _scratch.Add(_pending[i]);
        }

        if (_scratch.Count == 0)
            return;

        _pending.RemoveAll(h => h.ExpiresAt <= now);

        var hits = _scratch.ToArray();

        foreach (var hit in hits)
        {
            ApplyPending(hit, hit.Damage);
        }
    }

    private void ApplyPending(in PendingHit hit, DamageSpecifier damage)
    {
        _melee.ApplyDeferredMeleeHit(
            hit.Target,
            hit.Attacker,
            hit.Weapon,
            damage,
            hit.ResistanceBypass,
            hit.HitSoundOverride,
            hit.Heavy);
    }

    #endregion
}
