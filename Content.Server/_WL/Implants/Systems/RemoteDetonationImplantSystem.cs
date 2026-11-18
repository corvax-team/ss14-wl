using Content.Server.Explosion.EntitySystems;
using Content.Server.Implants.Components;
using Content.Shared.Actions;
using Content.Shared.Explosion.Components;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;
using Content.Shared._WL.Implants.Components;

namespace Content.Server._WL.Implants;

public sealed partial class RemoteDetonationImplantSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private ExplosionSystem _explosion = default!;

    [SubscribeLocalEvent]
    private void OnImplantAttempt(Entity<RemoteDetonationImplantComponent> ent, ref AddImplantAttemptOnImplantEvent args)
    {
        if (args.User == args.Target)
            args.Cancel();
    }

    [SubscribeLocalEvent]
    private void OnImplanted(Entity<RemoteDetonationImplantComponent> ent, ref ImplantationCompleteEvent args)
    {
        ent.Comp.Implanter = args.User;
        ent.Comp.Target = args.Target;

        _actions.AddAction(args.User, ref ent.Comp.ActionEntity, ent.Comp.Action, ent.Owner);
    }

    [SubscribeLocalEvent]
    private void OnRemoved(Entity<RemoteDetonationImplantComponent> ent, ref ImplantRemovedEvent args)
    {
        RemoveAction(ent);
    }

    [SubscribeLocalEvent]
    private void OnShutdown(Entity<RemoteDetonationImplantComponent> ent, ref ComponentShutdown args)
    {
        RemoveAction(ent);
    }

    [SubscribeLocalEvent]
    private void OnDetonate(Entity<RemoteDetonationImplantComponent> ent, ref DetonateImplantedVictimActionEvent args)
    {
        if (ent.Comp.Implanter != args.Performer
            || ent.Comp.Target is not { } target
            || TerminatingOrDeleted(target)
            || !TryComp<SubdermalImplantComponent>(ent.Owner, out var implant)
            || implant.ImplantedEntity != target
            || !TryComp<ExplosiveComponent>(ent.Owner, out var explosive))
            return;

        _explosion.TriggerExplosive(ent.Owner, explosive, user: args.Performer);

        args.Handled = true;
    }

    private void RemoveAction(Entity<RemoteDetonationImplantComponent> ent)
    {
        if (ent.Comp.Implanter is { } implanter && ent.Comp.ActionEntity is { } action)
            _actions.RemoveAction(implanter, action);

        ent.Comp.Implanter = null;
        ent.Comp.Target = null;
        ent.Comp.ActionEntity = null;
    }
}
