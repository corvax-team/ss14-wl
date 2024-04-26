using Content.Server.Mind;
using Content.Server.Roles.Jobs;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;
using System.Linq;
using System.Numerics;
using System.Security.Policy;

namespace Content.Server.StationEvents.Components;

/// <summary>
///     Defines basic data for a station event
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class StationEventComponent : Component
{
    public const float WeightVeryLow = 0.0f;
    public const float WeightLow = 5.0f;
    public const float WeightNormal = 10.0f;
    public const float WeightHigh = 15.0f;
    public const float WeightVeryHigh = 20.0f;

    [DataField("weight")]
    public float Weight = WeightNormal;

    [DataField("startAnnouncement")]
    public string? StartAnnouncement;

    [DataField("endAnnouncement")]
    public string? EndAnnouncement;

    [DataField("startAudio")]
    public SoundSpecifier? StartAudio;

    [DataField("endAudio")]
    public SoundSpecifier? EndAudio;

    /// <summary>
    ///     In minutes, when is the first round time this event can start
    /// </summary>
    [DataField("earliestStart")]
    public int EarliestStart = 5;

    /// <summary>
    ///     In minutes, the amount of time before the same event can occur again
    /// </summary>
    [DataField("reoccurrenceDelay")]
    public int ReoccurrenceDelay = 30;

    /// <summary>
    ///     How long after being added does the event start
    /// </summary>
    [DataField("startDelay")]
    public TimeSpan StartDelay = TimeSpan.Zero;

    /// <summary>
    ///     How long the event lasts.
    /// </summary>
    [DataField("duration")]
    public TimeSpan? Duration = TimeSpan.FromSeconds(1);

    /// <summary>
    ///     The max amount of time the event lasts.
    /// </summary>
    [DataField("maxDuration")]
    public TimeSpan? MaxDuration;

    /// <summary>
    ///     Содержит настройки ивента, связанные с количеством игроков/должностей.
    /// </summary>
    /// <remarks>
    ///     To avoid running deadly events with low-pop
    /// </remarks>
    [DataField("spawnConfig")]
    public EventPlayersConfiguration SpawnConfiguration;

    /// <summary>
    ///     How many times this even can occur in a single round
    /// </summary>
    [DataField("maxOccurrences")]
    public int? MaxOccurrences;

    /// <summary>
    /// When the station event starts.
    /// </summary>
    [DataField("startTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan StartTime;

    /// <summary>
    /// When the station event ends.
    /// </summary>
    [DataField("endTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan? EndTime;
}

[UsedImplicitly]
[DataDefinition]
public sealed partial class EventPlayersConfiguration
{
    [DataField]
    public MinMaxPlayers StandartConfig = new();

    [DataField(customTypeSerializer: typeof(PrototypeIdDictionarySerializer<MinMaxPlayers, JobPrototype>))]
    public Dictionary<string, MinMaxPlayers> JobConfig = new();


    public bool IsEventPassed(IPlayerManager playerMan, JobSystem jobSystem, MindSystem mindSystem)
    {
        if (playerMan.PlayerCount < StandartConfig.MinPlayers
            || playerMan.PlayerCount > StandartConfig.MaxPlayers)
            return false;

        var jobNsessions = new Dictionary<string, int>();

        var sessions = playerMan.Sessions
            .Where(session => !session.ClientSide && session.Status == SessionStatus.InGame);

        foreach (var session in sessions)
        {
            if (!mindSystem.TryGetMind(session, out var mindId, out _))
                continue;

            if (!jobSystem.MindTryGetJobName(mindId, out var jobName))
                continue;

            if (!jobNsessions.TryAdd(jobName, 1))
                jobNsessions[jobName] += 1;
        }

        var jobConfigPassed = !JobConfig
            .Any(config => jobNsessions
                .Any(jobNsession => jobNsession.Key.Equals(config.Key) && (jobNsession.Value > config.Value.MaxPlayers || jobNsession.Value < config.Value.MinPlayers)));

        return jobConfigPassed;
    }

    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class MinMaxPlayers
    {
        [DataField]
        public int MinPlayers = 0;

        [DataField]
        public int MaxPlayers = 150;
    }
}
