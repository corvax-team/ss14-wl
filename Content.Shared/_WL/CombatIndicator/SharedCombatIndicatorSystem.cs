using Content.Shared.Input;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared._WL.CombatIndicator;

public abstract partial class SharedCombatIndicatorSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.CombatIndicator, InputCmdHandler.FromDelegate(ToggleCombatIndicator, handle: false, outsidePrediction: false))
            .Register<SharedCombatIndicatorSystem>();
    }

    public override void Shutdown()
    {
        base.Shutdown();

        CommandBinds.Unregister<SharedCombatIndicatorSystem>();
    }

    [SubscribeLocalEvent]
    private void OnPlayerDetached(PlayerDetachedEvent ev)
    {
        TrySetAutoEnable(ev.Entity, false);
    }

    [SubscribeLocalEvent]
    private void OnCombatModeChanged(Entity<CombatIndicatorComponent> ent, ref CombatModeChangedEvent args)
    {
        if (!ent.Comp.AutoEnable)
            return;

        TrySetEnabled(ent, args.Enabled);
    }

    [SubscribeNetworkEvent]
    private void OnCombatIndicatorAutoEnableChanged(SetCombatIndicatorAutoEnableEvent ev, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } entity)
            return;

        TrySetAutoEnable(entity, ev.Enabled);
    }

    private void ToggleCombatIndicator(ICommonSession? session)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (session?.AttachedEntity is not { Valid: true } uid)
            return;

        TrySetEnabled(uid);
    }

    public bool TrySetAutoEnable(EntityUid uid, bool value)
    {
        if (!TryComp<CombatIndicatorComponent>(uid, out var indicator))
            return false;

        if (indicator.AutoEnable != value)
        {
            indicator.AutoEnable = value;

            Dirty(uid, indicator);
        }

        return true;
    }

    public bool TrySetEnabled(EntityUid uid, bool? value = null, bool playSound = true)
    {
        if (!TryComp<CombatIndicatorComponent>(uid, out var indicator))
            return false;

        value ??= !indicator.Enabled;

        var status = value ?? !indicator.Enabled;

        if (indicator.Enabled != status)
        {
            indicator.Enabled = status;

            if (playSound)
                _audio.PlayPredicted(indicator.IndicatorSound, uid, uid);

            var ev = new CombatIndicatorChangedEvent(status);
            RaiseLocalEvent(uid, ref ev);

            Dirty(uid, indicator);
        }

        return true;
    }
}

