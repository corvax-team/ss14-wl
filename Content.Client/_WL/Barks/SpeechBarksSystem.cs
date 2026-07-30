using Content.Shared.Chat;
using Content.Shared._WL.Barks;
using Content.Shared._WL.CCVars;
using Robust.Client.Audio;
using Robust.Client.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.Client._WL.Barks;

public sealed partial class SpeechBarksSystem : EntitySystem
{
    private const float MinimalVolume = -10f;
    private const float WhisperFade = 4f;
    // ADT uses a clearly audible, uniform +/- 0.1 variation for every grain.
    // It is especially important for very short voices such as Temmie.
    private const float PitchVariation = 0.1f;
    private const float MinPlaybackPitch = 0.45f;
    private const float MaxPlaybackPitch = 1.85f;

    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IPrototypeManager _prototypes = default!;

    private readonly List<ActiveBark> _active = new();
    private readonly HashSet<EntityUid> _previewStreams = new();
    private SpeechMode _speechMode;
    private float _volume;

    public override void Initialize()
    {
        base.Initialize();
        _cfg.OnValueChanged(WLCVars.BarksVolume, OnVolumeChanged, true);
        _cfg.OnValueChanged(WLCVars.SpeechMode, OnSpeechModeChanged, true);
        SubscribeNetworkEvent<PlaySpeechBarksEvent>(OnBarks);
    }

    public override void Shutdown()
    {
        StopPreview();
        _cfg.UnsubValueChanged(WLCVars.BarksVolume, OnVolumeChanged);
        _cfg.UnsubValueChanged(WLCVars.SpeechMode, OnSpeechModeChanged);
        _active.Clear();
        base.Shutdown();
    }

    private void OnVolumeChanged(float value)
    {
        _volume = value;
    }

    private float AdjustVolume(BarkProsody prosody, bool isWhisper)
    {
        var volume = MinimalVolume +
            SharedAudioSystem.GainToVolume(_volume) +
            prosody.VolumeOffset;

        return isWhisper ? volume - WhisperFade : volume;
    }

    private void OnSpeechModeChanged(SpeechMode mode)
    {
        _speechMode = mode;
        if (mode != SpeechMode.Barks)
            _active.RemoveAll(bark => !bark.IsPreview);
    }

    private void OnBarks(PlaySpeechBarksEvent ev)
    {
        if (_speechMode != SpeechMode.Barks ||
            !TryGetEntity(ev.Source, out var source) ||
            Transform(source.Value).MapID == MapId.Nullspace)
            return;

        var pitch = SpeechBarksComponent.SanitizePitch(ev.Pitch);
        var (minDelay, maxDelay) =
            SpeechBarksComponent.SanitizeDelays(ev.MinDelay, ev.MaxDelay);
        // Keep ADT's speech behaviour: every message and every already-started
        // grain is allowed to finish independently. This is what makes very
        // short voice collections sound like natural game dialogue instead of
        // a stream repeatedly cut by a concurrency limiter.
        QueueBark(
            source.Value,
            ev.Sound,
            pitch,
            ev.IsWhisper ? SharedChatSystem.WhisperMuffledRange : SharedChatSystem.VoiceRange,
            AdjustVolume(ev.Prosody, ev.IsWhisper),
            minDelay,
            maxDelay,
            Math.Max(1, ev.Count),
            ev.Prosody,
            false);
    }

    public void PlayDataPreview(
        ProtoId<BarkPrototype> voiceId,
        float pitch,
        float minDelay,
        float maxDelay,
        string message)
    {
        if (_player.LocalSession == null || !_prototypes.TryIndex(voiceId, out var voice))
            return;

        StopPreview();

        pitch = SpeechBarksComponent.SanitizePitch(pitch);
        (minDelay, maxDelay) = SpeechBarksComponent.SanitizeDelays(minDelay, maxDelay);
        var prosody = BarkProsody.FromMessage(message);

        QueueBark(
            null,
            voice.Sound,
            pitch,
            SharedChatSystem.VoiceRange,
            AdjustVolume(prosody, false),
            minDelay,
            maxDelay,
            BarkProsody.GetBarkCount(message),
            prosody,
            true);
    }

