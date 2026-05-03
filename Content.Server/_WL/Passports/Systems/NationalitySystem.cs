using Content.Server.Players.PlayTimeTracking;
using Content.Shared._WL.Records;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.Players;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Utility;

namespace Content.Server._WL.Passports.Systems;

public sealed class NationalitySystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

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
            Logger.Warning($"Nationality '{nationalityId}' not found!");
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
