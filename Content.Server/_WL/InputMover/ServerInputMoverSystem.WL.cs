using Content.Shared._WL.InputMover;
using Content.Shared.Movement.Components;
using Robust.Server.Player;
using Robust.Shared.Player;

namespace Content.Server._WL.InputMover
{
    public sealed partial class ServerInputMoverSystem : EntitySystem
    {
        [Dependency] private readonly IPlayerManager _playMan = default!;

        private readonly Dictionary<ICommonSession, bool> _updates = new();

        public override void Initialize()
        {
            base.Initialize();

            SubscribeNetworkEvent<RunningOnShiftNeedsUpdateEvent>(OnRequest);
        }

        private void OnRequest(RunningOnShiftNeedsUpdateEvent args)
        {
            if (!_playMan.TryGetSessionById(args.NetUserId, out var session))
                return;

            _updates[session] = args.Value;
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            foreach (var needUpdate in _updates)
            {
                var session = needUpdate.Key;
                var cvarValue = needUpdate.Value;

                if (session.AttachedEntity == null)
                    continue;

                var entity = session.AttachedEntity.Value;

                if (!TryComp<InputMoverComponent>(entity, out var inputMoverComp))
                    continue;

                if (inputMoverComp.RunningOnShift == cvarValue)
                    continue;

                inputMoverComp.RunningOnShift = cvarValue;

                Dirty(entity, inputMoverComp);
            }
        }
    }
}
