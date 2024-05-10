using Content.Server.Wires;
using Content.Shared._WL.PulseDemon;
using Content.Shared.Emag.Systems;
using Content.Shared.Lock;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Wires;

namespace Content.Server._WL.PulseDemon.TamperActions
{
    public sealed partial class SiliconLawBound : ElectromagneticTamperAction
    {
        public override bool Action(ElectromagneticTamperActionArgs args)
        {
            var _entityMan = args.EntityManager;
            var _emag = _entityMan.System<EmagSystem>();
            var _wires = _entityMan.System<WiresSystem>();

            if (!_entityMan.TryGetComponent<SiliconLawProviderComponent>(args.TargetUid, out _) ||
                !_entityMan.TryGetComponent<WiresPanelComponent>(args.TargetUid, out var wiresPanelComp))
                return false;

            _entityMan.RemoveComponent<LockedWiresPanelComponent>(args.TargetUid);

            _wires.TogglePanel(args.TargetUid, wiresPanelComp, true);
            var result = _emag.DoEmagEffect(args.DemonUid, args.TargetUid);
            _wires.TogglePanel(args.TargetUid, wiresPanelComp, false);
            return result;
        }
    }
}
