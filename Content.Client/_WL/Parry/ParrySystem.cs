using Content.Shared._WL.Parry;
using Content.Shared.Input;
using Robust.Client.Graphics;
using Robust.Shared.Input.Binding;
using Robust.Shared.Timing;

namespace Content.Client._WL.Parry;

public sealed partial class ParrySystem : SharedParrySystem
{
    [Dependency] private IOverlayManager _overlayManager = default!;
    [Dependency] private IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.Parry, InputCmdHandler.FromDelegate(
                enabled: _ =>
                {
                    if (!_gameTiming.IsFirstTimePredicted)
                        return;

                    RaisePredictiveEvent(new ParryStartEvent());
                },
                disabled: _ =>
                {
                    if (!_gameTiming.IsFirstTimePredicted)
                        return;

                    RaisePredictiveEvent(new ParryStopEvent());
                },
                handle: false,
                outsidePrediction: true))
            .Register<ParrySystem>();

        _overlayManager.AddOverlay(new ParryOverlay());
    }

    public override void Shutdown()
    {
        _overlayManager.RemoveOverlay<ParryOverlay>();
        CommandBinds.Unregister<ParrySystem>();

        base.Shutdown();
    }
}
