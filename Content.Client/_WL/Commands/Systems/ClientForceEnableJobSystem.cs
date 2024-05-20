using Content.Client.Lobby;
using Content.Shared._WL.Commands.Events;

namespace Content.Client._WL.Commands.Systems
{
    public sealed partial class ClientForceEnableJobSystem : EntitySystem
    {
        [Dependency] private readonly IClientPreferencesManager _prefMan = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeNetworkEvent<ServerSaveCharacterEvent>(OnSave);
        }

        private void OnSave(ServerSaveCharacterEvent ev)
        {
            _prefMan.UpdateCharacter(ev.Profile, ev.Slot);
        }
    }
}
