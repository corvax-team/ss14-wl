using Content.Shared._WL.Records;
using Content.Shared.GameTicking;
using Content.Shared.Preferences;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Passports.Systems;

public sealed partial class NationalitySystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args) =>
        ApplyNationality(args.Mob, args.Profile);

    public void ApplyNationality(EntityUid uid, HumanoidCharacterProfile profile)
    {
        var nationalityId = profile.Confederation;

        if (!_prototype.TryIndex<ConfederationRecordsPrototype>(nationalityId, out var confederationRecordsPrototype))
        {
            Log.Warning($"Nationality '{nationalityId}' not found!");
            return;
        }

        AddNationality(uid, confederationRecordsPrototype);
    }

    public void AddNationality(EntityUid uid, ConfederationRecordsPrototype confederationRecordsPrototype)
    {
        foreach (var special in confederationRecordsPrototype.Special)
        {
            special.AfterEquip(uid);
        }
    }
}
