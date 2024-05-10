using Content.Shared._WL.PulseDemon;
using Content.Shared.VendingMachines;
using Robust.Shared.Timing;

namespace Content.Server._WL.PulseDemon.TamperActions;

public sealed partial class VendingMachineAction : ElectromagneticTamperAction
{
    public override bool Action(ElectromagneticTamperActionArgs args)
    {
        var _entityManager = args.EntityManager;
        var _gameTime = IoCManager.Resolve<IGameTiming>();

        if (!_entityManager.TryGetComponent<VendingMachineComponent>(args.TargetUid, out var vendingMachineComp))
            return false;

        vendingMachineComp.CanShoot = true;
        vendingMachineComp.NextEmpEject = _gameTime.CurTime;

        return true;
    }
}
