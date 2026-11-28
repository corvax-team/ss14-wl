using Content.Shared._WL.CCVars;
using Content.Shared._WL.CombatIndicator;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Client._WL.CombatIndicator;

public sealed partial class CombatIndicatorSystem : SharedCombatIndicatorSystem
{
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private CombatIndicatorSystem _combatIndicator = default!;

    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(_cfg, WLCVars.CombatIndicatorAutoEnable, OnCombatIndicatorAutoEnableChanged, true);
    }

    [SubscribeLocalEvent]
    private void OnLocalPlayerAttached(LocalPlayerAttachedEvent args)
    {
        SendCombatIndicatorAutoEnable();
    }

    private void OnCombatIndicatorAutoEnableChanged(bool _)
    {
        SendCombatIndicatorAutoEnable();
    }

    private void SendCombatIndicatorAutoEnable()
    {
        if (_playerManager.LocalSession?.AttachedEntity is null)
            return;

        RaiseNetworkEvent(new SetCombatIndicatorAutoEnableEvent(_cfg.GetCVar(WLCVars.CombatIndicatorAutoEnable)));
    }
}
