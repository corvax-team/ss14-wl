using System.Linq;
using Content.Server.Chat.Systems;
using Content.Server.Station.Systems;
using Content.Shared.AlertLevel; // WL-Changes: Alert Level Rework
using Content.Shared.CCVar;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Server.AlertLevel;

public sealed class AlertLevelSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;

    // Until stations are a prototype, this is how it's going to have to be.
    public const string DefaultAlertLevelSet = "stationAlerts";

    public override void Initialize()
    {
        SubscribeLocalEvent<StationInitializedEvent>(OnStationInitialize);
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypeReload);
    }

    public override void Update(float time)
    {
        var query = EntityQueryEnumerator<AlertLevelComponent>();

        while (query.MoveNext(out var station, out var alert))
        {
            if (alert.CurrentDelay <= 0)
            {
                if (alert.ActiveDelay)
                {
                    RaiseLocalEvent(new AlertLevelDelayFinishedEvent());
                    alert.ActiveDelay = false;
                }
                continue;
            }

            alert.CurrentDelay -= time;
        }
    }

    private void OnStationInitialize(StationInitializedEvent args)
    {
        if (!TryComp<AlertLevelComponent>(args.Station, out var alertLevelComponent))
            return;

        if (!_prototypeManager.TryIndex(alertLevelComponent.AlertLevelsListPrototype, out AlertLevelsListPrototype? alerts)) // WL-Changes: Alert Level Rework
        {
            return;
        }

        alertLevelComponent.AlertLevels = alerts;

        var defaultLevel = alertLevelComponent.AlertLevels.DefaultLevel;
        if (string.IsNullOrEmpty(defaultLevel))
        {
            defaultLevel = alertLevelComponent.AlertLevels.Levels.First(); // WL-Changes: Alert Level Rework
        }

        SetLevel(args.Station, defaultLevel, false, false, true);
    }

    private void OnPrototypeReload(PrototypesReloadedEventArgs args)
    {
        // WL-Changes-start: Alert Level Rework
        if (!args.ByType.TryGetValue(typeof(AlertLevelsListPrototype), out var alertListPrototypes)
            || !alertListPrototypes.Modified.TryGetValue(DefaultAlertLevelSet, out var alertObject)
            || alertObject is not AlertLevelsListPrototype alerts)
            return;
        // WL-Changes-end

        var query = EntityQueryEnumerator<AlertLevelComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            comp.AlertLevels = alerts;

            if (!comp.AlertLevels.Levels.Contains(comp.CurrentLevel)) // WL-Changes: Alert Level Rework
            {
                var defaultLevel = comp.AlertLevels.DefaultLevel;
                if (string.IsNullOrEmpty(defaultLevel)
                // WL-Changes-start: Alert Level Rework
                    && comp.AlertLevels.Levels.Count > 0)
                    defaultLevel = comp.AlertLevels.Levels.First();
                else
                    continue;
                // WL-Changes-end

                SetLevel(uid, defaultLevel, true, true, true);
            }
        }

        RaiseLocalEvent(new AlertLevelsListPrototypeReloadedEvent()); // WL-Changes: Alert Level Rework
    }

    public string GetLevel(EntityUid station, AlertLevelComponent? alert = null)
    {
        if (!Resolve(station, ref alert))
        {
            return string.Empty;
        }

        return alert.CurrentLevel;
    }

    public float GetAlertLevelDelay(EntityUid station, AlertLevelComponent? alert = null)
    {
        if (!Resolve(station, ref alert))
        {
            return float.NaN;
        }

        return alert.CurrentDelay;
    }

    /// <summary>
    /// Get the default alert level for a station entity.
    /// Returns an empty string if the station has no alert levels defined.
    /// </summary>
    /// <param name="station">The station entity.</param>
    public string GetDefaultLevel(Entity<AlertLevelComponent?> station)
    {
        if (!Resolve(station.Owner, ref station.Comp) || station.Comp.AlertLevels == null)
        {
            return string.Empty;
        }
        return station.Comp.AlertLevels.DefaultLevel;
    }

    /// <summary>
    /// Set the alert level based on the station's entity ID.
    /// </summary>
    /// <param name="station">Station entity UID.</param>
    /// <param name="level">Level to change the station's alert level to.</param>
    /// <param name="playSound">Play the alert level's sound.</param>
    /// <param name="announce">Say the alert level's announcement.</param>
    /// <param name="force">Force the alert change. This applies if the alert level is not selectable or not.</param>
    /// <param name="locked">Will it be possible to change level by crew.</param>
    public void SetLevel(EntityUid station, string level, bool playSound, bool announce, bool force = false,
        bool locked = false, MetaDataComponent? dataComponent = null, AlertLevelComponent? component = null)
    {
        if (!Resolve(station, ref component, ref dataComponent)
            || component.AlertLevels == null
            // WL-Changes: Alert Level Rework
            || component.CurrentLevel == level
            || !component.AlertLevels.Levels.Contains(level)
            || !_prototypeManager.TryIndex<AlertLevelPrototype>(level, out var prototype)
            || prototype == null)
            return;
            // WL-Changes-end

        if (!force)
        {
            if (!prototype.Selectable // WL-Changes: Alert Level Rework
                || component.CurrentDelay > 0
                || component.IsLevelLocked)
            {
                return;
            }

            component.CurrentDelay = _cfg.GetCVar(CCVars.GameAlertLevelChangeDelay);
            component.ActiveDelay = true;
        }

        component.CurrentLevel = level;
        component.IsLevelLocked = locked;

        var stationName = dataComponent.EntityName;

        // WL-Changes-start: Alert Level Rework
        var name = level;

        if (Loc.TryGetString($"alert-level-{level.ToLower()}", out var locId))
            name = locId.ToLower();
        else if (!string.IsNullOrEmpty(prototype.SetName))
            name = prototype.SetName.ToLower();
        else
            name = Loc.GetString("alert-level-unknown").ToLower();

        // Announcement text. Is passed into announcementFull.
        var announcement = prototype.Announcement;

        if (Loc.TryGetString(prototype.Announcement, out var locAnnouncement))
            announcement = locAnnouncement;
        // WL-Changes-end

        // The full announcement to be spat out into chat.
        var announcementFull = Loc.GetString("alert-level-announcement", ("name", name), ("announcement", announcement));

        var playDefault = false;
        if (playSound)
        {
            if (prototype.Sound != null) // WL-Changes: Alert Level Rework
            {
                var filter = _stationSystem.GetInOwningStation(station);
                _audio.PlayGlobal(prototype.Sound, filter, true, prototype.Sound.Params); // WL-Changes: Alert Level Rework
            }
            else
            {
                playDefault = true;
            }
        }

        if (announce)
        {
            _chatSystem.DispatchStationAnnouncement(station, announcementFull, playDefaultSound: playDefault,
                colorOverride: prototype.Color, sender: stationName); // WL-Changes: Alert Level Rework
        }

        RaiseLocalEvent(new AlertLevelChangedEvent(station, level));
    }
}

public sealed class AlertLevelDelayFinishedEvent : EntityEventArgs
{}

public sealed class AlertLevelsListPrototypeReloadedEvent : EntityEventArgs // WL-Changes: Alert Level Rework
{}

public sealed class AlertLevelChangedEvent : EntityEventArgs
{
    public EntityUid Station { get; }
    public string AlertLevel { get; }

    public AlertLevelChangedEvent(EntityUid station, string alertLevel)
    {
        Station = station;
        AlertLevel = alertLevel;
    }
}
