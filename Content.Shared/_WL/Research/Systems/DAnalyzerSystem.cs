using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Interaction;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Research.Systems;

public abstract partial class SharedDAnalyzerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
    }

    [SubscribeLocalEvent]
    private void OnAnalyzerInsertAttempt(Entity<DAnalyzerComponent> ent, ref ItemSlotInsertAttemptEvent args)
    {
        if (args.Slot.ID != ent.Comp.SlotId || args.Cancelled)
            return;

        if (HasComp<DAnalyzableComponent>(args.Item))
            return;

        args.Cancelled = true;
    }

    public bool IsValidDAnalyzable(EntityUid? uid, [NotNullWhen(true)] out DAnalyzableComponent? component)
    {
        component = null;

        if (uid is not { } danalyzable || !Resolve(danalyzable, ref component, false))
            return false;

        return !component.DeconstructPoints.Empty || !component.AnalyzePoints.Empty;
    }
}
