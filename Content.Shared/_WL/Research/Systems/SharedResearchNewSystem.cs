using System.Linq;
using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Research.Systems;

public abstract partial class SharedResearchNewSystem : EntitySystem
{
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
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
        var avalibleResearches = GetAvaliableResearches(uid, component);

        foreach (var researchProto in avalibleResearches)
        {
            component.Researches.Add(researchProto, new ResearchState());
        }

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

    public List<ResearchPrototype> GetAvaliableResearches(EntityUid uid, TechnologyServerComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return new List<ResearchPrototype>();

        var availableTechnologies = new List<ResearchPrototype>();
        foreach (var tech in ProtoMan.EnumeratePrototypes<ResearchPrototype>())
        {
            if (component.SupportedDisciplines.Contains(tech.Discipline))
                availableTechnologies.Add(tech);
        }

        return availableTechnologies;
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
}
