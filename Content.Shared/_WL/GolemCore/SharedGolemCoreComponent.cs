using Content.Shared.Actions;

namespace Content.Shared._WL.GolemCore
{
    public abstract class SharedGolemCoreSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<GolemCoreComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<GolemCoreComponent, ComponentShutdown>(OnShutdown);
        }

        private void OnMapInit(EntityUid uid, GolemCoreComponent component, MapInitEvent args)
        {
            _actionsSystem.AddAction(uid, ref component.MidiAction, component.MidiActionId);
        }

        private void OnShutdown(EntityUid uid, GolemCoreComponent component, ComponentShutdown args)
        {
            _actionsSystem.RemoveAction(uid, component.MidiAction);
        }
    }
}

