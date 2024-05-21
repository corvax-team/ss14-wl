using Content.Server._WL.Economics.Components;
using Content.Server._WL.Economics.Prototypes;
using Content.Server.Medical.SuitSensors;
using Content.Server.Station.Systems;
using Content.Shared._WL.Economics;
using Content.Shared._WL.Economics.Components;
using Content.Shared.IdentityManagement;
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

        [ViewVariables] public SalaryGettingConfigurationPrototype SalaryConfiguration { get; private set; } = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<BankAccountHolderComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<BankAccountHolderComponent, ComponentShutdown>(OnShutdown);

            SubscribeLocalEvent<BankAccountHolderComponent, StationRenamedEvent>(OnStationRenamed);

            SalaryConfiguration = _protoMan.EnumeratePrototypes<SalaryGettingConfigurationPrototype>()
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

            var bankAccountHolders = EntityQueryEnumerator<BankAccountHolderComponent, GettingSalaryComponent>();
            while (bankAccountHolders.MoveNext(out _, out var comp, out var gettingSalaryComp))
            {
                if (_gameTime.CurTime < gettingSalaryComp.NextGetTime)
                    continue;
                else gettingSalaryComp.NextGetTime += TimeSpan.FromSeconds(gettingSalaryComp.Interval);

                // Зачем отсылать зп незарегистрированным пользователям?..
                if (comp.Account.Status != BankAccountStatus.Active)
                    continue;

                if (comp.CurrentUser == null)
                    continue;

                var suitStatus = _suitSensor.GetSensorState(comp.CurrentUser.Value);
                if (suitStatus == null)
                    continue;

                var sum = gettingSalaryComp.Salary;
                if (gettingSalaryComp.UseCoefficients)
                {
                    foreach (var coef in SalaryConfiguration.Coefficients!)
                    {
                        sum *= coef.Calculate(EntityManager, comp.CurrentUser.Value, suitStatus);
                    }

                    foreach (var penalty in gettingSalaryComp.Penalties)
                    {
                        if (_gameTime.CurTime >= penalty.RemoveTime)
                        {
                            gettingSalaryComp.Penalties.Remove(penalty);
                            continue;
                        }

                        sum *= penalty.Coefficient;
                    }
                }

                comp.Account.AdjustBalance(sum);
            }
        }

        private void OnInit(EntityUid holder, BankAccountHolderComponent comp, ComponentInit _)
        {
            var player = GetPlayer(holder);
            if (player == null)
                return;

            player.Value.EconomicUserComponent.BankCard = holder;

            var name = Identity.Name(player.Value.User, EntityManager);

            comp.Account = GetAccount(CreateAccount(name, comp.StartBalance, comp.StartStatus));
            comp.Account.Log("CREATED", $"открыт счёт с начальным балансом в {comp.StartBalance} кредитов");
            comp.CurrentUser = player.Value.User;
        }

        private void OnShutdown(EntityUid holder, BankAccountHolderComponent comp, ComponentShutdown _)
        {
            comp.Account.SetStatus(BankAccountStatus.Deleted);
        }

        private void OnStationRenamed(EntityUid station, BankAccountHolderComponent comp, StationRenamedEvent args)
        {
            comp.Account.SetAccountName(args.NewName);
            comp.Account.Log("RENAMED", $"название счёта было изменено с '{args.OldName}' на '{args.NewName}'");
        }
    }
}
