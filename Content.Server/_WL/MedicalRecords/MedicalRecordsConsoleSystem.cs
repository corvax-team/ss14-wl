using Content.Server.Station.Systems;
using Content.Server.StationRecords;
using Content.Server.StationRecords.Systems;
using Content.Shared._WL.MedicalRecords;
using Content.Shared._WL.MedicalRecords.Components;
using Content.Shared.StationRecords;
using Robust.Server.GameObjects;

namespace Content.Server._WL.MedicalRecords.Systems;

public sealed class MedicalRecordsConsoleSystem : EntitySystem
{
    [Dependency] private readonly StationRecordsSystem _records = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MedicalRecordsConsoleComponent, RecordModifiedEvent>(UpdateUserInterface);
        SubscribeLocalEvent<MedicalRecordsConsoleComponent, AfterGeneralRecordCreatedEvent>(UpdateUserInterface);

        Subs.BuiEvents<MedicalRecordsConsoleComponent>(MedicalRecordsConsoleKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(UpdateUserInterface);
            subs.Event<MedicalRecordsSelectStationRecord>(OnKeySelected);
            subs.Event<MedicalRecordsSetStationRecordFilter>(OnFiltersChanged);
        });
    }

    private void UpdateUserInterface<T>(Entity<MedicalRecordsConsoleComponent> ent, ref T args)
    {
        UpdateUserInterface(ent);
    }

    private void OnKeySelected(Entity<MedicalRecordsConsoleComponent> ent, ref MedicalRecordsSelectStationRecord msg)
    {
        ent.Comp.ActiveKey = msg.SelectedKey;
        UpdateUserInterface(ent);
    }

    private void OnFiltersChanged(Entity<MedicalRecordsConsoleComponent> ent, ref MedicalRecordsSetStationRecordFilter msg)
    {
        if (ent.Comp.Filter == null ||
            ent.Comp.Filter.Type != msg.Type || ent.Comp.Filter.Value != msg.Value)
        {
            ent.Comp.Filter = new StationRecordsFilter(msg.Type, msg.Value);
            UpdateUserInterface(ent);
        }
    }

    private void UpdateUserInterface(Entity<MedicalRecordsConsoleComponent> ent)
    {
        var (uid, console) = ent;
        var owningStation = _station.GetOwningStation(uid);

        if (!TryComp<StationRecordsComponent>(owningStation, out var stationRecords))
        {
            _ui.SetUiState(uid, MedicalRecordsConsoleKey.Key, new MedicalRecordsConsoleState());
            return;
        }

        var listing = _records.BuildListing((owningStation.Value, stationRecords), console.Filter);
        var state = new MedicalRecordsConsoleState(listing, console.Filter);

        if (console.ActiveKey is { } id)
        {
            var key = new StationRecordKey(id, owningStation.Value);
            _records.TryGetRecord(key, out state.StationRecord, stationRecords);
            state.SelectedKey = id;
        }

        _ui.SetUiState(uid, MedicalRecordsConsoleKey.Key, state);
    }
}
