using Content.Shared.Chat;
using Content.Shared._WL.Barks; // WL-Changes
using Content.Shared.Speech.Components;

namespace Content.Shared.Speech.EntitySystems;

public sealed partial class VoiceOverrideSystem : EntitySystem
{
    [SubscribeLocalEvent]
    private void OnTransformSpeakerName(Entity<VoiceOverrideComponent> ent, ref TransformSpeakerNameEvent args)
    {
        if (!ent.Comp.Enabled)
            return;

        args.VoiceName = ent.Comp.NameOverride ?? args.VoiceName;
        args.SpeechVerb = ent.Comp.SpeechVerbOverride ?? args.SpeechVerb;
    }

    // WL-Changes-Start: Speech barks
    [SubscribeLocalEvent]
    private void OnTransformSpeakerBark(Entity<VoiceOverrideComponent> entity, ref TransformSpeakerBarkEvent args)
    {
        if (!entity.Comp.Enabled)
            return;

        args.Voice = entity.Comp.BarkVoiceOverride ?? args.Voice;
        args.Pitch = entity.Comp.BarkPitchOverride ?? args.Pitch;
        args.MinDelay = entity.Comp.BarkMinDelayOverride ?? args.MinDelay;
        args.MaxDelay = entity.Comp.BarkMaxDelayOverride ?? args.MaxDelay;
    }
    // WL-Changes-End
}
