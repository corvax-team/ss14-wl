using Content.Server.Access.Components;
using Content.Server.Roles;
using Content.Server.Station.Systems;
using Content.Shared.Access.Systems;
using Content.Shared.GameTicking;
using Content.Shared.PDA; // WL-Changes
using Content.Shared.Roles;
using Content.Shared.Station.Components;
using Robust.Server.Containers; // WL-Changes
using Robust.Shared.Containers; // WL-Changes
using Robust.Shared.Player; // WL-Changes

namespace Content.Server.Access.Systems;

public sealed partial class PresetIdCardSystem : EntitySystem
{
    [Dependency] private IdCardSystem _card = default!;
    [Dependency] private SharedAccessSystem _access = default!;
    [Dependency] private ServerStationSystem _station = default!;
    // WL-Changes: start
    [Dependency] private ContainerSystem _container = default!;
    [Dependency] private RoleSystem _role = default!;

    private static string IDItemSlot = "id";
    // WL-Changes: end

    [Dependency] private EntityQuery<StationJobsComponent> _stationJobsQuery;

    [SubscribeLocalEvent]
    private void PlayerJobsAssigned(PlayerSpawnCompleteEvent ev)
    {
        if (ev.JobId == null)
            return;

        if (!TryComp<ContainerManagerComponent>(ev.Mob, out var containersComp))
            return;

        if (!_container.TryGetContainer(ev.Mob, IDItemSlot, out var idContainer, containersComp))
            return;

        var jobProto = ProtoMan.Index<JobPrototype>(ev.JobId);

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

            SetupIdAccess((card.Value, preset), true, subname); //WL-Changes
            SetupIdName((card.Value, preset));
        }
    }

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<PresetIdCardComponent> ent, ref MapInitEvent args)
    {
        // If a preset ID card is spawned on a station at setup time,
        // the station may not exist,
        // or may not yet know whether it is on extended access (players not spawned yet).
        // PlayerJobsAssigned makes sure extended access is configured correctly in that case.

        var station = _station.GetOwningStation(ent);
        var extended = false;

        // Station not guaranteed to have jobs (e.g. nukie outpost).
        if (_stationJobsQuery.TryComp(station, out var stationJobs))
            extended = stationJobs.ExtendedAccess;

        SetupIdAccess(ent, extended);
        SetupIdName(ent);
    }

    private void SetupIdName(Entity<PresetIdCardComponent> ent)
    {
        if (ent.Comp.IdName == null)
            return;

        _card.TryChangeFullName(ent, Loc.GetString(ent.Comp.IdName));
    }

    private void SetupIdAccess(Entity<PresetIdCardComponent> ent, bool extended, ICommonSession? user = null)
    {
        if (ent.Comp.JobName == null)
            return;

        if (!ProtoMan.TryIndex(ent.Comp.JobName, out JobPrototype? job))
        {
            Log.Error($"Invalid job id ({ent.Comp.JobName}) for preset card");
            return;
        }

        _access.SetAccessToJob(ent, job, extended);

        var jobName = _role.GetSubnameBySesssion(user, job.ID) ?? job.LocalizedName; //WL-Changes
        _card.TryChangeJobTitle(ent, jobName); // WL-Changes
        _card.TryChangeJobDepartment(ent, job);

        if (ProtoMan.TryIndex(job.Icon, out var jobIcon))
        {
            _card.TryChangeJobIcon(ent, jobIcon);
        }
    }

    private void SetupIdAccess(Entity<PresetIdCardComponent> ent, bool extended, string jobName)
    {
        if (ent.Comp.JobName == null)
            return;

        if (!ProtoMan.TryIndex(ent.Comp.JobName, out JobPrototype? job))
        {
            Log.Error($"Invalid job id ({ent.Comp.JobName}) for preset card");
            return;
        }

        _access.SetAccessToJob(ent, job, extended);

        _card.TryChangeJobTitle(ent, jobName);
        _card.TryChangeJobDepartment(ent, job);

        if (ProtoMan.TryIndex(job.Icon, out var jobIcon))
            _card.TryChangeJobIcon(ent, jobIcon);
    }
}
