using Content.Server._WL.Emergency.Components;
using Content.Server.AlertLevel;
using Content.Server.Chat.Systems;
using Content.Server.Station.Systems;
using Content.Shared._WL.CCVars;
using Content.Shared._WL.Emergency.Prototype;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Server._WL.Emergency;

public sealed class EmergencyLevelSystem : EntitySystem
{

    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationInitializedEvent>(OnStationInitialize);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<EmergencyLevelComponent>();

        while (query.MoveNext(out var station, out var emergency))
        {
            if (emergency.CurrentDelay <= 0)
            {
                if (emergency.ActiveDelay)
                {
                    RaiseLocalEvent(new EmergencyDelayFinished());
                    emergency.ActiveDelay = false;
                }
                continue;
            }

            emergency.CurrentDelay -= frameTime;
        }
    }

    private void OnStationInitialize(StationInitializedEvent arg)
    {
        if (!TryComp<EmergencyLevelComponent>(arg.Station, out var emergencyLevelComponent))
            return;

        if (!_proto.TryIndex(emergencyLevelComponent.emergencyList, out var emergencyList))
            return;

        emergencyLevelComponent.Emergencies = emergencyList;

        var defaultLevel = emergencyLevelComponent.Emergencies.DefaultEmergency;

        if (string.IsNullOrEmpty(defaultLevel))
            defaultLevel = emergencyLevelComponent.Emergencies.Emergencys.First();

        SetEmergency(arg.Station, defaultLevel, true);

    }

    public float GetEmergencyDelay(EntityUid station, EmergencyLevelComponent? emergency = null)
    {
        if (!Resolve(station, ref emergency))
        {
            return float.NaN;
        }

        return emergency.CurrentDelay;
    }

    public void SetEmergency(EntityUid station, string emergency, bool playSound,
        MetaDataComponent? dataComponent = null, EmergencyLevelComponent? component = null)
    {
        if (!Resolve(station, ref dataComponent, ref component)
            || component.CurrentEmergency == emergency
            || !_proto.TryIndex<EmergencyPrototype>(emergency, out var prototype))
            return;

        component.CurrentEmergency = emergency;

        var stationName = dataComponent.EntityName;

        var announcement = prototype.Announcement;

        if (Loc.TryGetString(prototype.Announcement, out var locannouncement))
            announcement = locannouncement;

        var name = Loc.GetString(prototype.Name);

        var announcementFull = string.Empty;

        if (!string.IsNullOrEmpty(prototype.UniqueStartAnnouncement))
            announcementFull = Loc.GetString(
                prototype.UniqueStartAnnouncement, ("announcement", announcement));
        else
            announcementFull = Loc.GetString(
                "emergency-level-announcement", ("name", name), ("announcement", announcement));

        if (prototype.IsSoundAnnouncment)
        {
            var filter = _stationSystem.GetInOwningStation(station);
            _audio.PlayGlobal(prototype.Sound, filter, true, prototype.Sound.Params);
        }

        _chatSystem.DispatchStationAnnouncement(station, announcementFull, playDefaultSound: !prototype.IsSoundAnnouncment,
                colorOverride: prototype.Color, sender: stationName);

        component.CurrentDelay = _cfg.GetCVar(WLCVars.GameEmergencylChangeDelay);
        component.ActiveDelay = true;

        RaiseLocalEvent(new EmergencyChangedEvent(station, emergency));

    }
}

public sealed class EmergencyDelayFinished : EntityEventArgs
{ }

public sealed class EmergencyChangedEvent : EntityEventArgs
{
    public EntityUid Station { get; }
    public string Emergency { get; }

    public EmergencyChangedEvent(EntityUid station, string emergency)
    {
        Station = station;
        Emergency = emergency;
    }
}
