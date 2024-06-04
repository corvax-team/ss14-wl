using Content.Server._WL.Economics.Components;
using Content.Server.DeviceNetwork;
using Content.Server.DeviceNetwork.Components;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Server.Roles.Jobs;
using Content.Server.Station.Systems;
using Content.Shared._WL.Economics;
using Content.Shared.Database;
using Content.Shared.DeviceNetwork;
using Content.Shared.Mind.Components;
using Robust.Shared.Map;
using System.Linq;

namespace Content.Server._WL.Economics.Systems
{
    public sealed partial class BankAccountServerSystem : EntitySystem
    {
        [Dependency] private readonly DeviceNetworkSystem _device = default!;
        [Dependency] private readonly EconomicSystem _economics = default!;
        [Dependency] private readonly WirelessNetworkSystem _wirelessNetwork = default!;
        [Dependency] private readonly JobSystem _jobs = default!;
        [Dependency] private readonly MindSystem _mind = default!;
        [Dependency] private readonly SingletonDeviceNetServerSystem _singletonNet = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<BankAccountServerComponent, ComponentRemove>(OnRemove);
            SubscribeLocalEvent<BankAccountServerComponent, DeviceNetworkPacketEvent>(OnPacketReceived);
            SubscribeLocalEvent<BankAccountServerComponent, DeviceNetServerDisconnectedEvent>(OnDisconnected);
        }

        private void OnRemove(EntityUid server, BankAccountServerComponent comp, ComponentRemove args)
        {
            comp.Accounts.Clear();
        }

        private void OnPacketReceived(EntityUid server, BankAccountServerComponent comp, DeviceNetworkPacketEvent args)
        {
            var nullableWrapper = PacketToWrapper(args.Data);
            if (nullableWrapper == null)
                return;

            var command = nullableWrapper.Value.Command;
            var wrapper = nullableWrapper.Value.Wrapper;

            var account = comp.Accounts
                .FirstOrDefault(a => wrapper.AccountID == a.ID);

            switch (command)
            {
                case BankAccountServerQueryType.ADD:
                    if (account == null)
                    {
                        account = AddAccount(comp, wrapper);
                        account.Log("created", $"Устройство с адресом {args.SenderAddress} создало аккаунт {account.AccountName}.");
                    }
                    comp.Accounts.Add(account);
                    break;
                case BankAccountServerQueryType.SET:
                    if (account == null)
                    {
                        account = AddAccount(comp, wrapper);
                        account.Log("created", $"Устройство с адресом {args.SenderAddress} создало аккаунт {account.AccountName}.");
                    }
                    else
                    {
                        account.ImportFromWrapper(wrapper);
                        account.Log("changed", $"Устройство с адресом {args.SenderAddress} изменило данные вашего аккаунта!", LogImpact.High);
                    }
                    comp.Accounts.Add(account);
                    break;
                case BankAccountServerQueryType.REMOVE:
                    if (account != null)
                    {
                        comp.Accounts.Remove(account);
                        account.Delete();
                    }
                    break;
            }
        }

        private void OnDisconnected(EntityUid server, BankAccountServerComponent comp, ref DeviceNetServerDisconnectedEvent args)
        {
            comp.Accounts.Clear();
        }

        private BankAccount AddAccount(BankAccountServerComponent comp, BankAccountWrapper wrapper)
        {
            var newAccount = _economics.CreateAccount(out _, wrapper.Holder, wrapper.AccountName, wrapper.Balance, wrapper.Status);
            comp.Accounts.Add(newAccount);
            return newAccount;
        }

        #region Public API

        public static NetworkPayload AccountWrapperToPacket(BankAccountWrapper account, BankAccountServerQueryType queryType)
        {
            var payload = new NetworkPayload()
            {
                [DeviceNetworkConstants.Command] = queryType,
                [BankAccountServerConstants.BankAccountWrapper] = account
            };

            return payload;
        }

        public static NetworkPayload AccountToPacket(BankAccount account, BankAccountServerQueryType queryType)
        {
            var wrapper = new BankAccountWrapper(account.ID, account.AccountName, account.Balance, account.Status, account.Holder);

            var payload = new NetworkPayload()
            {
                [DeviceNetworkConstants.Command] = queryType,
                [BankAccountServerConstants.BankAccountWrapper] = wrapper
            };

            return payload;
        }

        public static (BankAccountWrapper Wrapper, BankAccountServerQueryType Command)? PacketToWrapper(NetworkPayload packet)
        {
            if (!packet.TryGetValue(DeviceNetworkConstants.Command, out BankAccountServerQueryType? command))
                return null;

            if (!packet.TryGetValue(BankAccountServerConstants.BankAccountWrapper, out BankAccountWrapper? wrapper))
                return null;

            return (wrapper.Value, command.Value);
        }
        #endregion
    }
}
