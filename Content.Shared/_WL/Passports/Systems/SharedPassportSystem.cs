using Content.Shared._WL.Passports.Components;
using Content.Shared._WL.Passports.Events;
using Content.Shared._WL.Records;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Preferences;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Roles;
using Content.Shared.GameTicking;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._WL.Passports.Systems;

public sealed class SharedPassportSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] private readonly SharedTransformSystem _sharedTransformSystem = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogManager = default!;

    public int CurrentYear = DateTime.Today.Year + 849;
    private const string NoConfederationId = "NoConfederation";
    private const string PIDChars = "ABCDEFGHJKLMNPQRSTUVWXYZ0123456789";
    private static readonly TimeSpan ToggleCooldown = TimeSpan.FromSeconds(0.5);
    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PassportComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        SubscribeLocalEvent<PassportComponent, ExaminedEvent>(OnExamined);
        _sawmill = LogManager.GetSawmill("passport");
    }

    private void OnExamined(EntityUid uid, PassportComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange || component.IsClosed)
            return;

        args.PushText(Loc.GetString("passport-registered-to", ("name", component.DisplayName)), 50);
        args.PushText(Loc.GetString("passport-species", ("species", component.DisplaySpecies)), 49);
        args.PushText(Loc.GetString("passport-gender", ("gender", component.DisplayGender)), 48);
        args.PushText(Loc.GetString("passport-height", ("height", component.DisplayHeight)), 47);
        args.PushText(Loc.GetString("passport-year-of-birth", ("year", component.DisplayYearOfBirth)), 47);
        args.PushText(Loc.GetString("passport-pid", ("pid", component.DisplayPID)), 46);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        var profile = ev.Profile;

        SpawnPassportForPlayer(ev.Mob, profile, ev.JobId);
    }

    public void SpawnPassportForPlayer(EntityUid mob, HumanoidCharacterProfile profile, string? jobId)
    {
        _sawmill.Debug($"Attempting passport spawn for {profile.Name}, job: {jobId}, confederation: {profile.Confederation}");

        if (jobId == null || !_prototypeManager.TryIndex(jobId, out JobPrototype? jobPrototype)
                          || Deleted(mob)
                          || !Exists(mob))
        {
            _sawmill.Warning($"No valid jobId for {profile.Name}");
            return;
        }

        var confederationId = string.IsNullOrEmpty(profile.Confederation)
            ? NoConfederationId
            : profile.Confederation;

        if (!_prototypeManager.TryIndex(confederationId, out ConfederationRecordsPrototype? confProto) ||
            !_prototypeManager.TryIndex(confProto.PassportPrototype, out EntityPrototype? entityPrototype))
        {
            if (!_prototypeManager.TryIndex<ConfederationRecordsPrototype>(NoConfederationId, out confProto) ||
                !_prototypeManager.TryIndex(confProto.PassportPrototype, out entityPrototype))
                return;
        }

        var passportEntity = _entityManager.SpawnEntity(entityPrototype.ID, _sharedTransformSystem.GetMapCoordinates(mob));
        var passportComponent = _entityManager.GetComponent<PassportComponent>(passportEntity);

        UpdatePassportProfile(new(passportEntity, passportComponent), profile);

        if (_inventory.TryGetSlotEntity(mob, "back", out var item) &&
                TryComp<StorageComponent>(item, out var inventory))
        {
            if (!TryComp<ItemComponent>(passportEntity, out var itemComp)
                || !_storage.CanInsert(item.Value, passportEntity, out _, inventory, itemComp)
                || !_storage.Insert(item.Value, passportEntity, out _, playSound: false))
            {
                _adminLogManager.Add(
                    LogType.EntitySpawn,
                    LogImpact.Low,
                    $"Passport for {profile.Name} was spawned on the floor due to missing bag space");
            }
        }
    }


    public void UpdatePassportProfile(Entity<PassportComponent> passport, HumanoidCharacterProfile profile)
    {
        passport.Comp.OwnerProfile = profile;

        var speciesProto = _prototypeManager.Index<SpeciesPrototype>(profile.Species);
        var genderString = profile.Gender.ToString();
        passport.Comp.DisplayName = profile.Name;
        passport.Comp.DisplaySpecies = Loc.GetString(speciesProto.Name);
        passport.Comp.DisplayGender = genderString switch
        {
            "Female" => Loc.GetString("passport-identity-gender-feminine"),
            "Male" => Loc.GetString("passport-identity-gender-masculine"),
            _ => Loc.GetString("passport-identity-gender-person")
        };
        passport.Comp.DisplayHeight = profile.Height.ToString();
        passport.Comp.DisplayYearOfBirth = (CurrentYear - profile.Age).ToString();
        passport.Comp.DisplayPID = GenerateIdentityString(
            profile.Name + profile.Height + profile.Age + profile.Height + profile.FlavorText
        );

        var evt = new PassportProfileUpdatedEvent(profile);
        RaiseLocalEvent(passport, ref evt);
        Dirty(passport);
    }

    private void OnUseInHand(Entity<PassportComponent> passport, ref UseInHandEvent evt)
    {
        if (evt.Handled || !_timing.IsFirstTimePredicted)
            return;

        evt.Handled = true;

        if (_timing.CurTime < passport.Comp.ToggleCooldownEnd)
            return;

        passport.Comp.ToggleCooldownEnd = _timing.CurTime + ToggleCooldown;
        passport.Comp.IsClosed = !passport.Comp.IsClosed;

        var passportEvent = new PassportToggleEvent();
        RaiseLocalEvent(passport, ref passportEvent);

        Dirty(passport);
    }

    private static string GenerateIdentityString(string seed)
    {
        var hashCode = seed.GetHashCode();
        var random = new System.Random(hashCode);

        var result = new char[17];

        var j = 0;
        for (var i = 0; i < 15; i++)
        {
            if (i is 5 or 10)
            {
                result[j++] = '-';
            }
            result[j++] = PIDChars[random.Next(PIDChars.Length)];
        }

        return new string(result);
    }
}
