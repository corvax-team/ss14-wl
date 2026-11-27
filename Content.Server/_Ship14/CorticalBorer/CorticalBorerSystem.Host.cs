using System.Linq;
using Content.Server.Resist;
using Content.Shared._Ship14.CorticalBorer;
using Content.Shared._Ship14.CorticalBorer.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Bed.Sleep;
using Content.Shared.CombatMode;
using Content.Shared.DoAfter;
using Content.Shared.Emoting;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Popups;
using Content.Shared.Speech;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._Ship14.CorticalBorer;

public sealed partial class CorticalBorerSystem
{
    private const string BorerContainerId = "cortical-borer";
    private const float InfestRange = 1.5f;
    private static readonly ProtoId<SpeciesPrototype> IpcSpecies = "Ipc";
    private static readonly ProtoId<SpeciesPrototype> AndroidSpecies = "Android";
    private static readonly HashSet<string> ProtectiveHelmetPrototypes =
    [
        "ClothingHeadEVAHelmetBase",
        "ClothingHeadHardsuitBase",
        "ClothingHeadHelmetBase",
        "ClothingHeadHelmetBombSuit",
        "ClothingHeadHelmetFire",
        "ClothingHeadHelmetAtmosFire",
    ];

    private void OnBorerShutdown(Entity<CorticalBorerComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Host is { } host && TryComp(host, out CorticalBorerHostComponent? hostComp))
        {
            CancelDoAfter<CorticalBorerControlDoAfterEvent>(ent);

            if (ent.Comp.ControllingHost)
                ReleaseControl(ent, host);

            if (hostComp.TrappedMind is { } trapped)
                CancelDoAfter<CorticalBorerResistDoAfterEvent>(trapped);

            hostComp.Borer = null;
            Dirty(host, hostComp);
            RemCompDeferred<CorticalBorerHostComponent>(host);
        }

