using Content.Shared.Actions;
using Content.Shared.Standing;
using Content.Shared.Buckle.Components;
using Content.Shared.Stunnable;
using Content.Shared.Bed.Sleep;
using Content.Shared.Bed.Components;
using Content.Shared.Actions.Components;

namespace Content.Shared._WL.Sleep;

public sealed class SleepOnBuckleSystem : EntitySystem
{
    [Dependency] private readonly ActionContainerSystem _actConts = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SleepingSystem _sleepingSystem = default!;
    [Dependency] protected readonly StandingStateSystem _standing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SleepOnBuckleComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SleepOnBuckleComponent, StrappedEvent>(OnStrapped);
        SubscribeLocalEvent<SleepOnBuckleComponent, UnstrappedEvent>(OnUnstrapped);
        SubscribeLocalEvent<BuckleComponent, WakeActionEvent>(OnWakeWhileBuckled);
    }

    private void OnMapInit(Entity<SleepOnBuckleComponent> ent, ref MapInitEvent args)
    {
        _actConts.EnsureAction(ent.Owner, ref ent.Comp.SleepAction, SleepingSystem.SleepActionId);
        Dirty(ent);
    }

    private void OnStrapped(Entity<SleepOnBuckleComponent> ent, ref StrappedEvent args)
    {
        if (TryComp<StandingStateComponent>(args.Buckle, out var standing)
            && standing.SleepAction != null
            && TryComp<ActionComponent>(standing.SleepAction.Value, out var actionComp)
            && actionComp.AttachedEntity == args.Buckle.Owner)
            _actionsSystem.RemoveAction(args.Buckle.Owner, standing.SleepAction);

        _actionsSystem.AddAction(args.Buckle, ref ent.Comp.SleepAction, SleepingSystem.SleepActionId, ent);
        Dirty(ent);
    }

    private void OnUnstrapped(Entity<SleepOnBuckleComponent> ent, ref UnstrappedEvent args)
    {
        if (!Terminating(args.Buckle.Owner))
        {
            _actionsSystem.RemoveAction(args.Buckle.Owner, ent.Comp.SleepAction);
            _sleepingSystem.TryWaking(args.Buckle.Owner);

            RemComp<KnockedDownComponent>(args.Buckle.Owner);
            RemComp<StunnedComponent>(args.Buckle.Owner);

            if (!TryComp<HealOnBuckleComponent>(ent, out var _))
                _standing.Stand(args.Buckle.Owner, force: true);

            if (TryComp<StandingStateComponent>(args.Buckle.Owner, out var standing)
                && standing.SleepAction != null)
                _actionsSystem.RemoveAction(args.Buckle.Owner, standing.SleepAction); //delete sleep action(it added after you unstrapped) after you unstrapped
        }
    }

    private void OnWakeWhileBuckled(Entity<BuckleComponent> ent, ref WakeActionEvent args)
    {
        if (ent.Comp.BuckledTo != null
            && TryComp<SleepOnBuckleComponent>(ent.Comp.BuckledTo.Value, out _)
            && TryComp<StrapComponent>(ent.Comp.BuckledTo.Value, out var strap)
            && strap.Position == StrapPosition.Stand)
            _standing.Stand(ent.Owner, force: true);
    }
}
