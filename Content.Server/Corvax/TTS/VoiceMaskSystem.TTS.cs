using Content.Shared._WL.Barks; // WL-Changes
using Content.Shared.Corvax.TTS;
using Content.Shared.Implants;
using Content.Shared.Inventory;
using Content.Shared.VoiceMask;

namespace Content.Server.VoiceMask;

public partial class VoiceMaskSystem
{
    private void InitializeTTS()
    {
        SubscribeLocalEvent<VoiceMaskComponent, InventoryRelayedEvent<TransformSpeakerVoiceEvent>>(OnSpeakerVoiceTransform);
        SubscribeLocalEvent<VoiceMaskComponent, VoiceMaskChangeVoiceMessage>(OnChangeVoice);
        SubscribeLocalEvent<VoiceMaskComponent, ImplantRelayEvent<TransformSpeakerVoiceEvent>>(OnSpeakerVoiceTransformImplant);
        // WL-Changes-Start: Speech barks
        SubscribeLocalEvent<VoiceMaskComponent, InventoryRelayedEvent<TransformSpeakerBarkEvent>>(OnSpeakerBarkTransform);
        SubscribeLocalEvent<VoiceMaskComponent, ImplantRelayEvent<TransformSpeakerBarkEvent>>(OnSpeakerBarkTransformImplant);
        SubscribeLocalEvent<VoiceMaskComponent, TransformSpeakerBarkEvent>(OnInnateSpeakerBarkTransform);
        SubscribeLocalEvent<VoiceMaskComponent, VoiceMaskChangeBarkMessage>(OnChangeBark);
        SubscribeLocalEvent<VoiceMaskComponent, VoiceMaskChangeBarkPitchMessage>(OnChangeBarkPitch);
        // WL-Changes-End
    }

    private void OnSpeakerVoiceTransform(EntityUid uid, VoiceMaskComponent component, InventoryRelayedEvent<TransformSpeakerVoiceEvent> args)
    {
        if (!component.Active)
            return;
        args.Args.VoiceId = component.VoiceId;
    }

    private void OnChangeVoice(Entity<VoiceMaskComponent> entity, ref VoiceMaskChangeVoiceMessage msg)
    {
        if (msg.Voice is { } id && !ProtoMan.HasIndex<TTSVoicePrototype>(id))
            return;

        entity.Comp.VoiceId = msg.Voice;

        _popupSystem.PopupEntity(Loc.GetString("voice-mask-voice-popup-success"), entity);

        UpdateUI(entity);
    }
    private void OnSpeakerVoiceTransformImplant(EntityUid uid, VoiceMaskComponent component, ImplantRelayEvent<TransformSpeakerVoiceEvent> args)
    {
        if (!component.Active)
            return;
        args.Args.VoiceId = component.VoiceId;
    }

    // WL-Changes-Start: Speech barks
    private static void TransformBark(VoiceMaskComponent component, TransformSpeakerBarkEvent args)
    {
        if (!component.Active)
            return;

        args.Voice = component.BarkVoice;
        args.Pitch = component.BarkPitch;
    }

    private void OnSpeakerBarkTransform(
        EntityUid uid,
        VoiceMaskComponent component,
        InventoryRelayedEvent<TransformSpeakerBarkEvent> args)
    {
        TransformBark(component, args.Args);
    }

    private void OnSpeakerBarkTransformImplant(
        EntityUid uid,
        VoiceMaskComponent component,
        ImplantRelayEvent<TransformSpeakerBarkEvent> args)
    {
        TransformBark(component, args.Args);
    }

    private void OnInnateSpeakerBarkTransform(
        EntityUid uid,
        VoiceMaskComponent component,
        ref TransformSpeakerBarkEvent args)
    {
        TransformBark(component, args);
    }

    private void OnChangeBark(Entity<VoiceMaskComponent> entity, ref VoiceMaskChangeBarkMessage msg)
    {
        if (!ProtoMan.TryIndex<BarkPrototype>(msg.Bark, out var bark) || !bark.RoundStart)
            return;

        entity.Comp.BarkVoice = msg.Bark;
        _popupSystem.PopupEntity(Loc.GetString("voice-mask-voice-popup-success"), entity);
        UpdateUI(entity);
    }

    private void OnChangeBarkPitch(Entity<VoiceMaskComponent> entity, ref VoiceMaskChangeBarkPitchMessage msg)
    {
        if (!float.IsFinite(msg.Pitch))
            return;

        entity.Comp.BarkPitch = Math.Clamp(
            msg.Pitch,
            SpeechBarksComponent.MinPitch,
            SpeechBarksComponent.MaxPitch);
        _popupSystem.PopupEntity(Loc.GetString("voice-mask-voice-popup-success"), entity);
        UpdateUI(entity);
    }
    // WL-Changes-End
}
