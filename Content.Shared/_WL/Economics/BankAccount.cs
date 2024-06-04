using Content.Shared.Database;
using Content.Shared.Voting;
using JetBrains.Annotations;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Economics
{
    [UsedImplicitly]
    [Serializable, NetSerializable]
    public sealed class BankAccount : IEquatable<BankAccount>
    {
        private readonly SharedEconomicSystem _ecoSystem;

        [ViewVariables] public readonly uint ID;
        [ViewVariables] public string AccountName { get; private set; }
        [ViewVariables] public float Balance { get; private set; }
        [ViewVariables] public BankAccountStatus Status { get; private set; }
        [ViewVariables] public IReadOnlyList<string> History { get; private set; }
        [ViewVariables] public BankAccountHolder Holder { get; private set; }

        public BankAccount(
            SharedEconomicSystem economicSys,
            string accountName,
            float startBalance,
            BankAccountStatus status,
            BankAccountHolder holder)
        {
            _ecoSystem = economicSys;

            AccountName = accountName;
            Balance = startBalance;
            Status = status;
            Holder = holder;

            ID = economicSys.CreateID();

            History = [];
        }

        #region Operations
        public void SetAccountName(string newName)
        {
            AccountName = newName;
        }

        public void SetBalance(float newValue)
        {
            Balance = newValue;
        }

        public void AdjustBalance(float value)
        {
            Balance += value;
        }

        public void SetStatus(BankAccountStatus status)
        {
            Status = status;
        }

        public void NewHolder(BankAccountHolder holder)
        {
            Holder = holder;
        }

        public BankAccount ImportFromWrapper(BankAccountWrapper wrapper)
        {
            Status = wrapper.Status;
            Balance = wrapper.Balance;
            AccountName = wrapper.HolderName;

            return this;
        }

        public BankAccountWrapper ExportToWrapper()
        {
            return new BankAccountWrapper(ID, AccountName, Balance, Status);
        }

        public void Delete()
        {
            Status = BankAccountStatus.Deleted;
            Balance = 0;
            AccountName = "Deleted";
        }

        public void Log(string operation, string message, LogImpact logLevel = LogImpact.Low)
        {
            var historyCopy = new List<string>(History)
            {
                $"{DateTime.Now.AddYears(1000)} {Enum.GetName(logLevel)?.ToLower() ?? "low"}: {operation.ToUpper()} \"{message}\""
            };

            History = historyCopy;
        }
        #endregion

        #region Utility
        public bool Equals(BankAccount? other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (ID != other.ID)
                return false;

            return true;
        }

        public BankAccount Clone()
        {
            return new BankAccount(
                _ecoSystem,
                AccountName,
                Balance,
                Status);
        }
        #endregion
    }

    [Serializable, NetSerializable]
    public readonly record struct BankAccountHolder(string Name, string SuitAddress);

    public enum BankAccountStatus : byte
    {
        Active,
        Frozen,
        Checking,
        Close,
        Deleted
    }
}
