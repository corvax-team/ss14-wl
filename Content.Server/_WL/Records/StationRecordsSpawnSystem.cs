using Content.Server.Traits;
using Content.Shared.GameTicking;
using Content.Shared.StationRecords.Systems;

namespace Content.Server._WL.Records;

/// <summary>
/// Creates station records only after character traits have modified the spawned entity.
/// </summary>
public sealed partial class StationRecordsSpawnSystem : EntitySystem
{
    [Dependency] private StationRecordsSystem _records = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(
            OnPlayerSpawnComplete,
            after: [typeof(TraitSystem)]);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        _records.CreateGeneralRecord(args);
    }
}
