using Content.Shared.StationRecords.Systems;

// WL-Changes-Records-Start
using Content.Server.Power.Components;
using Content.Server.Station.Systems;
using Content.Server._WL.Records;
using Content.Shared.Paper;
using Content.Shared.StationRecords;
using Content.Shared.StationRecords.Components;
using Content.Shared._WL.Records;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
// WL-Changes-Records-End

namespace Content.Server.StationRecords;

public sealed partial class GeneralStationRecordConsoleSystem : SharedGeneralStationRecordConsoleSystem
{
    // WL-Changes-Records-Start
    [Dependency] private StationRecordsSystem _records = default!;
    [Dependency] private StationSystem _station = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private IPrototypeManager _prototypes = default!;
    [Dependency] private PaperSystem _paper = default!;

    public override void Initialize()
    {
        Subs.BuiEvents<GeneralStationRecordConsoleComponent>(GeneralStationRecordConsoleKey.Key, subs =>
        {
            subs.Event<PrintStationRecord>(OnPrinted);
        });
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<GeneralStationRecordConsoleComponent, ApcPowerReceiverComponent>();
        while (query.MoveNext(out var uid, out var console, out var receiver))
        {
            if (receiver.Powered)
                ProcessPrinting(uid, frameTime, console);
        }
    }

    private void OnPrinted(Entity<GeneralStationRecordConsoleComponent> ent, ref PrintStationRecord msg)
    {
        if (!ent.Comp.CanPrintEntries)
            return;

        var station = _station.GetOwningStation(ent.Owner);
        if (station == null ||
            !_records.TryGetRecord<GeneralStationRecord>(new StationRecordKey(msg.Id, station.Value), out var record))
            return;

        var identity = RecordPrintIdentityBuilder.FromStationRecord(record, _prototypes, Loc.GetString);
        ent.Comp.ContextPrint = StructuredRecordFormatter.FormatDocument(
            Loc.GetString("records-print-employment-title"),
            "#356A49",
            identity,
            StructuredRecordFormatter.FormatEmployment(record.EmploymentRecord, _prototypes, Loc.GetString),
            Loc.GetString);
        ent.Comp.CanPrintEntries = false;
        ent.Comp.TimePrintRemaining = ent.Comp.TimePrint;
        Dirty(ent);
        _audio.PlayPvs(ent.Comp.PrintAudio, ent.Owner);
    }

    private void ProcessPrinting(EntityUid uid, float frameTime, GeneralStationRecordConsoleComponent component)
    {
        if (component.TimePrintRemaining <= 0)
            return;

        component.TimePrintRemaining -= frameTime;
        if (component.TimePrintRemaining > 0)
            return;

        var printed = Spawn(component.PrintPaperId, Transform(uid).Coordinates);
        if (TryComp<PaperComponent>(printed, out var paper))
            _paper.SetContent((printed, paper), component.ContextPrint);

        component.ContextPrint = string.Empty;
        component.CanPrintEntries = true;
        Dirty(uid, component);
    }
    // WL-Changes-Records-End
}
