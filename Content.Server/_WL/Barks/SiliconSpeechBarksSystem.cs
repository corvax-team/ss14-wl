using Content.Shared._CorvaxNext.Silicons.Borgs.Components;
using Content.Shared._WL.Barks;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Barks;

/// <summary>
/// Uses the station AI voice while it is remotely controlling a borg.
/// </summary>
public sealed partial class SiliconSpeechBarksSystem : EntitySystem
{
    private static readonly ProtoId<BarkPrototype> StationAiVoice = "Sam";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AiRemoteControllerComponent, TransformSpeakerBarkEvent>(OnTransformSpeakerBark);
    }

    private void OnTransformSpeakerBark(
        Entity<AiRemoteControllerComponent> ent,
        ref TransformSpeakerBarkEvent args)
    {
        if (ent.Comp.AiHolder == null)
            return;

        args.Voice = StationAiVoice;
        args.Pitch = SpeechBarksComponent.DefaultPitch;
        args.MinDelay = SpeechBarksComponent.DefaultMinDelay;
        args.MaxDelay = SpeechBarksComponent.DefaultMaxDelay;
        args.PlayOnRadio = false;
    }
}
