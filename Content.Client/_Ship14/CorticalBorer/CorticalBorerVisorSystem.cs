using Content.Shared._Ship14.CorticalBorer.Components;
using Content.Shared.Inventory;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Client._Ship14.CorticalBorer;

public sealed partial class CorticalBorerVisorSystem : EntitySystem
{
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private InventorySystem _inventory = default!;

    private static readonly ProtoId<StatusIconPrototype> InfestationIcon = "HealthIconCorticalBorer";

    public override void Initialize()
    {
        SubscribeLocalEvent<CorticalBorerHostComponent, GetStatusIconsEvent>(OnGetStatusIcons);
    }

    private void OnGetStatusIcons(Entity<CorticalBorerHostComponent> ent, ref GetStatusIconsEvent args)
    {
        if (ent.Comp.Borer == null || _player.LocalEntity is not { } viewer ||
            !_inventory.TryGetSlotEntity(viewer, "eyes", out var visor) ||
            !HasComp<CorticalBorerVisorComponent>(visor))
            return;

        args.StatusIcons.Add(ProtoMan.Index(InfestationIcon));
    }
}
