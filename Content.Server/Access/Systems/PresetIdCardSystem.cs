using Content.Server.Access.Components;
using Content.Server.GameTicking;
using Content.Server.Roles;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.Access.Systems;
using Content.Shared.Containers.ItemSlots;
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
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly ContainerSystem _container = default!;

    private static readonly string IDItemSlot = "id";

    public override void Initialize()
    {
        SubscribeLocalEvent<PresetIdCardComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(PlayerJobsAssigned);
    }

    private void PlayerJobsAssigned(PlayerSpawnCompleteEvent ev)
    {
        // Go over all ID cards and make sure they're correctly configured for extended access.

        var player = ev.Player;

        if (player.AttachedEntity == null)
            return;

        if (!TryComp<ContainerManagerComponent>(player.AttachedEntity.Value, out var containersComp))
            return;

        if (!_container.TryGetContainer(player.AttachedEntity.Value, IDItemSlot, out var idContainer, containersComp))
            return;

        foreach (var containedEntity in idContainer.ContainedEntities)
        {
            var station = _stationSystem.GetOwningStation(containedEntity);

            // If we're not on an extended access station, the ID is already configured correctly from MapInit.
            if (station == null || !Comp<StationJobsComponent>(station.Value).ExtendedAccess)
                continue;

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

            SetupIdAccess(card.Value, preset, true, player);
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
        if (id.JobName == null )
            return;

        if (!_prototypeManager.TryIndex(id.JobName, out JobPrototype? job))
        {
            Log.Error($"Invalid job id ({id.JobName}) for preset card");
            return;
        }

        _accessSystem.SetAccessToJob(uid, job, extended);

        var jobName = _role.GetSubnameBySesssion(user, job.ID) ?? job.LocalizedName;
        _cardSystem.TryChangeJobTitle(uid, jobName);
        _cardSystem.TryChangeJobDepartment(uid, job);

        if (_prototypeManager.TryIndex<StatusIconPrototype>(job.Icon, out var jobIcon))
        {
            _cardSystem.TryChangeJobIcon(uid, jobIcon);
        }
    }
}