        DeleteAction(ref ent.Comp.ScreechActionEntity);
    }

    private void OnHostInit(Entity<CorticalBorerHostComponent> ent, ref ComponentInit args)
    {
        ent.Comp.BorerContainer = _containers.EnsureContainer<ContainerSlot>(ent, BorerContainerId);
    }

    private void OnHostShutdown(Entity<CorticalBorerHostComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Borer is { } borer && TryComp(borer, out CorticalBorerComponent? borerComp))
            ForceLeave((borer, borerComp), ent.Owner);
    }

    private void OnHostStateChanged(Entity<CorticalBorerHostComponent> ent, ref MobStateChangedEvent args)
    {
        if (ent.Comp.Borer is not { } borer || !TryComp(borer, out CorticalBorerComponent? borerComp))
            return;

        if (args.NewMobState == MobState.Critical)
        {
            CancelDoAfter<CorticalBorerControlDoAfterEvent>(borer);
            if (ent.Comp.TrappedMind is { } trapped)
                CancelDoAfter<CorticalBorerResistDoAfterEvent>(trapped);
            return;
        }

        if (args.NewMobState != MobState.Dead)
            return;

        ForceLeave((borer, borerComp), ent.Owner);
    }

    private void CancelDoAfter<TEvent>(EntityUid user) where TEvent : DoAfterEvent
    {
        if (!TryComp(user, out DoAfterComponent? doAfters))
            return;

        foreach (var doAfter in doAfters.DoAfters.Values.ToList())
        {
            if (doAfter.Args.Event is TEvent)
                _doAfter.Cancel(user, doAfter.Index, doAfters, force: true);
        }
    }

    private void OnInfestAction(Entity<CorticalBorerComponent> ent, ref CorticalBorerInfestActionEvent args)
    {
        if (args.Handled || !CanInfest(ent, args.Target, true))
            return;

        args.Handled = true;
        var delay = HasComp<SleepingComponent>(args.Target) ? 2.5f : 5f;
        var doAfter = new DoAfterArgs(EntityManager,
            ent,
            TimeSpan.FromSeconds(delay),
            new CorticalBorerInfestDoAfterEvent(),
            ent,
            target: args.Target)
        {
            BreakOnMove = false,
            BreakOnDamage = false,
            RequireCanInteract = false,
            AttemptFrequency = AttemptFrequency.StartAndEnd,
        };

        _popup.PopupEntity(Loc.GetString("cortical-borer-infest-start-borer", ("target", args.Target)), ent, ent);
        _popup.PopupEntity(Loc.GetString("cortical-borer-infest-start-target"), args.Target, args.Target, PopupType.LargeCaution);
        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnInfestDoAfter(Entity<CorticalBorerComponent> ent, ref CorticalBorerInfestDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } target || !CanInfest(ent, target, false))
            return;

        args.Handled = true;
        Infest(ent, target);
    }

    private bool CanInfest(Entity<CorticalBorerComponent> ent, EntityUid target, bool showPopup)
    {
        if (ent.Comp.Host != null || ent.Comp.SugarSuppressed)
            return false;

        if (!TryComp(target, out HumanoidProfileComponent? humanoid) ||
            !ProtoMan.TryIndex(humanoid.Species, out var species) ||
            !species.RoundStart)
        {
            PopupInfestFailure(ent, showPopup, "cortical-borer-infest-invalid-target");
            return false;
        }

        if (_mobState.IsDead(target))
        {
            PopupInfestFailure(ent, showPopup, "cortical-borer-infest-dead-target");
            return false;
        }

        if (HasComp<CorticalBorerHostComponent>(target))
        {
            PopupInfestFailure(ent, showPopup, "cortical-borer-infest-already-infested");
            return false;
        }

        if (!_transform.InRange(Transform(ent).Coordinates, Transform(target).Coordinates, InfestRange))
        {
            PopupInfestFailure(ent, showPopup, "cortical-borer-infest-out-of-range");
            return false;
        }

        if (humanoid.Species == IpcSpecies || humanoid.Species == AndroidSpecies)
        {
            PopupInfestFailure(ent, showPopup, "cortical-borer-infest-incompatible-species");
            return false;
        }

        if (_inventory.TryGetSlotEntity(target, "head", out var head) &&
            (HasComp<CorticalBorerInfestationBlockerComponent>(head) ||
             MetaData(head.Value).EntityPrototype is { } helmet &&
             ProtoMan.EnumerateParents(helmet, includeSelf: true).Any(proto => ProtectiveHelmetPrototypes.Contains(proto.ID))))
        {
            PopupInfestFailure(ent, showPopup, "cortical-borer-infest-blocked-head");
            return false;
        }

        return true;
    }

    private void PopupInfestFailure(Entity<CorticalBorerComponent> ent, bool showPopup, LocId message)
    {
        if (showPopup)
            _popup.PopupEntity(Loc.GetString(message), ent, ent);
    }

    private void Infest(Entity<CorticalBorerComponent> ent, EntityUid target)
    {
        var host = EnsureComp<CorticalBorerHostComponent>(target);
        host.BorerContainer = _containers.EnsureContainer<ContainerSlot>(target, BorerContainerId);
        if (!_containers.Insert(ent.Owner, host.BorerContainer))
        {
            RemCompDeferred<CorticalBorerHostComponent>(target);
            return;
        }

        host.Borer = ent;
        if (ent.Comp.ParalyzingBiteArmed)
            DisarmParalyzingBite(ent);

        ent.Comp.Host = target;
        EnsureComp<CorticalBorerContainedComponent>(ent);
        RemCompDeferred<CanEscapeInventoryComponent>(ent);
        RemoveFreeActions(ent);
        AddHostedActions(ent);
        Dirty(target, host);
        Dirty(ent);
        _popup.PopupEntity(Loc.GetString("cortical-borer-infest-success-borer", ("target", target)), target, ent);
        _popup.PopupEntity(Loc.GetString("cortical-borer-infest-success-target"), target, target, PopupType.LargeCaution);
    }

    private void OnLeaveAction(Entity<CorticalBorerComponent> ent, ref CorticalBorerLeaveActionEvent args)
    {
        if (args.Handled || ent.Comp.Host is not { } host || ent.Comp.SugarSuppressed)
            return;

        args.Handled = true;
        var doAfter = new DoAfterArgs(EntityManager,
            ent,
            TimeSpan.FromSeconds(5),
            new CorticalBorerLeaveDoAfterEvent(),
            ent,
            target: host)
        {
            BreakOnMove = false,
            BreakOnDamage = false,
            RequireCanInteract = false,
            AttemptFrequency = AttemptFrequency.StartAndEnd,
        };
        _doAfter.TryStartDoAfter(doAfter);
        _popup.PopupEntity(Loc.GetString("cortical-borer-leave-start-target"), host, host, PopupType.LargeCaution);
    }

    private void OnLeaveDoAfter(Entity<CorticalBorerComponent> ent, ref CorticalBorerLeaveDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || ent.Comp.Host is not { } host || args.Target != host || ent.Comp.SugarSuppressed)
            return;

        args.Handled = true;
        ForceLeave(ent, host);
    }

    private void ForceLeave(Entity<CorticalBorerComponent> ent, EntityUid host)
    {
        CancelDoAfter<CorticalBorerControlDoAfterEvent>(ent);
        CancelDoAfter<CorticalBorerLeaveDoAfterEvent>(ent.Owner);

        if (ent.Comp.ControllingHost)
            ReleaseControl(ent, host);

        if (TryComp(host, out CorticalBorerHostComponent? hostComp))
        {
            if (hostComp.TrappedMind is { } trapped)
                CancelDoAfter<CorticalBorerResistDoAfterEvent>(trapped);

            _containers.Remove(ent.Owner, hostComp.BorerContainer);
            hostComp.Borer = null;
            Dirty(host, hostComp);
            RemCompDeferred<CorticalBorerHostComponent>(host);
        }

        _transform.SetCoordinates(ent.Owner, Transform(host).Coordinates);
        ent.Comp.Host = null;
        ClearSugarSuppression(ent);
        ent.Comp.LastChemicalSeverity = -1;
        RemCompDeferred<CorticalBorerContainedComponent>(ent);
        EnsureComp<CanEscapeInventoryComponent>(ent);
        _alerts.ClearAlert(ent.Owner, ent.Comp.ChemicalsAlert);
        RemoveHostedActions(ent);
        AddFreeActions(ent);
        DisarmParalyzingBite(ent);
        _actions.SetCooldown(ent.Comp.ParalyzeActionEntity, TimeSpan.FromSeconds(150));
        Dirty(ent);
        _popup.PopupEntity(Loc.GetString("cortical-borer-leave-success"), ent, ent);
    }

    private void OnControlAction(Entity<CorticalBorerComponent> ent, ref CorticalBorerControlActionEvent args)
    {
        if (args.Handled || ent.Comp.Host is not { } host || ent.Comp.ControllingHost ||
            _mobState.IsCritical(host) || _mobState.IsDead(host) || ent.Comp.SugarSuppressed)
            return;

        args.Handled = true;
        var doAfter = new DoAfterArgs(EntityManager,
            ent,
            TimeSpan.FromSeconds(15),
            new CorticalBorerControlDoAfterEvent(),
            ent,
            target: host)
        {
            BreakOnMove = false,
            BreakOnDamage = false,
            RequireCanInteract = false,
            DistanceThreshold = null,
            AttemptFrequency = AttemptFrequency.StartAndEnd,
        };
        if (_doAfter.TryStartDoAfter(doAfter))
            _popup.PopupEntity(Loc.GetString("cortical-borer-control-start-target"), host, host, PopupType.LargeCaution);
    }

    private void OnControlDoAfter(Entity<CorticalBorerComponent> ent, ref CorticalBorerControlDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || ent.Comp.Host is not { } host || args.Target != host || ent.Comp.ControllingHost ||
            _mobState.IsCritical(host) || _mobState.IsDead(host) || ent.Comp.SugarSuppressed)
            return;

        args.Handled = true;
        AssumeControl(ent, host);
    }

    private void AssumeControl(Entity<CorticalBorerComponent> ent, EntityUid host)
    {
        if (!_mind.TryGetMind(ent, out var borerMind, out var borerMindComp) ||
            !TryComp(host, out CorticalBorerHostComponent? hostComp))
        {
            Log.Warning($"Failed to control cortical borer host. Borer={ToPrettyString(ent)}, host={ToPrettyString(host)}.");
            return;
        }

        RemoveHostedActions(ent);
        hostComp.ControllingMind = borerMind;
        if (_mind.TryGetMind(host, out var hostMind, out var hostMindComp))
        {
            var trapped = SpawnAttachedTo("CorticalBorerTrappedMind", new EntityCoordinates(host, 0f, 0f));
            var victim = EnsureComp<CorticalBorerVictimComponent>(trapped);
            victim.Host = host;
            victim.Borer = ent;
            hostComp.TrappedMind = trapped;
            hostComp.OriginalMind = hostMind;
            Dirty(trapped, victim);
            _mind.TransferTo(hostMind, trapped, mind: hostMindComp, ghostCheckOverride: true, createGhost: false);
            _popup.PopupEntity(Loc.GetString("cortical-borer-control-success-target"), trapped, trapped, PopupType.LargeCaution);
        }

        _mind.TransferTo(borerMind, host, mind: borerMindComp, ghostCheckOverride: true, createGhost: false);

        ent.Comp.ControllingHost = true;
        _actions.AddAction(host, ref ent.Comp.ReleaseActionEntity, ent.Comp.ReleaseAction);
        Dirty(ent);
        Dirty(host, hostComp);
        _popup.PopupEntity(Loc.GetString("cortical-borer-control-success-borer"), host, host, PopupType.Large);
    }

    private void OnVictimInit(Entity<CorticalBorerVictimComponent> ent, ref ComponentInit args)
    {
        _actions.AddAction(ent, ref ent.Comp.ResistActionEntity, ent.Comp.ResistAction);
    }

    private void OnResistAction(ref CorticalBorerResistActionEvent args)
    {
        if (args.Handled || args.Action.Comp.AttachedEntity is not { } victimUid ||
            !TryComp(victimUid, out CorticalBorerVictimComponent? victim) ||
            _mobState.IsCritical(victim.Host) || _mobState.IsDead(victim.Host) ||
            !TryComp(victim.Borer, out CorticalBorerComponent? borer))
            return;

        args.Handled = true;
        var doAfter = new DoAfterArgs(EntityManager,
            victimUid,
            TimeSpan.FromSeconds(75),
            new CorticalBorerResistDoAfterEvent(),
            eventTarget: victimUid,
            target: victim.Host)
        {
            BreakOnMove = false,
            BreakOnDamage = false,
            RequireCanInteract = false,
            DistanceThreshold = null,
            AttemptFrequency = AttemptFrequency.StartAndEnd,
        };
        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnResistDoAfter(Entity<CorticalBorerVictimComponent> ent, ref CorticalBorerResistDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || _mobState.IsCritical(ent.Comp.Host) || _mobState.IsDead(ent.Comp.Host) ||
            !TryComp(ent.Comp.Borer, out CorticalBorerComponent? borer))
            return;

        args.Handled = true;
        ReleaseControl((ent.Comp.Borer, borer), ent.Comp.Host);
    }

    private void OnReleaseAction(Entity<CorticalBorerHostComponent> ent, ref CorticalBorerReleaseActionEvent args)
    {
        if (args.Handled || ent.Comp.Borer is not { } borer ||
            !TryComp(borer, out CorticalBorerComponent? borerComp) ||
            borerComp.SugarSuppressed)
            return;

        args.Handled = true;
        ReleaseControl((borer, borerComp), ent.Owner);
    }

    private void ReleaseControl(Entity<CorticalBorerComponent> ent, EntityUid host)
    {
        if (!ent.Comp.ControllingHost || !TryComp(host, out CorticalBorerHostComponent? hostComp))
            return;

        DeleteAction(ref ent.Comp.ReleaseActionEntity);
        var borerTerminating = TerminatingOrDeleted(ent);
        var hostTerminating = TerminatingOrDeleted(host);
        if (hostComp.ControllingMind is { } borerMind && TryComp<MindComponent>(borerMind, out var borerMindComp) &&
            borerMindComp.OwnedEntity == host)
        {
            var borerBody = !borerTerminating && CanRestoreMindTo(ent) ? ent.Owner : (EntityUid?) null;
            _mind.TransferTo(borerMind, borerBody, mind: borerMindComp,
                ghostCheckOverride: true, createGhost: borerBody == null && !hostTerminating);
        }

        if (hostComp.TrappedMind is { } trapped)
        {
            CancelDoAfter<CorticalBorerResistDoAfterEvent>(trapped);
            if (hostComp.OriginalMind is { } hostMind && TryComp<MindComponent>(hostMind, out var hostMindComp) &&
                hostMindComp.OwnedEntity == trapped)
            {
                var hostBody = !hostTerminating && CanRestoreMindTo(host) ? host : (EntityUid?) null;
                _mind.TransferTo(hostMind, hostBody, mind: hostMindComp,
                    ghostCheckOverride: true, createGhost: hostBody == null);
            }

            QueueDel(trapped);
        }

        ent.Comp.ControllingHost = false;
        hostComp.TrappedMind = null;
        hostComp.ControllingMind = null;
        hostComp.OriginalMind = null;
        if (!borerTerminating)
            AddHostedActions(ent);

        Dirty(ent);
        Dirty(host, hostComp);
        _popup.PopupEntity(Loc.GetString("cortical-borer-control-released"), host, host, PopupType.Large);
    }

    private bool CanRestoreMindTo(EntityUid body)
    {
        return !_mind.TryGetMind(body, out _, out _) && !HasComp<ActorComponent>(body);
    }

    private void OnContainedStartup(Entity<CorticalBorerContainedComponent> ent, ref ComponentStartup args)
    {
        if (TryComp(ent, out CombatModeComponent? combat))
        {
            _combatMode.SetInCombatMode(ent.Owner, false, combat);
            _actions.RemoveAction(ent.Owner, combat.CombatToggleActionEntity);
        }

        _actionBlocker.UpdateCanMove(ent);
    }

    private void OnContainedShutdown(Entity<CorticalBorerContainedComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        if (TryComp(ent, out CombatModeComponent? combat))
            _actions.AddAction(ent.Owner, ref combat.CombatToggleActionEntity, combat.CombatToggleAction);

        _actionBlocker.UpdateCanMove(ent);
    }

    private void OnContainedUpdateCanMove(Entity<CorticalBorerContainedComponent> ent, ref UpdateCanMoveEvent args)
    {
        if (ent.Comp.LifeStage > ComponentLifeStage.Running)
            return;

        args.Cancel();
    }

    private void OnContainedSpeakAttempt(Entity<CorticalBorerContainedComponent> ent, ref SpeakAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnContainedEmoteAttempt(Entity<CorticalBorerContainedComponent> ent, ref EmoteAttemptEvent args)
    {
        args.Cancel();
    }
}
