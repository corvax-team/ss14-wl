using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared._WL.Audio.Jukebox;
using Content.Shared.Audio.Jukebox;
using Content.Shared.Examine;
using Content.Shared.Power;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio.Systems;

namespace Content.Server.Audio.Jukebox;

public sealed partial class JukeboxSystem : SharedJukeboxSystem
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly AppearanceSystem _appearanceSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        // WL-Changes-start
        SubscribeLocalEvent<JukeboxComponent, JukeboxVolumeChangedMessage>(OnJukeboxVolumeChanged);
        SubscribeLocalEvent<JukeboxComponent, ExaminedEvent>(OnExamined);
        // WL-Changes-end

        SubscribeLocalEvent<JukeboxComponent, JukeboxSelectedMessage>(OnJukeboxSelected);
        SubscribeLocalEvent<JukeboxComponent, JukeboxPlayingMessage>(OnJukeboxPlay);
        SubscribeLocalEvent<JukeboxComponent, JukeboxPauseMessage>(OnJukeboxPause);
        SubscribeLocalEvent<JukeboxComponent, JukeboxStopMessage>(OnJukeboxStop);
        SubscribeLocalEvent<JukeboxComponent, JukeboxSetTimeMessage>(OnJukeboxSetTime);
        SubscribeLocalEvent<JukeboxComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<JukeboxComponent, ComponentShutdown>(OnComponentShutdown);

        SubscribeLocalEvent<JukeboxComponent, PowerChangedEvent>(OnPowerChanged);
    }

    private void OnComponentInit(Entity<JukeboxComponent> ent, ref ComponentInit args)
    {
        if (HasComp<ApcPowerReceiverComponent>(ent))
        {
            TryUpdateVisualState(ent.AsNullable());
        }
    }

    private void OnJukeboxPlay(Entity<JukeboxComponent> ent, ref JukeboxPlayingMessage args)
    {
        TryPlay(ent.AsNullable());
    }

    private void StartPlaying(EntityUid uid, JukeboxComponent component)
    {
        if (string.IsNullOrEmpty(component.SelectedSongId) ||
            !_protoManager.Resolve(component.SelectedSongId, out var jukeboxProto))
        {
            return;
        }

        var @params = AudioParams.Default
            .WithVolume(SharedAudioSystem.GainToVolume(component.Gain))
            .WithMaxDistance(10f);

        var newAudio = Audio.PlayPvs(jukeboxProto.Path, uid, @params);
        component.AudioStream = newAudio?.Entity;

        Dirty(uid, component);
    }

    private void OnJukeboxVolumeChanged(EntityUid uid, JukeboxComponent component, ref JukeboxVolumeChangedMessage args)
    {
        var newGain = Math.Clamp(args.Volume, 0f, 1f);

        if (MathHelper.CloseTo(component.Gain, newGain))
            return;

        component.Gain = newGain;
        Audio.SetGain(component.AudioStream, newGain);

        Dirty(uid, component);
    }

    private void OnExamined(EntityUid uid, JukeboxComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (!_protoManager.TryIndex(component.SelectedSongId, out var proto) ||
            component.AudioStream == null ||
            !TryComp<AudioComponent>(component.AudioStream, out var audioComp) ||
            audioComp.State is AudioState.Paused)
        {
            args.PushMarkup(Loc.GetString("jukebox-examined-song-not-playing"), 1);
            return;
        }

        args.PushMarkup(Loc.GetString("jukebox-examined-song-playing", ("song", GetSongRepresentation(proto))), 1);
    }
    // WL-Changes-end

    private void OnJukeboxPause(Entity<JukeboxComponent> ent, ref JukeboxPauseMessage args)
    {
        Pause(ent.AsNullable());
    }

    private void OnJukeboxSetTime(Entity<JukeboxComponent> ent, ref JukeboxSetTimeMessage args)
    {
        if (TryComp(args.Actor, out ActorComponent? actorComp))
        {
            var offset = actorComp.PlayerSession.Channel.Ping * 1.5f / 1000f;
            SetTime(ent.AsNullable(), args.SongTime + offset);
        }
    }

    private void OnPowerChanged(Entity<JukeboxComponent> entity, ref PowerChangedEvent args)
    {
        TryUpdateVisualState(entity.AsNullable());

        if (!this.IsPowered(entity.Owner, EntityManager))
        {
            Stop(entity.AsNullable());
        }
    }

    private void OnJukeboxStop(Entity<JukeboxComponent> entity, ref JukeboxStopMessage args)
    {
        Stop(entity.AsNullable());
    }

    private void OnJukeboxSelected(EntityUid uid, JukeboxComponent component, JukeboxSelectedMessage args)
    {
        // WL-Changes-start
        var hasStream = Exists(component.AudioStream);

        if (args.SongId == component.SelectedSongId &&
            hasStream &&
            TryComp<AudioComponent>(component.AudioStream, out var audioComp))
        {
            var state = audioComp.State switch
            {
                AudioState.Playing => AudioState.Paused,
                AudioState.Paused => AudioState.Playing,
                _ => AudioState.Stopped
            };

            if (state is not AudioState.Stopped)
            {
                Audio.SetState(component.AudioStream, state);
                Dirty(uid, component);
                return;
            }
        }

        if (hasStream)
        {
            Audio.SetState(component.AudioStream, AudioState.Stopped);
            component.AudioStream = Audio.Stop(component.AudioStream);
        }

        component.SelectedSongId = args.SongId;

        DirectSetVisualState(uid, JukeboxVisualState.Select);
        component.Selecting = true;

        StartPlaying(uid, component);

        Dirty(uid, component);
        // WL-Changes-end
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<JukeboxComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Selecting)
            {
                comp.SelectAccumulator += frameTime;
                if (comp.SelectAccumulator >= 0.5f)
                {
                    comp.SelectAccumulator = 0f;
                    comp.Selecting = false;

                    TryUpdateVisualState((uid, comp));
                }
            }
        }
    }

    private void OnComponentShutdown(Entity<JukeboxComponent> ent, ref ComponentShutdown args)
    {
        ent.Comp.AudioStream = Audio.Stop(ent.Comp.AudioStream);
    }

    private void DirectSetVisualState(EntityUid uid, JukeboxVisualState state)
    {
        _appearanceSystem.SetData(uid, JukeboxVisuals.VisualState, state);
    }

    private void TryUpdateVisualState(Entity<JukeboxComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        var finalState = JukeboxVisualState.On;

        if (!this.IsPowered(ent, EntityManager))
        {
            finalState = JukeboxVisualState.Off;
        }

        _appearanceSystem.SetData(ent, JukeboxVisuals.VisualState, finalState);
    }

    /// <summary>
    /// Set the selected track of the jukebox to the specified prototype.
    /// </summary>
    public void SetSelectedTrack(Entity<JukeboxComponent?> ent, ProtoId<JukeboxPrototype> track)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        if (!Audio.IsPlaying(ent.Comp.AudioStream))
        {
            ent.Comp.SelectedSongId = track;
            DirectSetVisualState(ent, JukeboxVisualState.Select);
            ent.Comp.Selecting = true;
            ent.Comp.AudioStream = Audio.Stop(ent.Comp.AudioStream);
            Dirty(ent);
        }
    }

    /// <summary>
    /// Attempts to play the jukebox's current selected track.
    /// </summary>
    /// <returns>false if no track is selected or the track prototype cannot be found, otherwise true.</returns>
    public bool TryPlay(Entity<JukeboxComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        if (Exists(ent.Comp.AudioStream))
        {
            Audio.SetState(ent.Comp.AudioStream, AudioState.Playing);
            return true;
        }

        // WL-Changes: Jukebox tweaks start
        else
        {
            if (string.IsNullOrEmpty(ent.Comp.SelectedSongId) ||
                !_protoManager.Resolve(ent.Comp.SelectedSongId, out var jukeboxProto))
            {
                return false;
            }

            ent.Comp.AudioStream = Audio.Stop(ent.Comp.AudioStream);
            StartPlaying(ent, ent.Comp);
            return true;
        }
        // WL-Changes: Jukebox tweaks end
    }

    /// <summary>
    /// Stops any track that may currently be playing.
    /// </summary>
    public void Stop(Entity<JukeboxComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, logMissing: false))
            return;

        Audio.SetState(entity.Comp.AudioStream, AudioState.Stopped);
    }

    /// <summary>
    /// Pauses any track that may currently be playing.
    /// </summary>
    public void Pause(Entity<JukeboxComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, logMissing: false))
            return;

        Audio.SetState(entity.Comp.AudioStream, AudioState.Paused);
    }

    /// <summary>
    /// Sets the playback position within the current audio track.
    /// </summary>
    /// <remarks>
    /// If setting based on user input, you may need to compensate for the player's ping.
    /// </remarks>
    public void SetTime(Entity<JukeboxComponent?> entity, float songTime)
    {
        if (!Resolve(entity, ref entity.Comp))
            return;

        Audio.SetPlaybackPosition(entity.Comp.AudioStream, songTime);
    }
}
