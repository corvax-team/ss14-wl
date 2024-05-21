using JetBrains.Annotations;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Economics
{
    [UsedImplicitly]
    [Serializable, NetSerializable]
    public sealed partial class BankAccount : IEquatable<BankAccount>
    {
        private readonly SharedEconomicSystem _ecoSystem;

        [ViewVariables] public readonly uint ID;
        [ViewVariables] public string AccountName { get; private set; }
        [ViewVariables] public float Balance { get; private set; }
        [ViewVariables] public BankAccountStatus Status { get; private set; }
        [ViewVariables] public IReadOnlyList<string> History { get; private set; }

        public BankAccount(
            SharedEconomicSystem economicSys,
            string accountName,
            float startBalance,
            BankAccountStatus status)
        {
            _ecoSystem = economicSys;

            AccountName = accountName;
            Balance = startBalance;
            Status = status;

            ID = economicSys.CreateID();

            History = [];
        }

        #region Operations
        public BankAccount Clone()
        {
            return new BankAccount(
                _ecoSystem,
                AccountName,
                Balance,
                Status);
        }

        public void SetAccountName(string newName)
        {
            AccountName = newName;
            _ecoSystem.Synchronize(this);
        }

        public void SetBalance(float newValue)
        {
            Balance = newValue;
            _ecoSystem.Synchronize(this);
        }

        public void AdjustBalance(float value)
        {
            Balance += value;
            _ecoSystem.Synchronize(this);
        }

        public void SetStatus(BankAccountStatus status)
        {
            Status = status;
            _ecoSystem.Synchronize(this);
        }

        public void Log(string operation, string message)
        {
            var historyCopy = new List<string>(History)
            {
                $"{DateTime.Now.AddYears(1000)}: {operation.ToUpper()} \"{message}\""
            };

            History = historyCopy;

            _ecoSystem.Synchronize(this);
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
        #endregion
    }

    public enum BankAccountStatus : byte
    {
        Active,
        Frozen,
        Checking,
        Close,
        Deleted
    }
}
