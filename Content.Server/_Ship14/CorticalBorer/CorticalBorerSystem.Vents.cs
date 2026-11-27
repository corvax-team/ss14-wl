using System.Linq;
using Content.Server.Atmos.Piping.Unary.Components;
using Content.Server.NodeContainer.Nodes;
using Content.Shared._Ship14.CorticalBorer;
using Content.Shared._Ship14.CorticalBorer.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.NodeContainer.NodeGroups;
using Robust.Shared.Random;

namespace Content.Server._Ship14.CorticalBorer;

public sealed partial class CorticalBorerSystem
{
    private void OnVentInteractHand<T>(Entity<T> ent, ref InteractHandEvent args) where T : Component
    {
        if (args.Handled || !TryComp(args.User, out CorticalBorerComponent? borer))
            return;

        if (TryStartVentTravel((args.User, borer), ent.Owner))
            args.Handled = true;
    }

    private void OnVentActivate<T>(Entity<T> ent, ref ActivateInWorldEvent args) where T : Component
    {
        if (args.Handled || !TryComp(args.User, out CorticalBorerComponent? borer))
            return;

        if (TryStartVentTravel((args.User, borer), ent.Owner))
            args.Handled = true;
    }

    private bool TryStartVentTravel(Entity<CorticalBorerComponent> ent, EntityUid source)
    {
        if (ent.Comp.Host != null || !IsVent(source) || ent.Comp.SugarSuppressed ||
            !_nodes.TryGetNode<PipeNode>(source, GetVentNodeName(source), out var pipe) || pipe.NodeGroup is not { } group)
            return false;

        var destinations = GetVentDestinations(source, group);
        if (destinations.Count == 0)
            return false;

        var doAfter = new DoAfterArgs(EntityManager,
            ent,
            TimeSpan.FromSeconds(3),
            new CorticalBorerVentDoAfterEvent(),
            ent,
            target: source)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            RequireCanInteract = false,
            AttemptFrequency = AttemptFrequency.StartAndEnd,
        };

        return _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnVentDoAfter(Entity<CorticalBorerComponent> ent, ref CorticalBorerVentDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } source ||
            !_nodes.TryGetNode<PipeNode>(source, GetVentNodeName(source), out var pipe) || pipe.NodeGroup is not { } group)
            return;

        args.Handled = true;
        var destinations = GetVentDestinations(source, group);
        if (destinations.Count == 0)
            return;

        var destination = _random.Pick(destinations);
        _transform.SetCoordinates(ent.Owner, Transform(destination).Coordinates);
    }

    private bool IsVent(EntityUid uid)
    {
        return HasComp<GasVentPumpComponent>(uid) ||
               HasComp<GasVentScrubberComponent>(uid) ||
               HasComp<GasPassiveVentComponent>(uid);
    }

    private string GetVentNodeName(EntityUid uid)
    {
        if (TryComp(uid, out GasVentPumpComponent? pump))
            return pump.Inlet;

        if (TryComp(uid, out GasVentScrubberComponent? scrubber))
            return scrubber.OutletName;

        if (TryComp(uid, out GasPassiveVentComponent? passive))
            return passive.InletName;

        return "pipe";
    }

    private List<EntityUid> GetVentDestinations(EntityUid source, INodeGroup group)
    {
        return group.Nodes
            .Select(node => node.Owner)
            .Where(uid => uid != source && IsVent(uid))
            .Distinct()
            .ToList();
    }
}
