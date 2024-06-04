using Content.Server._WL.Economics.Components;
using Content.Server._WL.Economics.Prototypes;
using Content.Server.Cargo.Systems;
using Content.Server.DeviceNetwork;
using Content.Server.DeviceNetwork.Components;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.GameTicking;
using Content.Server.Medical.SuitSensors;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Content.Shared._WL.Economics;
using Content.Shared._WL.Economics.Components;
using Content.Shared.DeviceNetwork;
using Content.Shared.Inventory;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Server._WL.Economics.Systems
{
    public sealed partial class EconomicSystem : SharedEconomicSystem
    {
        [Dependency] private readonly IPrototypeManager _protoMan = default!;
        [Dependency] private readonly IGameTiming _gameTime = default!;
        [Dependency] private readonly SuitSensorSystem _suitSensor = default!;
        [Dependency] private readonly BankAccountServerSystem _bankAccountServer = default!;
        [Dependency] private readonly DeviceNetworkSystem _device = default!;
        [Dependency] private readonly InventorySystem _inventory = default!;
        [Dependency] private readonly ISawmill _sawmill = default!;
        [Dependency] private readonly SingletonDeviceNetServerSystem _singletonNet = default!;
        [Dependency] private readonly StationSystem _station = default!;

        [ViewVariables] public SalaryConfigurationPrototype SalaryConfiguration { get; private set; } = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<BankAccountHolderComponent, StationPostInitEvent>(OnStationInit);
            SubscribeLocalEvent<BankAccountHolderComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<BankAccountHolderComponent, StationRenamedEvent>(OnStationRenamed);
            SubscribeLocalEvent<BankAccountHolderComponent, PriceCalculationEvent>(OnGetPrice);

            SubscribeLocalEvent<EconomicsUserComponent, PlayerSpawnCompleteEvent>(OnPlayerSpawn);

            SubscribeLocalEvent<GettingSalaryComponent, DeviceNetworkPacketEvent>(OnPacketRecieve);

            SalaryConfiguration = _protoMan.EnumeratePrototypes<SalaryConfigurationPrototype>()
                .First();

            var salaryGetters = EntityQuery<GettingSalaryComponent>();
            foreach (var salaryGetter in salaryGetters)
            {
                salaryGetter.NextGetTime = _gameTime.CurTime + TimeSpan.FromSeconds(salaryGetter.Interval);
            }
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var bankAccountHolders = EntityQueryEnumerator<BankAccountServerComponent>();
            while (bankAccountHolders.MoveNext(out var server, out var bankAccountServerComp))
            {
                foreach (var account in bankAccountServerComp.Accounts)
                {
                    if (account.)

                    if (_gameTime.CurTime < gettingSalaryComp.NextGetTime)
                        continue;
                    else gettingSalaryComp.NextGetTime += TimeSpan.FromSeconds(gettingSalaryComp.Interval);

                    if (comp.CurrentUser == null)
                        continue;

                    var suitStatus = _suitSensor.GetSensorState(comp.CurrentUser.Value);
                    if (suitStatus == null)
                        continue;

                    var station = _station.GetOwningStation(holder, transformComp);
                    if (station == null)
                        continue;

                    if (!_singletonNet.TryGetActiveServerAddress<BankAccountServerComponent>(station.Value, out var address))
                        return;

                    var payload = new NetworkPayload()
                    {

                    };

                    _device.QueuePacket(holder, address, payload, deviceNetworkComp.TransmitFrequency, deviceNetworkComp.DeviceNetId, deviceNetworkComp);
                }
            }
        }

        private void OnPacketRecieve(EntityUid salaryGetter, GettingSalaryComponent comp, DeviceNetworkPacketEvent ev)
        {
            if (!ev.Data.TryGetValue(BankAccountServerConstants.BankAccountWrapper, out IEnumerable<BankAccountWrapper>? accounts))
                return;

            foreach (var accountWrapper in accounts)
            {
                if (!TryGetAccount(accountWrapper, out var account))
                    continue;

                var sum = comp.Salary;
                if (comp.UseCoefficients)
                {
                    foreach (var coef in SalaryConfiguration.Coefficients!)
                    {
                        sum *= coef.Calculate(EntityManager, comp.CurrentUser.Value, suitStatus);
                    }

                    foreach (var penalty in comp.Penalties)
                    {
                        if (_gameTime.CurTime >= penalty.RemoveTime)
                        {
                            comp.Penalties.Remove(penalty);
                            continue;
                        }

                        sum *= penalty.Coefficient;
                    }
                }

                account.AdjustBalance(sum);
            }
        }

        private void OnStationInit(EntityUid holder, BankAccountHolderComponent comp, StationPostInitEvent ev)
        {
            var station = ev.Station;

            if (!TryComp<EconomicsUserComponent>(station, out var economicsUser))
            {
                _sawmill.Error($"Станция {Name(station)} имеет только {nameof(BankAccountHolderComponent)}, а ожидалось наличие также и {nameof(EconomicsUserComponent)}");
                throw new AggregateException($"{nameof(EconomicSystem)}: строка 106");
            }

            economicsUser.BankCard = (holder, comp);

            var name = Name(station);
            comp.Account = CreateAccount(out _, new BankAccountHolder(), name, comp.StartBalance, comp.StartStatus);
            comp.CurrentUser = (station, economicsUser);
        }

        private void OnPlayerSpawn(EntityUid user, EconomicsUserComponent comp, PlayerSpawnCompleteEvent ev)
        {
            if (!_inventory.TryGetInventoryEntity<SuitSensorComponent>(user, SlotFlags.SUITSTORAGE, out var suit))
                return;

            var name = Name(suit.Value.Owner);
            var deviceNetworkComp = EnsureComp<DeviceNetworkComponent>(suit.Value.Owner);

            if (!_inventory.TryGetInventoryEntity<BankAccountHolderComponent>(user, SlotFlags.All, out var card))
                return;

            card.Value.Comp.Account = CreateAccount(out _, new BankAccountHolder(name, deviceNetworkComp.Address), name, card.Value.Comp.StartBalance, card.Value.Comp.StartStatus);
            card.Value.Comp.CurrentUser = (user, comp);
            comp.BankCard = card;

            var entity = _bankAccountServer.FindBestServerNearestTo(user);
            if (entity == null)
                return;

            var server = entity.Value.Owner;
            var serverDeviceNetworkComp = entity.Value.Comp2;

            var payload = new NetworkPayload()
            {
                [DeviceNetworkConstants.Command] = BankAccountServerQueryType.SET,
                [BankAccountServerConstants.BankAccountWrapper] = card.Value.Comp.Account.ExportToWrapper()
            };

            _device.QueuePacket(
                server,
                serverDeviceNetworkComp.Address,
                payload,
                serverDeviceNetworkComp.ReceiveFrequency,
                serverDeviceNetworkComp.DeviceNetId,
                serverDeviceNetworkComp);
        }

        private void OnShutdown(EntityUid holder, BankAccountHolderComponent comp, ComponentShutdown _)
        {
            comp.Account?.Delete();
        }

        private void OnStationRenamed(EntityUid station, BankAccountHolderComponent comp, StationRenamedEvent args)
        {
            comp.Account?.SetAccountName(args.NewName);
            comp.Account?.Log("RENAMED", $"название счёта было изменено с '{args.OldName}' на '{args.NewName}'");
        }

        [Obsolete("Возможен абуз с узнаванием баланса карты без банкомата")]
        private void OnGetPrice(EntityUid holder, BankAccountHolderComponent comp, ref PriceCalculationEvent args)
        {
            args.Price += comp.Account?.Balance ?? 0;
        }
    }
}
