using Content.Shared._WL.Barks;
using Content.Shared.Corvax.TTS;
using Content.Shared.Implants;
using Content.Shared.Inventory;
using Content.Shared.VoiceMask;

namespace Content.Server.VoiceMask;

/*
 * WL-Changes: Barks
 * Maded for barks system
 * TODO: Move to the bark system into Corvax.Server/_WL/Barks
 *
 */

public partial class VoiceMaskSystem
{
    private static void TransformBark(VoiceMaskComponent component, ref TransformSpeakerBarkEvent args)
    {
        if (!component.Active)
            return;

        args.Voice = component.BarkVoice;
        args.Pitch = component.BarkPitch;
    }

    [SubscribeLocalEvent]
    private void OnSpeakerBarkTransform(
        EntityUid uid,
        VoiceMaskComponent component,
        InventoryRelayedEvent<TransformSpeakerBarkEvent> args)
    {
        TransformBark(component, ref args.Args);
    }

    [SubscribeLocalEvent]
    private void OnSpeakerBarkTransformImplant(
        EntityUid uid,
        VoiceMaskComponent component,
        ImplantRelayEvent<TransformSpeakerBarkEvent> args)
    {
        TransformBark(component, ref args.Args);
    }

    [SubscribeLocalEvent]
    private void OnInnateSpeakerBarkTransform(
        EntityUid uid,
        VoiceMaskComponent component,
        ref TransformSpeakerBarkEvent args)
    {
        TransformBark(component, ref args);
    }

    [SubscribeLocalEvent]
    private void OnChangeBark(Entity<VoiceMaskComponent> entity, ref VoiceMaskChangeBarkMessage msg)
    {
        if (!ProtoMan.TryIndex<BarkPrototype>(msg.Bark, out var bark) || !bark.RoundStart)
            return;

        entity.Comp.BarkVoice = msg.Bark;
        _popupSystem.PopupEntity(Loc.GetString("voice-mask-voice-popup-success"), entity);
        UpdateUI(entity);
    }

    [SubscribeLocalEvent]
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
}
