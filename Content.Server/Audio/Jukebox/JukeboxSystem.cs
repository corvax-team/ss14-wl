using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared._WL.Audio.Jukebox;
using Content.Shared.Audio.Jukebox;
using Content.Shared.Examine;
using Content.Shared.Power;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using JukeboxComponent = Content.Shared.Audio.Jukebox.JukeboxComponent;

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

    private void OnComponentInit(EntityUid uid, JukeboxComponent component, ComponentInit args)
    {
        if (HasComp<ApcPowerReceiverComponent>(uid))
        {
            TryUpdateVisualState(uid, component);
        }
    }

    // WL-Changes-start
    private void OnJukeboxPlay(EntityUid uid, JukeboxComponent component, ref JukeboxPlayingMessage args)
    {
        if (Exists(component.AudioStream))
        {
            Audio.SetState(component.AudioStream, AudioState.Playing);
        }
        else
        {
            component.AudioStream = Audio.Stop(component.AudioStream);
            StartPlaying(uid, component);
        }
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
        Audio.SetState(ent.Comp.AudioStream, AudioState.Paused);
    }

    private void OnJukeboxSetTime(EntityUid uid, JukeboxComponent component, JukeboxSetTimeMessage args)
    {
        if (TryComp(args.Actor, out ActorComponent? actorComp))
        {
            var offset = actorComp.PlayerSession.Channel.Ping * 1.5f / 1000f;
            Audio.SetPlaybackPosition(component.AudioStream, args.SongTime + offset);
        }
    }

    private void OnPowerChanged(Entity<JukeboxComponent> entity, ref PowerChangedEvent args)
    {
        TryUpdateVisualState(entity);

        if (!this.IsPowered(entity.Owner, EntityManager))
        {
            Stop(entity);
        }
    }

    private void OnJukeboxStop(Entity<JukeboxComponent> entity, ref JukeboxStopMessage args)
    {
        Stop(entity);
    }

    private void Stop(Entity<JukeboxComponent> entity)
    {
        Audio.SetState(entity.Comp.AudioStream, AudioState.Stopped);
        Dirty(entity);
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

                    TryUpdateVisualState(uid, comp);
                }
            }
        }
    }

    private void OnComponentShutdown(EntityUid uid, JukeboxComponent component, ComponentShutdown args)
    {
        component.AudioStream = Audio.Stop(component.AudioStream);
    }

    private void DirectSetVisualState(EntityUid uid, JukeboxVisualState state)
    {
        _appearanceSystem.SetData(uid, JukeboxVisuals.VisualState, state);
    }

    private void TryUpdateVisualState(EntityUid uid, JukeboxComponent? jukeboxComponent = null)
    {
        if (!Resolve(uid, ref jukeboxComponent))
            return;

        var finalState = JukeboxVisualState.On;

        if (!this.IsPowered(uid, EntityManager))
        {
            finalState = JukeboxVisualState.Off;
        }

        _appearanceSystem.SetData(uid, JukeboxVisuals.VisualState, finalState);
    }
}
