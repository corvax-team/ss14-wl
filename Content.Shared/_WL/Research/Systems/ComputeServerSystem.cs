using System.Linq;
using Content.Shared.Examine;
using Content.Shared.Emp;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Content.Shared.UserInterface;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared._WL.Research.Components;
using Robust.Shared.Containers;
using Robust.Shared.Timing;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Research.Systems;

public sealed partial class ComputeServerSystem : EntitySystem
{
    [Dependency] private SharedUserInterfaceSystem _ui = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    private void StartColdStart(EntityUid uid, ComputeServerComponent? component = null, TimeSpan? timerOverride = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.StartState = ColdStartState.Starting;
        component.ColdStartTime = _timing.CurTime;
        component.CurrentColdStartCooldown = timerOverride ?? component.BaseColdStartCooldown;

        Dirty(uid, component);
    }

    private void EnableBoard(EntityUid server, EntityUid board, ComputeServerComponent? serverComp = null, ComputeBoardComponent? boardComp = null)
    {
        if (!Resolve(server, ref serverComp) || !Resolve(board, ref boardComp))
            return;

        serverComp.CurrentBoard = board;
        StartColdStart(server, serverComp, boardComp.ColdStartTimeOverride);

        Dirty(server, serverComp);
    }

    private void DisableBoard(EntityUid uid, ComputeServerComponent? server = null)
    {
        if (!Resolve(uid, ref server))
            return;

        server.CurrentBoard = null;
        server.StartState = ColdStartState.Idle;

        Dirty(uid, server);
    }

    [SubscribeLocalEvent]
    private void GetVerb(Entity<ComputeServerComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract) // || !ShouldAddVerb(uid, component, args))
            return;

        if (_container.TryGetContainer(ent, ent.Comp.ContainerName, out var container))
        {
            var index = 0;

            foreach (var board in container.ContainedEntities)
            {
            if (!TryComp<ComputeBoardComponent>(board, out var boardComp))
                continue;

                var verb = new Verb
                {
                    Priority = index,
                    Category = VerbCategory.Programs,
                    Disabled = ent.Comp.CurrentBoard == board,
                    Act = () =>
                    {
                        EnableBoard(ent, board, ent.Comp, boardComp);

                        _popup.PopupEntity(Loc.GetString("compute-server-program-timer-popup", ("program", Loc.GetString(boardComp.Name))), ent);
                    },
                    Text = Loc.GetString(boardComp.Name),
                };

                args.Verbs.Add(verb);

                index++;
            }
        }

        var idleVerb = new Verb
        {
            Priority = -1,
            Category = VerbCategory.Programs,
            Disabled = ent.Comp.StartState == ColdStartState.Idle,
            Act = () =>
            {
                DisableBoard(ent, ent.Comp);

                _popup.PopupEntity(Loc.GetString("compute-server-disabled-popup"), ent);
            },
            Text = Loc.GetString("compute-server-verb-idle"),
        };

        args.Verbs.Add(idleVerb);
    }

    /*
    private void OnEmpPulse(Entity<BarSignComponent> ent, ref EmpPulseEvent args)
    {
        if (!ProtoMan.Resolve(ent.Comp.Emped, out var empedPrototype))
            return;

        SetBarSign(ent, empedPrototype);
        args.Affected = true;
        args.Disabled = true;
    }

    private void OnBoundUIAttempt(Entity<BarSignComponent> ent, ref BoundUserInterfaceMessageAttempt args)
    {
        if (HasComp<EmpDisabledComponent>(ent))
            args.Cancel();
    }
    */

    [SubscribeLocalEvent]
    private void OnExamineServer(Entity<ComputeServerComponent> ent, ref ExaminedEvent args)
    {
        var boardName = "compute-server-unknown-board";

        if (ent.Comp.CurrentBoard is {} board &&
                TryComp<ComputeBoardComponent>(board, out var boardComp))
            boardName = boardComp.Name;

        switch (ent.Comp.StartState)
        {
            case ColdStartState.Idle:
                args.PushText(Loc.GetString("compute-server-idle-examine-message"));
                break;

            case ColdStartState.Starting:
                args.PushText(
                    Loc.GetString(
                        "compute-server-examine-message",
                        ("mode", Loc.GetString("compute-server-starting")),
                        ("program", Loc.GetString(boardName))
                    )
                );
                break;

            case ColdStartState.Ready:
                args.PushText(
                    Loc.GetString(
                        "compute-server-examine-message",
                        ("mode", Loc.GetString("compute-server-ready")),
                        ("program", Loc.GetString(boardName))
                    )
                );
                break;

            case ColdStartState.Invalid:
                args.PushText(Loc.GetString("compute-server-invalid-examine-message"));
                break;
        }
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<ComputeServerComponent>();

        while (query.MoveNext(out var uid, out var server))
        {
            if (server.StartState != ColdStartState.Starting ||
                    _timing.CurTime - server.ColdStartTime < server.CurrentColdStartCooldown)
                continue;

            server.StartState = ColdStartState.Ready;
            _popup.PopupEntity(Loc.GetString("compute-server-program-start-popup"), uid);
        }
    }
}
