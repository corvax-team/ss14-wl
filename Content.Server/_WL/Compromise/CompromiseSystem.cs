using System.Linq;
using Content.Server.Station.Systems;
using Content.Server.Popups;
using Content.Shared._WL.Compromise;
using Content.Shared.StationRecords;
using Content.Shared.StationRecords.Components;
using Content.Shared.StationRecords.Events;
using Content.Shared.StationRecords.Systems;
using Content.Shared.Paper;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;

namespace Content.Server._WL.Compromise;

public sealed partial class CompromiseSystem : SharedCompromiseSystem
{
    [Dependency] private StationRecordsSystem _records = default!;
    [Dependency] private ServerStationSystem _station = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;
    [Dependency] private SharedAudioSystem _audioSystem = default!;
    [Dependency] private PaperSystem _paperSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CompromiseTabletComponent, RecordModifiedEvent>(UpdateUserInterface);
        SubscribeLocalEvent<CompromiseTabletComponent, GeneralRecordCreatedEvent>(UpdateUserInterface);

        Subs.BuiEvents<CompromiseTabletComponent>(CompromiseTabletKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(UpdateUserInterface);
            subs.Event<SelectStationRecord>(OnKeySelected);
            subs.Event<SetStationRecordFilter>(OnFiltersChanged);
            subs.Event<PrintStationRecord>(OnPrinted); // WL-Records
        });
    }

    [SubscribeLocalEvent]
    private void OnGeneralRecordCreated(ref GeneralRecordCreatedEvent ev)
    {
        if (ev.Profile.CompromiseText == string.Empty &&
                ev.Profile.CompromiseData.Count == 0)
            return;

        _records.AddRecordEntry(ev.Key, new CompromiseRecord(ev.Profile.CompromiseText, ev.Profile.CompromiseData.Values.ToList()));
        _records.Synchronize(ev.Key);
    }

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<CompromiseTabletComponent> ent, ref ComponentInit args)
    {
        UpdateStation(ent, ent.Comp);
    }

    private void UpdateStation(EntityUid uid, CompromiseTabletComponent? tablet = null)
    {
        if (!Resolve(uid, ref tablet))
            return;

        if (_station.GetOwningStation(uid) is not {} owning)
            return;

        tablet.Station = owning;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CompromiseTabletComponent>();
        while (query.MoveNext(out var uid, out var tablet))
        {
            ProcessPrintingAnimation(uid, frameTime, tablet);
        }
    }

    private void UpdateUserInterface<T>(Entity<CompromiseTabletComponent> ent, ref T args)
    {
        UpdateUserInterface(ent);
    }

    private void OnKeySelected(Entity<CompromiseTabletComponent> ent, ref SelectStationRecord msg)
    {
        ent.Comp.ActiveKey = msg.SelectedKey;
        UpdateUserInterface(ent);
    }

    private void OnPrinted(Entity<CompromiseTabletComponent> ent, ref PrintStationRecord msg)
    {
        if (!ent.Comp.CanPrint)
            return;

        if (ent.Comp.Station is not {} station)
        {
            UpdateStation(ent, ent.Comp);
            _popup.PopupEntity(Loc.GetString("compromise-tablet-station-fail"), ent);
            return;
        }

        if (!_records.TryGetRecord<CompromiseRecord>(new StationRecordKey(msg.Id, station), out var record))
            return;

        _audioSystem.PlayPvs(ent.Comp.PrintAudio, ent.Owner);

        ent.Comp.ContextPrint = BuildCompromiseText(record)
        ent.Comp.CanPrint = false;
        ent.Comp.TimePrintRemaining = ent.Comp.TimePrint;
    }

    private void ProcessPrintingAnimation(EntityUid uid, float frameTime, CompromiseTabletComponent comp)
    {
        if (comp.TimePrintRemaining > 0)
        {
            comp.TimePrintRemaining -= frameTime;

            if (comp.TimePrintRemaining <= 0)
            {
                var printed = SpawnAtPosition(comp.PrintPaperId, Transform(uid).Coordinates);

                if (TryComp<PaperComponent>(printed, out var paper))
                    _paperSystem.SetContent((printed, paper), comp.ContextPrint);

                comp.ContextPrint = string.Empty;
                comp.CanPrint = true;

                UpdateUserInterface((uid, comp));
            }
        }
    }
    // WL-Changes-Records-End

    private void OnFiltersChanged(Entity<CompromiseTabletComponent> ent, ref SetStationRecordFilter msg)
    {
        if (ent.Comp.Filter == null ||
            ent.Comp.Filter.Type != msg.Type || ent.Comp.Filter.Value != msg.Value)
        {
            ent.Comp.Filter = new StationRecordsFilter(msg.Type, msg.Value);
            UpdateUserInterface(ent);
        }
    }

    private void UpdateUserInterface(Entity<CompromiseTabletComponent> ent)
    {
        if (ent.Comp.Station is not {} station)
        {
            UpdateStation(ent, ent.Comp);
            _popup.PopupEntity(Loc.GetString("compromise-tablet-station-fail"), ent);
            return;
        }

        if (!TryComp<StationRecordsComponent>(station, out var stationRecords))
        {
            _ui.SetUiState(ent.Owner, CompromiseTabletKey.Key, new CompromiseTabletState());
            return;
        }

        var listing = _records.BuildListing((station, stationRecords), ent.Comp.Filter);

        var state = new CompromiseTabletState(listing, ent.Comp.Filter, ent.Comp.CanPrint);

        if (ent.Comp.ActiveKey is { } id)
        {
            var key = new StationRecordKey(id, station);
            _records.TryGetRecord(key, out state.StationRecord, stationRecords);
            _records.TryGetRecord(key, out state.Record, stationRecords);
            state.SelectedKey = id;
        }

        _ui.SetUiState(ent.Owner, CompromiseTabletKey.Key, state);
    }
}
