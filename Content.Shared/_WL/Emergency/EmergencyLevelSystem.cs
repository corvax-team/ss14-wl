using System.Diagnostics.CodeAnalysis;
using Content.Shared._WL.CCVars;
using Content.Shared.Chat;
using Content.Shared.Station;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._WL.Emergency;

public sealed partial class EmergencyLevelSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedChatSystem _chat = default!;
    [Dependency] private SharedStationSystem _station = default!;
    private static string _fallbackEmergencyKey = "emergency-level-unknown";
    private static string _fallbackEmergencyName = $"{_fallbackEmergencyKey}";
    private static string _fallbackEmergencyAnnouncement = $"{_fallbackEmergencyKey}-announcement";

    public override void Update(float time)
    {
        var query = EntityQueryEnumerator<EmergencyLevelComponent>();
        var curTime = _timing.CurTime;

        while (query.MoveNext(out var station, out var emergencyComp))
        {
            if (emergencyComp.DelayedUntil <= curTime)
            {
                var ev = new EmergencyLevelDelayFinishedEvent();
                RaiseLocalEvent(ref ev);
                emergencyComp.DelayedUntil = null;
                Dirty(station, emergencyComp);
            }
        }
    }

    public bool CanChangeEmergencyLevel(Entity<EmergencyLevelComponent?> station)
    {
        if (!Resolve(station, ref station.Comp))
            return false;

        return station.Comp.DelayedUntil == null;
    }

    public List<ProtoId<EmergencyLevelPrototype>> GetSelectableEmergencyLevels(Entity<EmergencyLevelComponent?> station)
    {
        List<ProtoId<EmergencyLevelPrototype>> list = new();
        if (!Resolve(station, ref station.Comp))
            return list;

        foreach (var levelId in station.Comp.AvailableEmergencyLevels)
        {
            if (_prototype.Resolve(levelId, out var level))
                list.Add(levelId);
        }

        return list;
    }

    public bool TryGetLevel(Entity<EmergencyLevelComponent?> station, [NotNullWhen(true)] out ProtoId<EmergencyLevelPrototype>? level)
    {
        level = null;
        if (!Resolve(station, ref station.Comp, false))
            return false;

        level = station.Comp.CurrentEmergencyLevel;
        return true;
    }

    public bool TryGetDefaultLevel(Entity<EmergencyLevelComponent?> station, [NotNullWhen(true)] out ProtoId<EmergencyLevelPrototype>? level)
    {
        level = null;
        if (!Resolve(station, ref station.Comp, false))
            return false;

        level = station.Comp.DefaultEmergencyLevel;
        return true;
    }

    public TimeSpan GetEmergencyLevelDelay(Entity<EmergencyLevelComponent?> station)
    {
        if (!Resolve(station, ref station.Comp, false))
            return TimeSpan.Zero;

        if (station.Comp.DelayedUntil == null)
            return TimeSpan.Zero;

        return station.Comp.DelayedUntil.Value - _timing.CurTime;
    }

    public void SetLevel(
        Entity<EmergencyLevelComponent?> station,
        ProtoId<EmergencyLevelPrototype> level,
        bool playSound = true,
        bool announce = true)
    {
        if (!Resolve(station, ref station.Comp))
            return;

        if (station.Comp.CurrentEmergencyLevel == level)
            return;

        if (!_prototype.Resolve(level, out var prototype))
            return;

        if (!CanChangeEmergencyLevel(station))
            return;

        station.Comp.DelayedUntil = _timing.CurTime + TimeSpan.FromSeconds(_cfg.GetCVar(WLCCVars.GameEmergencyLevelChangeDelay));
        station.Comp.CurrentEmergencyLevel = level;
        Dirty(station);

        var stationName = MetaData(station.Owner).EntityName;

        var announcementFull = Loc.GetString(
            string.IsNullOrEmpty(prototype.UniqueStartAnnouncement)
            ? "emergency-level-announcement"
            : prototype.UniqueStartAnnouncement,
            ("name", prototype.LocalizedName),
            ("announcement", EmergencyLevelAnnouncement(prototype)));

        var ev = new EmergencyLevelChangedEvent(station, level);
        RaiseLocalEvent(ref ev);

        if (_net.IsClient)
            return;

        var playDefault = false;
        if (playSound)
        {
            if (prototype.Sound != null)
            {
                var filter = _station.GetInOwningStation(station);
                _audio.PlayGlobal(prototype.Sound, filter, true);
            }
            else
                playDefault = true;
        }

        if (announce)
        {
            _chat.DispatchStationAnnouncement(
                station,
                announcementFull,
                playDefaultSound: playDefault,
                colorOverride: prototype.Color,
                sender: stationName);
        }
    }

    public (string Name, string SanitizeAnnouncement) EmergencyLevelData(Entity<EmergencyLevelComponent?> station)
    {
        if (!TryGetDefaultLevel(station, out var level))
        {
            return (Loc.GetString(_fallbackEmergencyName),
                Loc.GetString(_fallbackEmergencyAnnouncement));
        }

        return EmergencyLevelData(level);
    }

    public (string Name, string Announcement) EmergencyLevelData(ProtoId<EmergencyLevelPrototype>? level)
    {
        if (!ProtoMan.Resolve(level, out var proto))
        {
            return (Loc.GetString(_fallbackEmergencyName),
                Loc.GetString(_fallbackEmergencyAnnouncement));
        }

        return EmergencyLevelData(proto);
    }

    public (string Name, string Announcement) EmergencyLevelData(EmergencyLevelPrototype level)
    {
        return (level.LocalizedName, EmergencyLevelAnnouncement(level));
    }

    public string EmergencyLevelName(Entity<EmergencyLevelComponent?> station)
    {
        return !TryGetDefaultLevel(station, out var level)
            ? Loc.GetString(_fallbackEmergencyName)
            : EmergencyLevelName(level);
    }

    public string EmergencyLevelName(ProtoId<EmergencyLevelPrototype>? level)
    {
        return !ProtoMan.Resolve(level, out var proto)
            ? Loc.GetString(_fallbackEmergencyName)
            : proto.LocalizedName;
    }

    public string EmergencyLevelAnnouncement(Entity<EmergencyLevelComponent?> station)
    {
        return !TryGetDefaultLevel(station, out var level)
            ? Loc.GetString(_fallbackEmergencyAnnouncement)
            : EmergencyLevelAnnouncement(level);
    }

    public string EmergencyLevelAnnouncement(ProtoId<EmergencyLevelPrototype>? level)
    {
        return !ProtoMan.Resolve(level, out var proto)
            ? Loc.GetString(_fallbackEmergencyAnnouncement)
            : EmergencyLevelAnnouncement(proto);
    }

    public string EmergencyLevelAnnouncement(EmergencyLevelPrototype level)
    {
        return Loc.GetString(level.Announcement ?? _fallbackEmergencyAnnouncement);
    }
}

[ByRefEvent]
public record struct EmergencyLevelDelayFinishedEvent;

[ByRefEvent]
public record struct EmergencyLevelChangedEvent(EntityUid Station, ProtoId<EmergencyLevelPrototype> EmergencyLevel);
