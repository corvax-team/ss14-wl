using Robust.Shared.Serialization;

namespace Content.Shared._WL.Economics
{
    public sealed class BankAccountServerConstants
    {
        public const string BankAccountWrapper = "wrapper";
    }

    [Serializable, NetSerializable]
    public enum BankAccountServerQueryType : byte
    {
        ADD,
        SET,
        REMOVE
    }

    [Serializable, NetSerializable]
    public readonly record struct BankAccountWrapper(uint AccountID, string AccountName, float Balance, BankAccountStatus Status, BankAccountHolder Holder);
}
