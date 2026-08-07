using Content.Server.Speech.Components;
using Content.Shared._WL.Barks;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Barks;

/// <summary>
/// Keeps the skeleton bark voice after random humanoid profiles are applied.
/// </summary>
public sealed partial class SkeletonSpeechBarksSystem : EntitySystem
{
    private static readonly ProtoId<BarkPrototype> SkeletonVoice = "SkeletonBarkOne";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SkeletonAccentComponent, TransformSpeakerBarkEvent>(OnTransformSpeakerBark);
    }

    private void OnTransformSpeakerBark(
        Entity<SkeletonAccentComponent> ent,
        ref TransformSpeakerBarkEvent args)
    {
        args.Voice = SkeletonVoice;
    }
}
