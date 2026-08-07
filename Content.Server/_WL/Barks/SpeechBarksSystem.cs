using Content.Server.Chat.Systems;
using Content.Shared.Chat;
using Content.Shared._WL.Barks;
using Robust.Shared.Prototypes;
using Robust.Shared.Player;

namespace Content.Server._WL.Barks;

public sealed partial class SpeechBarksSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototypes = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpeechBarksComponent, EntitySpokeEvent>(OnEntitySpoke);
    }

    private void OnEntitySpoke(Entity<SpeechBarksComponent> ent, ref EntitySpokeEvent args)
    {
        var transform = new TransformSpeakerBarkEvent(
            ent,
            ent.Comp.Voice,
            ent.Comp.Pitch,
            ent.Comp.MinDelay,
            ent.Comp.MaxDelay,
            ent.Comp.PlayOnRadio);
        RaiseLocalEvent(ent, transform);

        if (args.WasRadio && !transform.PlayOnRadio)
            return;

        if (!_prototypes.TryIndex(transform.Voice, out var voice))
            return;

        var pitch = SpeechBarksComponent.SanitizePitch(transform.Pitch);
        var (minDelay, maxDelay) =
            SpeechBarksComponent.SanitizeDelays(transform.MinDelay, transform.MaxDelay);
        var prosody = BarkProsody.FromMessage(args.Message);
        var rhythm = BarkProsody.GetBarkRhythm(args.Message);
        if (rhythm.Length == 0)
            return;

        var ev = new PlaySpeechBarksEvent(
            GetNetEntity(ent),
            voice.Sound,
            pitch,
            minDelay,
            maxDelay,
            rhythm,
            prosody,
            args.ObfuscatedMessage != null);

        RaiseNetworkEvent(ev, Filter.Pvs(ent));
    }
}
