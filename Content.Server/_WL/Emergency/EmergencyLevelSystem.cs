using Content.Server._WL.Emergency.Commponents;
using Content.Server.Station.Systems;
using Content.Shared._WL.Communications.Prototype;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Server._WL.Emergency;

public sealed class EmergencyLevelSystem : EntitySystem
{

    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationInitializedEvent>(OnStationInitialize);
    }

    private void OnStationInitialize(StationInitializedEvent arg)
    {
        if (!TryComp<EmergencyLevelComponent>(arg.Station, out var emergencyLevelComponent))
            return;

        if (!_proto.TryIndex(emergencyLevelComponent.EmengercyList, out var emengercyList))
            return;

        emergencyLevelComponent.Emengercys = emengercyList;

        var defaultLevel = emergencyLevelComponent.Emengercys.DefaultEmergency;

        if (string.IsNullOrEmpty(defaultLevel))
            defaultLevel = emergencyLevelComponent.Emengercys.Emergencys.First();

        SetEmergency(arg.Station, defaultLevel, true);

    }

    public void SetEmergency(EntityUid station, string emergency, bool playSound,
        MetaDataComponent? dataComponent = null, EmergencyLevelComponent? component = null)
    {
        if (!Resolve(station, ref dataComponent, ref component)
            || !_proto.TryIndex<EmergencyPrototype>(emergency, out var prototype))
            return;

        component.CurrentEmengercy = emergency;

        var stationName = dataComponent.EntityName;

        var announcement = prototype.Announcement;

        if (Loc.TryGetString(prototype.Announcement, out var locannouncement))
            announcement = locannouncement;

        var announcementFull = Loc.GetString()
    }
}
