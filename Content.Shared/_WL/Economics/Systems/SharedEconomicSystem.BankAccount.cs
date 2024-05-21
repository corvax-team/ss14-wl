using Content.Shared._WL.Economics.Events;
using Content.Shared.GameTicking;
using System.Diagnostics.CodeAnalysis;

namespace Content.Shared._WL.Economics
{
    public abstract partial class SharedEconomicSystem : EntitySystem
    {
        private readonly Dictionary<uint, BankAccount> _accounts = new();
        private uint _indexer = 0;

        private void InitializeBankAccounts()
        {
            SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRestart);
        }

        private void OnRestart(RoundRestartCleanupEvent ev)
        {
            _accounts.Clear();
            _indexer = 0;
        }

        public uint CreateAccount(BankAccount account)
        {
            _accounts.Add(account.ID, account);

            return account.ID;
        }

        public uint CreateAccount(
            string accountName = "User",
            float startingBalance = 0,
            BankAccountStatus status = BankAccountStatus.Active)
        {
            var account = new BankAccount(this, accountName, startingBalance, status);
            return CreateAccount(account);
        }

        public BankAccount GetAccount(uint index)
        {
            return _accounts[index];
        }

        public bool TryGetAccount(uint index, [NotNullWhen(true)] out BankAccount? account)
        {
            _accounts.TryGetValue(index, out var thisAccount);

            account = thisAccount;

            return thisAccount != null;
        }

        public uint CreateID()
        {
            return _indexer += 1;
        }

        public void Synchronize(BankAccount account)
        {
            var ev = new BankAccountUpdatedEvent(account);

            RaiseNetworkEvent(ev);
        }
    }
}
