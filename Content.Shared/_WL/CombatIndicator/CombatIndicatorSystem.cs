using Content.Shared.Humanoid;
using Content.Shared.Input;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared._WL.CombatIndicator;

public sealed partial class CombatIndicatorSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.CombatIndicator, InputCmdHandler.FromDelegate(ToggleCombatIndicator, handle: false, outsidePrediction: false))
            .Register<CombatIndicatorSystem>();
    }

    public override void Shutdown()
    {
        base.Shutdown();

        CommandBinds.Unregister<CombatIndicatorSystem>();
    }

    [SubscribeLocalEvent]
    private void OnPlayerDetached(PlayerDetachedEvent ev)
    {
        RemComp<CombatIndicatorAutoEnableComponent>(ev.Entity);
    }

    private void ToggleCombatIndicator(ICommonSession? session)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (session?.AttachedEntity is not { Valid: true } uid)
            return;

        var enabled = !HasComp<CombatIndicatorComponent>(uid);

        TrySetEnabled(uid, enabled);
    }

    public bool TrySetEnabled(EntityUid uid, bool value, bool playSound = true)
    {
        if (!HasComp<HumanoidProfileComponent>(uid))
            return false;

        if (value)
        {
            if (HasComp<CombatIndicatorComponent>(uid))
                return true;

            var indicator = EnsureComp<CombatIndicatorComponent>(uid);

            if (playSound)
                _audio.PlayPredicted(indicator.IndicatorSound, uid, uid);
        }
        else
        {
            RemComp<CombatIndicatorComponent>(uid);
        }

        return true;
    }
}

