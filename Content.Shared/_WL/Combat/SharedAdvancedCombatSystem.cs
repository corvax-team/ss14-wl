using Content.Shared._WL.CombatIndicator;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Damage.Systems;
using Content.Shared.Input;
using Content.Shared.Interaction;
using Content.Shared.Hands.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._WL.Combat;

public abstract partial class SharedAdvancedCombatSystem : EntitySystem
{
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedStaminaSystem _stamina = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IGameTiming _timing = default!;

    [SubscribeLocalEvent]
    private void OnCombatIndicatorChanged(Entity<AdvancedCombatComponent> ent, ref CombatIndicatorChangedEvent args)
    {
        if (!ent.Comp.EnableByIndicator)
            return;

        ent.Comp.InCombat = args.Enabled;
    }

    [SubscribeLocalEvent]
    private void OnCombatAction(Entity<AdvancedCombatComponent> ent, ref CombatActionEvent args)
    {
        if (_mobState.IsIncapacitated(ent) ||
                Deleted(args.Target))
            return;

        switch (args.Intent)
        {
            case CombatIntent.Disarm:
                if (args.Target is {} target ||
                        target != ent.Owner)
                    DoDisarmAttack(ent, ent, target);
                break;
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

    private void DoDisarmAttack(Entity<AdvancedCombatComponent> ent, EntityUid weapon, EntityUid target)
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
            if (_stun.TryKnockdown(target, ent.Comp.KnockdownTime, true, true, false, false))
            {
                _adminLogger.Add(LogType.DisarmedKnockdown, LogImpact.Medium, $"{ToPrettyString(ent):ent} knocked down {ToPrettyString(target):target}");
                success = true;
            }
        }

        if (!success)
            return;

        _interaction.DoContactInteraction(ent, target);
        _stamina.TakeStaminaDamage(target, ent.Comp.DisarmStaminaDamage, source: ent);

        _audio.PlayPvs(ent.Comp.DisarmSuccessSound, target, AudioParams.Default.WithVariation(0.025f).WithVolume(5f));
    }

    protected abstract bool ThrowHeldItem(EntityUid uid, EntityCoordinates coordinates);
}
