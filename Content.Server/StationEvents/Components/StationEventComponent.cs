using Content.Server.Mind;
using Content.Server.Roles.Jobs;
using Content.Shared.Mind;
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

    [DataField]
    public float Weight = WeightNormal;

    [DataField]
    public string? StartAnnouncement;

    [DataField]
    public string? EndAnnouncement;

    [DataField]
    public Color StartAnnouncementColor = Color.Gold;

    [DataField]
    public Color EndAnnouncementColor = Color.Gold;

    [DataField]
    public SoundSpecifier? StartAudio;

    [DataField]
    public SoundSpecifier? EndAudio;

    /// <summary>
    ///     In minutes, when is the first round time this event can start
    /// </summary>
    [DataField]
    public int EarliestStart = 5;

    /// <summary>
    ///     In minutes, the amount of time before the same event can occur again
    /// </summary>
    [DataField]
    public int ReoccurrenceDelay = 30;

    /// <summary>
    ///     How long the event lasts.
    /// </summary>
    [DataField]
    public TimeSpan? Duration = TimeSpan.FromSeconds(1);

    /// <summary>
    ///     The max amount of time the event lasts.
    /// </summary>
    [DataField]
    public TimeSpan? MaxDuration;

    //WL-Changes-start
    /// <summary>
    ///     Содержит настройки ивента, связанные с количеством игроков/должностей.
    /// </summary>
    /// <remarks>
    ///     To avoid running deadly events with low-pop
    /// </remarks>
    [DataField("spawnConfig")]
    public EventPlayersConfiguration? SpawnConfiguration = null;
    //WL-Changes-end

    /// <summary>
    ///     How many times this even can occur in a single round
    /// </summary>
    [DataField]
    public int? MaxOccurrences;

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

    public bool IsEventPassed(IEntityManager entMan, JobSystem jobSystem, MindSystem mindSystem, int playerCount)
    {
        if (playerCount < StandartConfig.MinPlayers
            || playerCount > StandartConfig.MaxPlayers)
            return false;

        if (JobConfig.Count > 0)
        {
            var jobNsessions = new Dictionary<string, int>();

            var minds = entMan.EntityQueryEnumerator<MindComponent>();

            while (minds.MoveNext(out var mindId, out var mindComponent))
            {
                if (!jobSystem.MindTryGetJob(mindId, out _, out var jobProto))
                    continue;

                if (!jobNsessions.TryAdd(jobProto.ID, 1))
                    jobNsessions[jobProto.ID] += 1;
            }

            var jobConfigPassed = !JobConfig
                .Any(config => jobNsessions
                    .Any(jobNsession => jobNsession.Key.Equals(config.Key) && (jobNsession.Value > config.Value.MaxPlayers || jobNsession.Value < config.Value.MinPlayers)));

            return jobConfigPassed;
        }
        else return true;
    }
}

[UsedImplicitly]
[DataDefinition]
public sealed partial class MinMaxPlayers
{
    [DataField]
    public int MinPlayers = 0;

    [DataField]
    public int MaxPlayers = 350;
}
