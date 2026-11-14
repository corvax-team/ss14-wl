using System.Linq;
using Content.Server.GameTicking;
using Content.Server.Station.Systems;
using Content.Shared._WL.Compromise;
using Content.Shared.StationRecords;
using Content.Shared.StationRecords.Events;
using Content.Shared.StationRecords.Systems;

namespace Content.Server._WL.Compromise;

public sealed partial class CompromiseSystem : EntitySystem
{
    [Dependency] private ServerGameTicker _ticker = default!;
    [Dependency] private StationRecordsSystem _records = default!;
    [Dependency] private ServerStationSystem _station = default!;

    [SubscribeLocalEvent]
    private void OnGeneralRecordCreated(ref GeneralRecordCreatedEvent ev)
    {
        if (ev.Profile.CompromiseText == string.Empty &&
                ev.Profile.CompromiseData.Count == 0)
            return;

        _records.AddRecordEntry(ev.Key, new CompromiseRecord(ev.Profile.CompromiseText, ev.Profile.CompromiseData.Values.ToList()));
        _records.Synchronize(ev.Key);
    }
}
