using Content.Shared._WL.CombatIndicator;
using Content.Shared.Administration.Logs;
using Content.Shared.CombatMode;
using Content.Shared.Database;
using Content.Shared.Damage.Systems;
using Content.Shared.Input;
using Content.Shared.Interaction;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Hands.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Dynamics.Joints;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._WL.Combat;

public abstract partial class SharedAdvancedCombatSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedCombatModeSystem _combatMode = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedStaminaSystem _stamina = default!;
    [Dependency] private PullingSystem _pull = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AdvancedCombatComponent, PullStoppedMessage>(OnPullStopped);
    }

    [SubscribeLocalEvent]
    private void OnCombatIndicatorChanged(Entity<AdvancedCombatComponent> ent, ref CombatIndicatorChangedEvent args)
    {
        if (!ent.Comp.EnableByIndicator)
            return;

        ent.Comp.InCombat = args.Enabled;
    }

    [SubscribeLocalEvent]
    private void OnCombatIndicatorChanged(Entity<AdvancedCombatComponent> ent, ref AttemptMobCollideEvent args)
    {
        if (!ent.Comp.InCombat)
            return;

        args.Cancelled = true;
    }

    private void OnPullStopped(EntityUid uid, AdvancedCombatComponent comp, PullStoppedMessage args) =>
        FreeFromGrab(args.PulledUid, args.PullerUid);

    [SubscribeLocalEvent]
    private void OnThrow(Entity<AdvancedCombatComponent> ent, ref BeforeThrowEvent args)
    {
        if (!TryComp<VirtualItemComponent>(args.ItemUid, out var virtualComp) ||
                !HasComp<AdvancedCombatComponent>(virtualComp.BlockingEntity) ||
                !ent.Comp.ThrowableStages.Contains(ent.Comp.GrabStage))
            return;

        args.ItemUid = virtualComp.BlockingEntity;
        args.ThrowSpeed = ent.Comp.ThrowSpeed;
    }

    [SubscribeLocalEvent]
    private void OnCombatAction(Entity<AdvancedCombatComponent> ent, ref CombatActionEvent args)
    {
        if (_mobState.IsIncapacitated(ent) ||
                Deleted(args.Target) ||
                !_combatMode.IsInCombatMode(ent))
            return;

        if (args.Target is not {} target)
            return;

        if (TryComp<AdvancedCombatComponent>(target, out var advTarget) &&
                target != ent.Owner)
        {
            switch (args.Intent)
            {
                case CombatIntent.Disarm:
                    DoDisarmAttack(ent, (target, advTarget));
                    break;
                case CombatIntent.Grab:
                    DoGrabAttack(ent, (target, advTarget));
                    break;
            }
        }

        args.Handle();
    }

    private float GetActionChance(EntityUid uid, CombatAction action, AdvancedCombatComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return 0f;

        if (comp.ActionsChances.TryGetValue(action, out var value))
            return value;

        return 0f;
    }

    private float GetResistChance(EntityUid uid, CombatResist action, AdvancedCombatComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return 0f;

        if (comp.InCombat && comp.CombatResistChances.TryGetValue(action, out var combatValue))
            return combatValue;

        if (comp.BaseResistChances.TryGetValue(action, out var value))
            return value;

        return 0f;
    }

    private float GetSummarizeChance(EntityUid source, EntityUid target, CombatAction action, CombatResist resist)
    {
        return GetActionChance(source, action) * (1 - GetResistChance(target, resist));
    }

    private void DoDisarmAttack(Entity<AdvancedCombatComponent> ent, Entity<AdvancedCombatComponent> target)
    {
        var success = false;

        _adminLogger.Add(LogType.DisarmedAction, $"{ToPrettyString(ent):ent} used disarm on {ToPrettyString(target):target}");

        if (TryComp<PhysicsComponent>(target, out var physics) &&
                _random.Prob(GetSummarizeChance(ent, target, CombatAction.Push, CombatResist.Push)))
        {
            success = true;
            var userPos = _transform.GetWorldPosition(ent);
            var targetPos = _transform.GetMapCoordinates(target).Position;

            var direction = targetPos - userPos;

            var impulseVector = direction.Normalized() * ent.Comp.PushSpeed * physics.Mass;
            _physics.ApplyLinearImpulse(target, impulseVector, body: physics);

            if (TryComp<PhysicsComponent>(ent, out var userPhysics))
            {
                var ev = new ThrowerImpulseEvent();
                RaiseLocalEvent(ent, ref ev);

                if (ev.Push)
                    _physics.ApplyLinearImpulse(ent, -impulseVector, body: userPhysics);
            }

            _adminLogger.Add(LogType.DisarmedAction, LogImpact.Medium, $"{ToPrettyString(ent):ent} pushed {ToPrettyString(target):target}");
        }

        if (TryComp<HandsComponent>(target, out var hands) &&
                _random.Prob(GetSummarizeChance(ent, target, CombatAction.Push, CombatResist.Push)))
        {
            var offsetRandomCoordinates = _transform.GetMoverCoordinates(target).Offset(_random.NextVector2(1f, 1.5f));

            if (ThrowHeldItem(target, offsetRandomCoordinates))
            {
                success = true;
                _adminLogger.Add(LogType.DisarmedAction, LogImpact.Medium, $"{ToPrettyString(ent):ent} disarmed {ToPrettyString(target):target}");
            }
        }

        if (_random.Prob(GetSummarizeChance(ent, target, CombatAction.Knockdown, CombatResist.Knockdown)))
        {
            if (_stun.TryKnockdown(target.Owner, ent.Comp.KnockdownTime, true, true, false, false))
            {
                _adminLogger.Add(LogType.DisarmedKnockdown, LogImpact.Medium, $"{ToPrettyString(ent):ent} knocked down {ToPrettyString(target):target}");
                success = true;
            }
        }

        if (!success)
            return;

        _interaction.DoContactInteraction(ent, target);

        if (_net.IsServer)
        {
            _stamina.TakeStaminaDamage(target, ent.Comp.DisarmStaminaDamage, source: ent);
            _audio.PlayPvs(ent.Comp.DisarmSuccessSound, target, AudioParams.Default.WithVariation(0.025f).WithVolume(5f));
        }
    }

    private void DoGrabAttack(Entity<AdvancedCombatComponent> ent, Entity<AdvancedCombatComponent> target)
    {
        if (!TryComp<PullerComponent>(ent, out var puller))
            return;

        if (puller.Pulling is {} pulled &&
            pulled != target.Owner)
            FreeFromGrab(pulled, ent);

        if (_pull.GetPuller(target) is {} otherGrabber &&
                otherGrabber != ent.Owner &&
                TryComp<AdvancedCombatComponent>(otherGrabber, out var advOther))
            TryForceDecraceGrabStage(ent, target, (otherGrabber, advOther));
        else
            TryIncraceGrabStage(ent, target);

        _adminLogger.Add(LogType.DisarmedAction, $"{ToPrettyString(ent):ent} used grab on {ToPrettyString(target):target}");
    }

    private bool TryForceDecraceGrabStage(
            Entity<AdvancedCombatComponent> ent,
            Entity<AdvancedCombatComponent> target,
            Entity<AdvancedCombatComponent> otherGrabber)
    {
        var newStage = otherGrabber.Comp.GrabStage - 1;

        if (ent.Comp.GrabStagesCooldowns.TryGetValue(otherGrabber.Comp.GrabStage, out var cooldown) &&
                ent.Comp.LastGrab + cooldown > _timing.CurTime)
        {
            var failureLoc = newStage == GrabStage.Passive ? "grab-stage-cooldown-failure-first" : "grab-stage-cooldown-failure-stage-up";
            _popup.PopupEntity(Loc.GetString(failureLoc, ("target", target)), ent, ent, PopupType.SmallCaution);

            return false;
        }

        UpdateGrabTimer(ent);

        MakeGrabStage(otherGrabber, target, newStage);

        if (newStage is GrabStage.None)
            MakeGrabStage(ent, target, newStage + 1);

        if (_net.IsServer)
            _audio.PlayPvs(ent.Comp.GrabSuccessSound, target, AudioParams.Default.WithVariation(0.025f).WithVolume(5f));

        return true;
    }

    private bool TryIncraceGrabStage(Entity<AdvancedCombatComponent> ent, Entity<AdvancedCombatComponent> target)
    {
        var newStage = ent.Comp.GrabStage + 1;

        if (!GrabStage.IsDefined(newStage))
        {
            return false;
        }

        if (ent.Comp.GrabStagesCooldowns.TryGetValue(newStage, out var cooldown) &&
                ent.Comp.LastGrab + cooldown > _timing.CurTime)
        {
            var failureLoc = newStage == GrabStage.Passive ? "grab-stage-cooldown-failure-first" : "grab-stage-cooldown-failure-stage-up";
            _popup.PopupEntity(Loc.GetString(failureLoc, ("target", target)), ent, ent, PopupType.SmallCaution);

            return false;
        }

        var locId = "";
        var type = PopupType.Small;

        switch (newStage)
        {
            case GrabStage.Passive:
                locId = "grab-stage-passive-success";
                type = PopupType.Small;
                break;

            case GrabStage.Active:
                locId = "grab-stage-active-success";
                type = PopupType.Medium;
                break;

            case GrabStage.Hard:
                locId = "grab-stage-hard-success";
                type = PopupType.MediumCaution;
                break;
        }

        if (newStage is not GrabStage.None)
            UpdateGrabTimer(ent);

        MakeSpecializedPopup(ent, target, locId, type);

        MakeGrabStage(ent, target, newStage);

        if (_net.IsServer)
            _audio.PlayPvs(ent.Comp.GrabSuccessSound, target, AudioParams.Default.WithVariation(0.025f).WithVolume(5f));

        return true;
    }

    private void UpdateGrabTimer(Entity<AdvancedCombatComponent> ent)
    {
        ent.Comp.LastGrab = _timing.CurTime;

        Dirty(ent, ent.Comp);
    }

    private void MakeGrabStage(Entity<AdvancedCombatComponent> ent, Entity<AdvancedCombatComponent> target, GrabStage stage)
    {
        ent.Comp.GrabStage = stage;
        target.Comp.GrabbedStage = stage;

        switch (stage)
        {
            case GrabStage.None:
                FreeFromGrab(target);
                break;

            case GrabStage.Passive:
                _pull.TryStartPull(ent, target);
                break;

            case GrabStage.Active:
                _pull.TryStartPull(ent, target);
                break;
        }

        if (ent.Comp.GrabMaxDistances.TryGetValue(stage, out var maxDistance) &&
                TryComp<PullableComponent>(target, out var pullable) &&
                TryComp<JointComponent>(target, out var jointComp) &&
                pullable.PullJointId is {} jointId &&
                jointComp.GetJoints.TryGetValue(jointId, out var joint) &&
                joint is DistanceJoint distanceJoint)
            distanceJoint.MaxLength = Math.Min(distanceJoint.MaxLength, maxDistance);

        if (ent.Comp.GrabRespiratonRatios.TryGetValue(stage, out var respRatio))
            target.Comp.RespiratorRatio = respRatio;
        else
            target.Comp.RespiratorRatio = 1f;

        Dirty(ent, ent.Comp);
        Dirty(target, target.Comp);
    }

    private void FreeFromGrab(EntityUid grabbed, EntityUid? combatGrabber = null)
    {
        combatGrabber = _pull.GetPuller(grabbed) ?? combatGrabber;

        if (combatGrabber is {} grabber &&
                TryComp<AdvancedCombatComponent>(grabber, out var advGrabber))
        {
            advGrabber.GrabStage = GrabStage.None;
            Dirty(grabber, advGrabber);
        }

        if (TryComp<PullableComponent>(grabbed, out var pullable))
            _pull.TryStopPull(grabbed, pullable);

        if (!TryComp<AdvancedCombatComponent>(grabbed, out var advGrabbed))
            return;

        advGrabbed.RespiratorRatio = 1f;
        advGrabbed.GrabbedStage = GrabStage.None;

        Dirty(grabbed, advGrabbed);
    }

    private void MakeSpecializedPopup(
            EntityUid performer,
            EntityUid target,
            string locId,
            PopupType type)
    {
        if (!_net.IsServer)
            return;

        var filterOther = Filter.PvsExcept(performer, entityManager: EntityManager).RemoveWhereAttachedEntity(e => e == target);

        _popup.PopupEntity(Loc.GetString(locId + "-other", ("$target", target), ("source", performer)), target, filterOther, true, type);
        _popup.PopupEntity(Loc.GetString(locId + "-target", ("$target", target), ("source", performer)), target, target, type);
        _popup.PopupEntity(Loc.GetString(locId, ("$target", target), ("source", performer)), target, performer, type);
    }

    protected abstract bool ThrowHeldItem(EntityUid uid, EntityCoordinates coordinates);
}
