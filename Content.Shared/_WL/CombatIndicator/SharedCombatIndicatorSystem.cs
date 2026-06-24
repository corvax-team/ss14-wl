using Content.Shared.Humanoid;
using Content.Shared.Input;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared._WL.CombatIndicator;

public sealed partial class SharedCombatIndicatorSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeInteractions();
    }

    private void InitializeInteractions()
    {
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.CombatIndicator, InputCmdHandler.FromDelegate(ToggleCombatIndicator, handle: false, outsidePrediction: false))
            .Register<SharedCombatIndicatorSystem>();
    }

    public override void Shutdown()
    {
        base.Shutdown();

        CommandBinds.Unregister<SharedCombatIndicatorSystem>();
    }


    private void ToggleCombatIndicator(ICommonSession? session)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (session?.AttachedEntity is not { Valid: true } uid)
            return;

        if (!HasComp<HumanoidProfileComponent>(uid)) //only for people, I hope
            return;

        if (HasComp<CombatIndicatorComponent>(uid))
        {
            RemComp<CombatIndicatorComponent>(uid);
        }
        else
        {
            var indicator = EnsureComp<CombatIndicatorComponent>(uid);
            _audio.PlayPredicted(indicator.IndicatorSound, uid, uid);
            Dirty(uid, indicator);
        }
    }
}

