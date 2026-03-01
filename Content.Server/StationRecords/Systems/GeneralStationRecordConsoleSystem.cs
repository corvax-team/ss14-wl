using Content.Server.Power.Components; // WL-Records
using Content.Server.Station.Systems;
using Content.Server.StationRecords.Components;
using Content.Shared._WL.Languages;
using Content.Shared._WL.Records;
using Content.Shared.Humanoid.Prototypes; // WL-Records
using Content.Shared.Paper;
using Content.Shared.StationRecords;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes; // WL-Records
using System.Linq;

namespace Content.Server.StationRecords.Systems;

public sealed class GeneralStationRecordConsoleSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly StationRecordsSystem _stationRecords = default!;
    [Dependency] private readonly PaperSystem _paperSystem = default!; // WL-Records
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!; // WL-Records
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!; // WL-Records

    public override void Initialize()
    {
        SubscribeLocalEvent<GeneralStationRecordConsoleComponent, RecordModifiedEvent>(UpdateUserInterface);
        SubscribeLocalEvent<GeneralStationRecordConsoleComponent, AfterGeneralRecordCreatedEvent>(UpdateUserInterface);
        SubscribeLocalEvent<GeneralStationRecordConsoleComponent, RecordRemovedEvent>(UpdateUserInterface);

        Subs.BuiEvents<GeneralStationRecordConsoleComponent>(GeneralStationRecordConsoleKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(UpdateUserInterface);
            subs.Event<SelectStationRecord>(OnKeySelected);
            subs.Event<SetStationRecordFilter>(OnFiltersChanged);
            subs.Event<DeleteStationRecord>(OnRecordDelete);
            subs.Event<PrintStationRecord>(OnRecordPrint);
        });
    }

    // WL-Records-start
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<GeneralStationRecordConsoleComponent, ApcPowerReceiverComponent>();
        while (query.MoveNext(out var uid, out var console, out var receiver))
        {
            if (!receiver.Powered)
                continue;

            ProcessPrintingAnimation(uid, frameTime, console);
        }
    }
    // WL-Records-end

    private void OnRecordDelete(Entity<GeneralStationRecordConsoleComponent> ent, ref DeleteStationRecord args)
    {
        if (!ent.Comp.CanDeleteEntries)
            return;

        var owning = _station.GetOwningStation(ent.Owner);

        if (owning != null)
            _stationRecords.RemoveRecord(new StationRecordKey(args.Id, owning.Value));
        UpdateUserInterface(ent); // Apparently an event does not get raised for this.
    }

    // WL-Records-Start
    private void OnRecordPrint(Entity<GeneralStationRecordConsoleComponent> ent, ref PrintStationRecord args)
    {

        var owning = _station.GetOwningStation(ent.Owner);

        if (owning == null)
            return;

        if (_stationRecords.TryGetRecord<GeneralStationRecord>(new StationRecordKey(args.Id, owning.Value), out var record))
        {
            var confederation = string.Empty;

            if (_prototypeManager.TryIndex<ConfederationRecordsPrototype>(record.Confederation, out var proto))
                confederation = Loc.GetString(proto.Name);
            else
                confederation = Loc.GetString("generic-not-available-shorthand");

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
                {Loc.GetString("records-confederation-edit")} {confederation}
                {Loc.GetString("records-country-edit")} {(!string.IsNullOrEmpty(record.Country)
                ? record.Country : Loc.GetString("generic-not-available-shorthand"))}
                {Loc.GetString("records-species")} {Loc.GetString(_prototypeManager.Index<SpeciesPrototype>(record.Species).Name)}
                {Loc.GetString("records-language")} {languages}
                {(!string.IsNullOrEmpty(record.EmploymentRecord) ? record.EmploymentRecord
                : Loc.GetString("general-station-console-no-employment-record"))}
                """;
        }
        else
            return;

        _audioSystem.PlayPvs(ent.Comp.PrintAudio, ent.Owner);

        ent.Comp.CanPrintEntries = false;
        ent.Comp.TimePrintRemaining = ent.Comp.TimePrint;
    }

    private void ProcessPrintingAnimation(EntityUid uid, float frameTime, GeneralStationRecordConsoleComponent comp)
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

                var ent = new Entity<GeneralStationRecordConsoleComponent>(uid, comp);

                UpdateUserInterface(ent);
            }

            return;
        }
    }
    // WL-Records-End

    private void UpdateUserInterface<T>(Entity<GeneralStationRecordConsoleComponent> ent, ref T args)
    {
        UpdateUserInterface(ent);
    }

    // TODO: instead of copy paste shitcode for each record console, have a shared records console comp they all use
    // then have this somehow play nicely with creating ui state
    // if that gets done put it in StationRecordsSystem console helpers section :)
    private void OnKeySelected(Entity<GeneralStationRecordConsoleComponent> ent, ref SelectStationRecord msg)
    {
        ent.Comp.ActiveKey = msg.SelectedKey;
        UpdateUserInterface(ent);
    }

    private void OnFiltersChanged(Entity<GeneralStationRecordConsoleComponent> ent, ref SetStationRecordFilter msg)
    {
        if (ent.Comp.Filter == null ||
            ent.Comp.Filter.Type != msg.Type || ent.Comp.Filter.Value != msg.Value)
        {
            ent.Comp.Filter = new StationRecordsFilter(msg.Type, msg.Value);
            UpdateUserInterface(ent);
        }
    }

    private void UpdateUserInterface(Entity<GeneralStationRecordConsoleComponent> ent)
    {
        var (uid, console) = ent;
        var owningStation = _station.GetOwningStation(uid);

        if (!TryComp<StationRecordsComponent>(owningStation, out var stationRecords))
        {
            _ui.SetUiState(uid, GeneralStationRecordConsoleKey.Key, new GeneralStationRecordConsoleState());
            return;
        }

        var listing = _stationRecords.BuildListing((owningStation.Value, stationRecords), console.Filter);

        switch (listing.Count)
        {
            case 0:
                _ui.SetUiState(uid, GeneralStationRecordConsoleKey.Key, new GeneralStationRecordConsoleState());
                return;
            default:
                if (console.ActiveKey == null)
                    console.ActiveKey = listing.Keys.First();
                break;
        }

        if (console.ActiveKey is not { } id)
            return;

        var key = new StationRecordKey(id, owningStation.Value);
        _stationRecords.TryGetRecord<GeneralStationRecord>(key, out var record, stationRecords);

        GeneralStationRecordConsoleState newState = new(id, record, listing, console.Filter, ent.Comp.CanDeleteEntries, ent.Comp.CanPrintEntries); // WL-Records
        _ui.SetUiState(uid, GeneralStationRecordConsoleKey.Key, newState);
    }
}
