using Content.Shared._WL._Offbrand.Surgery;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Tools.Components;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;

namespace Content.Shared._Offbrand.Surgery;

// this code needs to use predicted popups when construction gets predicted
public sealed partial class SurgeryToolSystem : EntitySystem
{
    [Dependency] private EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private StandingStateSystem _standingState = default!;

    // WL-Changes: Getto-surg start
    [Dependency] private ExamineSystemShared _examine = default!;
    // WL-Changes: Getto-surg end

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SurgeryToolComponent, ToolUseAttemptEvent>(OnToolAttemptUse);

        // WL-Changes: Getto-surg start
        SubscribeLocalEvent<SurgeryToolComponent, GetVerbsEvent<ExamineVerb>>(OnDetailedExamine);
        // WL-Changes: Getto-surg end
    }

    private void OnToolAttemptUse(Entity<SurgeryToolComponent> ent, ref ToolUseAttemptEvent args)
    {
        if (args.Target is not { } target)
            return;

        // WL-Changes: Getto-surgery start
        if (!HasComp<SurgeryTargetComponent>(target))
            return;
        // WL-Changes: Getto-surgery end


        if (_inventory.TryGetContainerSlotEnumerator(target, out var enumerator, ent.Comp.SlotsToCheck))
        {
            while (enumerator.MoveNext(out var slot))
            {
                if (slot.ContainedEntity is not { } contained)
                    continue;

                if (_entityWhitelist.CheckBoth(contained, ent.Comp.Blacklist, ent.Comp.Whitelist))
                    continue;

                _popup.PopupCursor(Loc.GetString(ent.Comp.SlotsDenialPopup, ("target", args.Target), ("clothing", contained)), args.User);
                args.Cancel();
                return;
            }
        }


        if (!_standingState.IsDown(target))
        {
            _popup.PopupCursor(Loc.GetString(ent.Comp.DownDenialPopup, ("target", args.Target)), args.User);
            args.Cancel();

            return;
        }
    }

    private void OnDetailedExamine(Entity<SurgeryToolComponent> ent, ref GetVerbsEvent<ExamineVerb> args)
    {
        var iconTexture = "/Textures/_WL/Interface/VerbIcons/scalpel.png";

        _examine.AddHoverExamineVerb(args,
            ent.Comp,
            Loc.GetString("surgery-tool-verb-text"),
            Loc.GetString("surgery-tool-verb-text-message", ("successChance", ent.Comp.SuccessChance)),
            iconTexture
        );
    }
}
