using System.Linq;
using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Lathe;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Research.Systems;

public abstract partial class SharedResearchNewSystem : EntitySystem
{
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedLatheSystem _lathe = default!;
    [Dependency] protected SharedPopupSystem Popup = default!;
    [Dependency] protected SharedAudioSystem Audio = default!;
    [Dependency] protected SharedUserInterfaceSystem UI = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ResearchScannerComponent, ResearchScannerDoAfterEvent>(OnScannerDoAfter);
    }

    [SubscribeLocalEvent]
    private void OnRDBInit(EntityUid uid, TechnologyServerComponent component, ref MapInitEvent args)
    {
        var researchTree = new Dictionary<ProtoId<ResearchPrototype>, ResearchState>();
        researchTree.Add(component.RootResearch, new ResearchState());

        TryGenerateTechTree(component.RootResearch, ref researchTree);
        component.Researches = researchTree;

        Dirty(uid, component);
    }

    [SubscribeLocalEvent]
    private void OnScannerAfterInteract(Entity<ResearchScannerComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Target is not { } target)
            return;

        if (!HasComp<ResearchScannableComponent>(target))
            return;

        if (!args.CanReach)
            return;

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            args.User,
            ent.Comp.ScanDoAfterDuration,
            new ResearchScannerDoAfterEvent(),
            ent,
            target: target,
            used: ent
        )
        {
            DistanceThreshold = ent.Comp.Range
        };
        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    protected virtual void OnScannerDoAfter(EntityUid uid, ResearchScannerComponent component, DoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Target is null)
            return;

        Audio.PlayPredicted(component.CompleteSound, uid, args.User);
        Popup.PopupEntity(Loc.GetString("research-scanner-complete-scan"), uid, args.User);

        var ev = new ScannedResearchEvent(args.Args.Target.Value);
        RaiseLocalEvent(uid, ref ev);

        UI.OpenUi(uid, ResearchScannerUiKey.Key, args.User);

        args.Handled = true;
    }

    [SubscribeLocalEvent]
    private void OnInsertAttempt(Entity<DataReaderComponent> ent, ref ItemSlotInsertAttemptEvent args)
    {
        if (args.Slot.ID != ent.Comp.SlotId || args.Cancelled)
            return;

        if (HasComp<DataStorageComponent>(args.Item))
            return;

        args.Cancelled = true;
    }

    public bool TryGenerateTechTree(ProtoId<ResearchPrototype> rootResearch, ref Dictionary<ProtoId<ResearchPrototype>, ResearchState> tree)
    {
        var research = ProtoMan.Index(rootResearch);

        foreach (var child in research.ChildrenResearches)
        {
            if (!tree.TryAdd(child, new ResearchState(rootResearch)))
                return false;

            if (!TryGenerateTechTree(child, ref tree))
                return false;
        }

        return true;
    }

        public FormattedMessage GetResearchDescription(
        ProtoId<ResearchPrototype> researchId,
        bool includeCosts = true)
    {
        var research = ProtoMan.Index(researchId);
        var description = new FormattedMessage();
        var discipline = ProtoMan.Index(research.Discipline);

        if (research.Description != string.Empty)
        {
            description.AddMarkupOrThrow(Loc.GetString(research.Description));
            description.PushNewline();
            description.PushNewline();
        }

        description.AddMarkupOrThrow(Loc.GetString("research-console-tier-discipline-info",
            ("tier", 0), ("color", discipline.Color), ("discipline", Loc.GetString(discipline.Name))));
        description.PushNewline();
        description.PushNewline();

        if (includeCosts && !research.PointsCost.Empty)
        {
            description.AddMarkupOrThrow(Loc.GetString("points-spec-points-start"));
            description.PushNewline();
            foreach (var (typeId, value) in research.PointsCost.PointsDict)
            {
                var type = ProtoMan.Index(typeId);

                description.AddMarkupOrThrow(Loc.GetString("points-spec-points-entry",
                            ("type", type.LocalizedName),
                            ("color", type.Color),
                            ("value", value)
                        ));
                description.PushNewline();
            }
        }

        description.PushNewline();

        description.AddMarkupOrThrow(Loc.GetString("research-console-unlocks-list-start"));
        foreach (var recipe in research.RecipeUnlocks)
        {
            var recipeProto = ProtoMan.Index(recipe);
            description.PushNewline();
            description.AddMarkupOrThrow(Loc.GetString("research-console-unlocks-list-entry",
                ("name", _lathe.GetRecipeName(recipeProto))));
        }

        return description;
    }
}
