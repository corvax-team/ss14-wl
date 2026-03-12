using Content.Server.Power.Components;
using Content.Server.Station.Systems;
using Content.Server.StationRecords;
using Content.Server.StationRecords.Systems;
using Content.Shared._WL.Languages;
using Content.Shared._WL.MedicalRecords;
using Content.Shared._WL.MedicalRecords.Components;
using Content.Shared._WL.Records;
using Content.Shared.CriminalRecords.Components;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Paper;
using Content.Shared.StationRecords;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.MedicalRecords.Systems;

public sealed class MedicalRecordsConsoleSystem : EntitySystem
{
    [Dependency] private readonly StationRecordsSystem _records = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly PaperSystem _paperSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MedicalRecordsConsoleComponent, RecordModifiedEvent>(UpdateUserInterface);
        SubscribeLocalEvent<MedicalRecordsConsoleComponent, AfterGeneralRecordCreatedEvent>(UpdateUserInterface);

        Subs.BuiEvents<MedicalRecordsConsoleComponent>(MedicalRecordsConsoleKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(UpdateUserInterface);
            subs.Event<MedicalRecordsSelectStationRecord>(OnKeySelected);
            subs.Event<MedicalRecordsSetStationRecordFilter>(OnFiltersChanged);
            subs.Event<PrintStationRecord>(OnPrinted);
        });
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MedicalRecordsConsoleComponent, ApcPowerReceiverComponent>();
        while (query.MoveNext(out var uid, out var console, out var receiver))
        {
            if (!receiver.Powered)
                continue;

            ProcessPrintingAnimation(uid, frameTime, console);
        }
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

    private void OnPrinted(Entity<MedicalRecordsConsoleComponent> ent, ref PrintStationRecord msg)
    {
        var owning = _station.GetOwningStation(ent.Owner);

        if (owning == null)
            return;

        if (_records.TryGetRecord<GeneralStationRecord>(new StationRecordKey(msg.Id, owning.Value), out var record))
        {
            string languages = string.Empty;

            for (int i = 0; i < record.Languages.Count; i++)
            {
                languages += Loc.GetString(_prototypeManager.Index<LanguagePrototype>(record.Languages[i]).Name);

                if (i != record.Languages.Count - 1)
                    languages += ", ";
                else
                    languages += ".";
            }

            ent.Comp.ContextPrint = $"""
                {Loc.GetString("records-full-name-edit")} {(!string.IsNullOrEmpty(record.Fullname)
                ? record.Fullname : record.Name)}
                {Loc.GetString("records-date-of-birth-edit")}  {(!string.IsNullOrEmpty(record.DateOfBirth)
                ? record.DateOfBirth : Loc.GetString("generic-not-available-shorthand"))}
                {Loc.GetString("records-species")} {Loc.GetString(_prototypeManager.Index<SpeciesPrototype>(record.Species).Name)}
                {Loc.GetString("records-height", ("height", record.Height))}
                {Loc.GetString("records-language")} {languages}
                {(!string.IsNullOrEmpty(record.SecurityRecord) ? record.SecurityRecord
                : Loc.GetString("medical-records-console-no-record"))}
                """;
        }
        else
            return;

        _audioSystem.PlayPvs(ent.Comp.PrintAudio, ent.Owner);

        ent.Comp.CanPrintEntries = false;
        ent.Comp.TimePrintRemaining = ent.Comp.TimePrint;
    }

    private void ProcessPrintingAnimation(EntityUid uid, float frameTime, MedicalRecordsConsoleComponent comp)
    {
        if (comp.TimePrintRemaining > 0)
        {
            comp.TimePrintRemaining -= frameTime;

            var printSoundEnd = comp.TimePrintRemaining <= 0;

            if (printSoundEnd)
            {
                var printed = Spawn(comp.PrintPaperId, Transform(uid).Coordinates);

                if (TryComp<PaperComponent>(printed, out var paper))
                    _paperSystem.SetContent((printed, paper), comp.ContextPrint);

                comp.ContextPrint = string.Empty;

                comp.CanPrintEntries = true;

                var ent = new Entity<MedicalRecordsConsoleComponent>(uid, comp);

                UpdateUserInterface(ent);
            }

            return;
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
        var state = new MedicalRecordsConsoleState(listing, console.Filter, ent.Comp.CanPrintEntries);

        if (console.ActiveKey is { } id)
        {
            var key = new StationRecordKey(id, owningStation.Value);
            _records.TryGetRecord(key, out state.StationRecord, stationRecords);
            state.SelectedKey = id;
        }

        _ui.SetUiState(uid, MedicalRecordsConsoleKey.Key, state);
    }
}
