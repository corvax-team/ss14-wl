using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Trigger;
using Content.Shared.Trigger.Components.Triggers;
using Content.Shared._WL.Trigger.Components.Triggers;

namespace Content.Shared._WL.Trigger.Systems;

public sealed partial class TriggerOnActionSystem : TriggerOnXSystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TriggerOnActionComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<TriggerOnActionComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<TriggerOnActionComponent, TriggerActionEvent>(OnAction);
    }

    private void OnMapInit(Entity<TriggerOnActionComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp(ent, out ActionsComponent? comp))
            return;

        Dirty(ent);
        _actions.AddAction(ent, ref ent.Comp.ActionEntity, ent.Comp.Action, component: comp);
    }

    private void OnShutdown(Entity<TriggerOnActionComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp(ent, out ActionsComponent? comp))
            return;

        var actions = new Entity<ActionsComponent?>(ent, comp);
        _actions.RemoveAction(actions, ent.Comp.ActionEntity);
    }

    private void OnAction(Entity<TriggerOnActionComponent> ent, ref TriggerActionEvent args)
    {
        Trigger.Trigger(ent.Owner, args.Performer, ent.Comp.KeyOut);
    }
}
