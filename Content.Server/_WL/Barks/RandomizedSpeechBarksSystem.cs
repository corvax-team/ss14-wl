using Content.Shared._WL.Barks;
using Robust.Shared.Random;

namespace Content.Server._WL.Barks;

/// <summary>
/// Gives each marked entity stable, server-authoritative bark settings for its lifetime.
/// </summary>
public sealed partial class RandomizedSpeechBarksSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RandomizedSpeechBarksComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<RandomizedSpeechBarksComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<SpeechBarksComponent>(ent, out var barks))
            return;

        barks.Pitch = SpeechBarksComponent.SanitizePitch(
            _random.NextFloat(ent.Comp.MinPitch, ent.Comp.MaxPitch));

        var minDelay = _random.NextFloat(
            ent.Comp.MinDelayLowerBound,
            ent.Comp.MinDelayUpperBound);
        var maxDelay = _random.NextFloat(
            ent.Comp.MaxDelayLowerBound,
            ent.Comp.MaxDelayUpperBound);
        (barks.MinDelay, barks.MaxDelay) = SpeechBarksComponent.SanitizeDelays(minDelay, maxDelay);
        Dirty(ent.Owner, barks);
    }
}
