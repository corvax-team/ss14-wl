using Content.Shared.Chat;
using Content.Shared._WL.Barks; // WL-Changes
using Content.Server.Speech.Components;

namespace Content.Server.Speech.EntitySystems;

public sealed partial class VoiceOverrideSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VoiceOverrideComponent, TransformSpeakerNameEvent>(OnTransformSpeakerName);
        SubscribeLocalEvent<VoiceOverrideComponent, TransformSpeakerBarkEvent>(OnTransformSpeakerBark); // WL-Changes
    }

    private void OnTransformSpeakerName(Entity<VoiceOverrideComponent> entity, ref TransformSpeakerNameEvent args)
    {
        if (!entity.Comp.Enabled)
            return;

        args.VoiceName = entity.Comp.NameOverride ?? args.VoiceName;
        args.SpeechVerb = entity.Comp.SpeechVerbOverride ?? args.SpeechVerb;
    }

    // WL-Changes-Start: Speech barks
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