    private void QueueBark(
        EntityUid? source,
        SoundSpecifier sound,
        float pitch,
        float distance,
        float volume,
        float minDelay,
        float maxDelay,
        int count,
        BarkProsody prosody,
        bool isPreview)
    {
        _active.Add(new ActiveBark(
            source,
            sound,
            pitch,
            distance,
            volume,
            minDelay,
            maxDelay,
            count,
            prosody,
            isPreview));
    }

    public void StopPreview()
    {
        foreach (var stream in _previewStreams)
        {
            if (!Deleted(stream))
                _audio.Stop(stream);
        }

        _previewStreams.Clear();
        _active.RemoveAll(bark => bark.IsPreview);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_player.LocalSession == null)
            return;

        for (var i = _active.Count - 1; i >= 0; i--)
        {
            var bark = _active[i];
            if (bark.NextSound > _timing.CurTime)
                continue;

            if (bark.Played >= bark.Count || bark.Source is { } source && Deleted(source))
            {
                _active.RemoveAt(i);
                continue;
            }

            var progress = bark.Count <= 1
                ? 1f
                : bark.Played / (bark.Count - 1f);
            var variation = _random.NextFloat(1f - PitchVariation, 1f + PitchVariation);
            var pitch = Math.Clamp(
                bark.Pitch * bark.Prosody.GetPitchMultiplier(progress) * variation,
                MinPlaybackPitch,
                MaxPlaybackPitch);

            var parameters = AudioParams.Default
                .WithPitchScale(pitch)
                .WithVolume(bark.Volume)
                .WithMaxDistance(bark.Distance);

            var sound = _audio.ResolveSound(bark.Sound);
            var playbackDuration = TimeSpan.FromSeconds(
                _audio.GetAudioLength(sound).TotalSeconds / pitch);
            EntityUid? streamEntity = null;

            if (bark.Source == null || bark.Source == _player.LocalEntity)
            {
                var stream = _audio.PlayGlobal(sound, _player.LocalSession, parameters);
                if (stream is { } globalStream)
                {
                    streamEntity = globalStream.Entity;
                    if (bark.IsPreview)
                        _previewStreams.Add(globalStream.Entity);
                }
            }
            else
            {
                var stream = _audio.PlayEntity(
                    sound,
                    _player.LocalSession,
                    bark.Source.Value,
                    parameters);
                if (stream is { } entityStream)
                    streamEntity = entityStream.Entity;
            }

            ExtendAudioLifetime(streamEntity, playbackDuration);

            bark.Played++;
            var cadence = _random.NextFloat(bark.MinDelay, bark.MaxDelay);
            cadence *= bark.Prosody.DelayScale;
            bark.NextSound = _timing.CurTime + TimeSpan.FromSeconds(cadence);
        }
    }

    private void ExtendAudioLifetime(EntityUid? streamEntity, TimeSpan playbackDuration)
    {
        if (streamEntity == null ||
            !TryComp<TimedDespawnComponent>(streamEntity.Value, out var timedDespawn))
            return;

        var requiredLifetime =
            (float) playbackDuration.TotalSeconds +
            SharedAudioSystem.AudioDespawnBuffer;
        timedDespawn.Lifetime = Math.Max(timedDespawn.Lifetime, requiredLifetime);
    }

    private sealed class ActiveBark(
        EntityUid? source,
        SoundSpecifier sound,
        float pitch,
        float distance,
        float volume,
        float minDelay,
        float maxDelay,
        int count,
        BarkProsody prosody,
        bool isPreview = false)
    {
        public EntityUid? Source = source;
        public SoundSpecifier Sound = sound;
        public float Pitch = pitch;
        public float Distance = distance;
        public float Volume = volume;
        public float MinDelay = minDelay;
        public float MaxDelay = maxDelay;
        public int Count = count;
        public BarkProsody Prosody = prosody;
        public bool IsPreview = isPreview;
        public int Played;
        public TimeSpan NextSound;
    }

}
