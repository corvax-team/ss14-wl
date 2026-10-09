using Content.Server.Audio;
using Content.Server.Power.EntitySystems;
using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Systems;
using Content.Shared._WL.Research.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Power;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Server._WL.Research.Systems;

public sealed partial class DAnalyzerSystem : SharedDAnalyzerSystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private AmbientSoundSystem _ambientSound = default!;
    [Dependency] private ItemSlotsSystem _itemSlots = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.BuiEvents<DAnalyzerComponent>(DAnalyzerUiKey.Key, subs =>
        {
            subs.Event<DAnalyzeMessage>(OnStartDAnalyze);
        });
    }

    [SubscribeLocalEvent]
    private void OnInit(Entity<DAnalyzerComponent> ent, ref MapInitEvent args)
    {
        UpdateDAnalyzerInterface(ent, ent.Comp);
    }

    [SubscribeLocalEvent]
    private void OnItemInserted(Entity<DAnalyzerComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        UpdateDAnalyzerInterface(ent, ent.Comp);
    }

    [SubscribeLocalEvent]
    private void OnItemRemoved(Entity<DAnalyzerComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        UpdateDAnalyzerInterface(ent, ent.Comp);
    }

    private void OnStartDAnalyze(Entity<DAnalyzerComponent> ent, ref DAnalyzeMessage args)
    {
        var (uid, comp) = ent;

        if (!this.IsPowered(uid, EntityManager) || comp.CurrentType != DAnalyzeType.None)
            return;

        if (!_itemSlots.TryGetSlot(uid, comp.SlotId, out var itemSlot) || itemSlot.Item is not { } danalyzeObject)
            return;

        if (!IsValidDAnalyzable(danalyzeObject, out var danalyzable) || (danalyzable.AnalyzeType & args.Type) == 0x0)
            return;

        _itemSlots.SetLock(uid, comp.SlotId, true);
        ent.Comp.CurrentType = args.Type;
        ent.Comp.EndTime = _timing.CurTime + ent.Comp.Duration;
        // Appearance.SetData(uid, FlatpackCreatorVisuals.Packing, true);
        _ambientSound.SetAmbience(uid, true);

        Dirty(uid, comp);
        UpdateDAnalyzerInterface(uid, comp);
    }

    private void OnPowerChanged(Entity<DAnalyzerComponent> ent, ref PowerChangedEvent args)
    {
        if (!args.Powered)
            FinishDAnalyze(ent, true);
    }

    private void FinishDAnalyze(Entity<DAnalyzerComponent> ent, bool interrupted)
    {
        var (uid, comp) = ent;

        _itemSlots.SetLock(uid, comp.SlotId, false);
        comp.CurrentType = DAnalyzeType.None;
        // Appearance.SetData(uid, FlatpackCreatorVisuals.Packing, false);
        _ambientSound.SetAmbience(uid, false);
        Dirty(uid, comp);

        if (interrupted)
            return;

        if (!_itemSlots.TryGetSlot(uid, comp.SlotId, out var itemSlot) || itemSlot.Item is not { } danalyzeObject)
            return;

        Del(danalyzeObject);
        UpdateDAnalyzerInterface(uid, comp);
    }

    private void UpdateDAnalyzerInterface(EntityUid uid, DAnalyzerComponent? danalyzer = null)
    {
        if (!Resolve(uid, ref danalyzer))
            return;

        var state = new DAnalyzerBoundUserInterfaceState();

        if (_itemSlots.TryGetSlot(uid, danalyzer.SlotId, out var itemSlot) &&
                itemSlot.Item is { } danalyzeObject &&
                IsValidDAnalyzable(danalyzeObject, out var danalyzable))
        {
            var meta = MetaData(danalyzeObject);

            if (meta.EntityPrototype is { } entProto)
                state.ObjectId = entProto.ID;

            state.ObjectState = DAnalyzeState.NotAnalyzed;
            state.PointsData = danalyzable.DeconstructPoints;
        }

        _ui.SetUiState(uid, DAnalyzerUiKey.Key, state);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DAnalyzerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.CurrentType == DAnalyzeType.None)
                continue;

            if (_timing.CurTime < comp.EndTime)
                continue;

            FinishDAnalyze((uid, comp), false);
        }
    }
}
