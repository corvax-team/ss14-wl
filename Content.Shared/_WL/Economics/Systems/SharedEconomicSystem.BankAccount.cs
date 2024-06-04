using Content.Shared._WL.Economics.Events;
using Content.Shared.GameTicking;
using Robust.Shared.Utility;
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

        public static BankAccountWrapper AccountToWrapper(BankAccount account)
        {
            return new BankAccountWrapper(account.ID, account.AccountName, account.Balance, account.Status, account.Holder);
        }

        public BankAccount WrapperToUninitializeAccount(BankAccountWrapper wrapper)
        {
            return new BankAccount(this, wrapper.AccountName, wrapper.Balance, wrapper.Status, wrapper.Holder);
        }

        public uint CreateAccount(BankAccount account)
        {
            _accounts.Add(account.ID, account);

            return account.ID;
        }

        public uint CreateAccount(
            BankAccountHolder holder,
            string accountName = "User",
            float startingBalance = 0,
            BankAccountStatus status = BankAccountStatus.Active)
        {
            var account = new BankAccount(this, accountName, startingBalance, status, holder);
            return CreateAccount(account);
        }

        public BankAccount CreateAccount(out uint index,
            BankAccountHolder holder,
            string accountName = "User",
            float startingBalance = 0,
            BankAccountStatus status = BankAccountStatus.Active)
        {
            var account = new BankAccount(this, accountName, startingBalance, status, holder);
            index = CreateAccount(account);
            return account;
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

        public bool TryGetAccount(string holderName, [NotNullWhen(true)] out BankAccount? account)
        {
            account = _accounts.FirstOrNull(a => a.Value.AccountName.Equals(holderName, StringComparison.CurrentCultureIgnoreCase))?.Value;

            return account != null;
        }

        public bool TryGetAccount(BankAccountWrapper wrapper, [NotNullWhen(true)] out BankAccount? account)
        {
            var result = TryGetAccount(wrapper.AccountID, out var acc);

            account = acc;

            return result;
        }

        public IEnumerable<BankAccount> GetAllAccounts()
        {
            return _accounts.Values;
        }

        public uint CreateID()
        {
            return _indexer += 1;
        }
    }
}
