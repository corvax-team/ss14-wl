using Content.Shared._WL._Offbrand.DiagnosticTool.Components;
using Content.Shared._Offbrand.Wounds;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Containers;

namespace Content.Shared._WL._Offbrand.DiagnosticTool;

public sealed class DiagnosticToolSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DiagnosticToolComponent, InventoryRelayedEvent<GetVerbsEvent<InnateVerb>>>(AddDiagnosticVerb);
        SubscribeLocalEvent<DiagnosticToolComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<DiagnosticToolComponent, DiagnosticActionEvent>(OnDiagnostic);
        SubscribeLocalEvent<DiagnosticToolComponent, DiagnosticToolDoAfterEvent>(OnDoAfter);
    }

    private void OnGetActions(Entity<DiagnosticToolComponent> ent, ref GetItemActionsEvent args)
    {
        args.AddAction(ref ent.Comp.ActionEntity, ent.Comp.Action);
    }

    private void OnDiagnostic(Entity<DiagnosticToolComponent> ent, ref DiagnosticActionEvent args)
    {
        StartListening(ent, args.Target);
    }

    private void AddDiagnosticVerb(Entity<DiagnosticToolComponent> ent, ref InventoryRelayedEvent<GetVerbsEvent<InnateVerb>> args)
    {
        if (!args.Args.CanInteract || !args.Args.CanAccess)
            return;

        if (!HasComp<MobStateComponent>(args.Args.Target))
            return;

        var target = args.Args.Target;

        InnateVerb verb = new()
        {
            Act = () => StartListening(ent, target),
            Text = Loc.GetString(ent.Comp.Verb),
            IconEntity = GetNetEntity(ent),
            Priority = 2,
        };
        args.Args.Verbs.Add(verb);
    }

    private void StartListening(Entity<DiagnosticToolComponent> ent, EntityUid target)
    {
        if (!_container.TryGetContainingContainer((ent, null, null), out var container))
            return;

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, container.Owner, ent.Comp.Delay, new DiagnosticToolDoAfterEvent(), ent, target: target, used: ent)
        {
            // DuplicateCondition = DuplicateConditions.SameEvent,
            BreakOnMove = true,
            Hidden = true,
            BreakOnHandChange = false,
        });
    }

    private void OnDoAfter(Entity<DiagnosticToolComponent> ent, ref DiagnosticToolDoAfterEvent args)
    {
        var target = args.Target;

        if (args.Handled || target == null || args.Cancelled)
        {
            ent.Comp.LastMeasuredDamage = null;
            return;
        }

        ExamineWithTool(ent, args.Args.User, target.Value);

        args.Repeat = true;
    }

    private void ExamineWithTool(Entity<DiagnosticToolComponent> tool, EntityUid user, EntityUid target)
    {
        // TODO: Add check for respirator component when it gets moved to shared.
        // If the mob is dead or cannot asphyxiation damage, the popup shows nothing.
        if (!TryComp<MobStateComponent>(target, out var mobState)                        ||
            !TryComp<BrainDamageComponent>(target, out var damageComp) ||
            _mobState.IsDead(target, mobState))
        {
            _popup.PopupPredicted(Loc.GetString(tool.Comp.NothingVerb), target, user);
            tool.Comp.LastMeasuredDamage = null;
            return;
        }

        var currDmg = damageComp.Damage;

        var absString = GetAbsoluteDamageString(currDmg, 0, 100, tool.Comp.LevelsDesc);

        // Don't show the change if this is the first time listening.
        if (tool.Comp.LastMeasuredDamage == null)
        {
            _popup.PopupPredicted(absString, target, user);
        }
        else
        {
            var deltaString = GetDeltaDamageString(tool.Comp.LastMeasuredDamage.Value, currDmg, tool.Comp.DeltaVerbs);
            _popup.PopupPredicted(Loc.GetString("diagnostic-combined-status", ("absolute", absString), ("delta", deltaString)), target, user);
        }

        tool.Comp.LastMeasuredDamage = currDmg;
    }

    private string GetAbsoluteDamageString(FixedPoint2 dmg, FixedPoint2 min, FixedPoint2 max, string[] verbs)
    {
        if (verbs.Length == 0)
            return Loc.GetString("absolute-error");

        dmg -= min;
        max -= min;

        dmg = max - dmg;

        int e = ((int)dmg / (int)(max / verbs.Length));

        var msg = verbs[System.Math.Min(verbs.Length - 1, System.Math.Max(0, e))];
        return Loc.GetString(msg);
    }

    private string GetDeltaDamageString(FixedPoint2 lastDamage, FixedPoint2 currentDamage, string[] verbs)
    {
        if (verbs.Length != 3)
            return Loc.GetString("delta-error");
        if (lastDamage > currentDamage)
            return Loc.GetString(verbs[2]);
        if (lastDamage < currentDamage)
            return Loc.GetString(verbs[0]);
        return Loc.GetString(verbs[1]);
    }

}

public sealed partial class DiagnosticActionEvent : EntityTargetActionEvent;
