using Content.Server.Access.Components;
using Content.Server.GameTicking;
using Content.Server.Roles;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.Access.Systems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.GameTicking;
using Content.Shared.PDA;
using Content.Shared.Roles;
using Content.Shared.StatusIcon;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Server.Access.Systems;

public sealed class PresetIdCardSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IdCardSystem _cardSystem = default!;
    [Dependency] private readonly SharedAccessSystem _accessSystem = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly RoleSystem _role = default!; //WL-Changes
    [Dependency] private readonly ContainerSystem _container = default!; //WL-Changes

    private static readonly string IDItemSlot = "id"; //WL-Changes

    public override void Initialize()
    {
        SubscribeLocalEvent<PresetIdCardComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(PlayerJobsAssigned); //WL-Changes
    }

    private void PlayerJobsAssigned(PlayerSpawnCompleteEvent ev) //WL-Changes
    {
        if (ev.JobId == null)
            return;

        if (!TryComp<ContainerManagerComponent>(ev.Mob, out var containersComp))
            return;

        if (!_container.TryGetContainer(ev.Mob, IDItemSlot, out var idContainer, containersComp))
            return;

        var jobProto = _prototypeManager.Index<JobPrototype>(ev.JobId);

        foreach (var containedEntity in idContainer.ContainedEntities)
        {
            EntityUid? card = null;
            PresetIdCardComponent? preset = null;

            if (TryComp<PresetIdCardComponent>(containedEntity, out var presedIdCard))
            {
                card = containedEntity;
                preset = presedIdCard;
            }
            else if (TryComp<PdaComponent>(containedEntity, out var pdaComp)
                && pdaComp.ContainedId != null
                && TryComp<PresetIdCardComponent>(pdaComp.ContainedId, out var pdaContainedPresetCard))
            {
                card = pdaComp.ContainedId;
                preset = pdaContainedPresetCard;
            }

            if (card == null || preset == null)
                continue;

            if (!ev.Profile.JobSubnames.TryGetValue(ev.JobId, out var subname)) //WL-Changes
                subname = jobProto.LocalizedName; //WL-Changes

            SetupIdAccess(card.Value, preset, true, subname); //WL-Changes
            SetupIdName(card.Value, preset);
        }
    }

    private void OnMapInit(EntityUid uid, PresetIdCardComponent id, MapInitEvent args)
    {
        // If a preset ID card is spawned on a station at setup time,
        // the station may not exist,
        // or may not yet know whether it is on extended access (players not spawned yet).
        // PlayerJobsAssigned makes sure extended access is configured correctly in that case.

        var station = _stationSystem.GetOwningStation(uid);
        var extended = false;

        // Station not guaranteed to have jobs (e.g. nukie outpost).
        if (TryComp(station, out StationJobsComponent? stationJobs))
            extended = stationJobs.ExtendedAccess;

        SetupIdAccess(uid, id, extended);
        SetupIdName(uid, id);
    }

    private void SetupIdName(EntityUid uid, PresetIdCardComponent id)
    {
        if (id.IdName == null)
            return;
        _cardSystem.TryChangeFullName(uid, id.IdName);
    }

    private void SetupIdAccess(EntityUid uid, PresetIdCardComponent id, bool extended, ICommonSession? user = null)
    {
        if (id.JobName == null)
            return;

        if (!_prototypeManager.TryIndex(id.JobName, out JobPrototype? job))
        {
            Log.Error($"Invalid job id ({id.JobName}) for preset card");
            return;
        }

        _accessSystem.SetAccessToJob(uid, job, extended);

        var jobName = _role.GetSubnameBySesssion(user, job.ID) ?? job.LocalizedName; //WL-Changes
        _cardSystem.TryChangeJobTitle(uid, jobName); //WL-Changes
        _cardSystem.TryChangeJobDepartment(uid, job);

        if (_prototypeManager.TryIndex<JobIconPrototype>(job.Icon, out var jobIcon))
        {
            _cardSystem.TryChangeJobIcon(uid, jobIcon);
        }
    }

    private void SetupIdAccess(EntityUid uid, PresetIdCardComponent id, bool extended, string jobName)
    {
        if (id.JobName == null)
            return;

        if (!_prototypeManager.TryIndex(id.JobName, out JobPrototype? job))
        {
            Log.Error($"Invalid job id ({id.JobName}) for preset card");
            return;
        }

        _accessSystem.SetAccessToJob(uid, job, extended);

        _cardSystem.TryChangeJobTitle(uid, jobName);
        _cardSystem.TryChangeJobDepartment(uid, job);

        if (_prototypeManager.Resolve(job.Icon, out var jobIcon))
            _cardSystem.TryChangeJobIcon(uid, jobIcon);
    }
}
