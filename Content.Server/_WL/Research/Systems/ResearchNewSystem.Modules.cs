using System.Numerics;
using System.Linq;
using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Events;
using Content.Shared._WL.Research.Methods;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._WL.Research.Systems;

public sealed partial class ResearchSystemNew
{
    [Dependency] private SharedContainerSystem _containers = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;

    private void InitializeModules()
    { }

    [SubscribeLocalEvent]
    private void OnInserted(Entity<InsertableModuleComponent> ent, ref EntGotInsertedIntoContainerMessage _)
    {
        UpdateContainerStatus(ent, ent.Comp);
    }

    [SubscribeLocalEvent]
    private void OnRemoved(Entity<InsertableModuleComponent> ent, ref EntGotRemovedFromContainerMessage _)
    {
        UpdateContainerStatus(ent, ent.Comp);
    }

    [SubscribeLocalEvent]
    private void OnResearchAttempt(Entity<InsertableModuleComponent> ent, ref ResearchAttemtedEvent args)
    {
        if (!ent.Comp.InRightContainer)
            args.Cancel();
    }

    private void UpdateContainerStatus(EntityUid uid, InsertableModuleComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        if (!_containers.TryGetOuterContainer(uid, Transform(uid), out var container))
        {
            comp.InRightContainer = false;
            return;
        }

        comp.InRightContainer = !_whitelist.IsWhitelistFail(comp.Whitelist, container.Owner);
    }
}
