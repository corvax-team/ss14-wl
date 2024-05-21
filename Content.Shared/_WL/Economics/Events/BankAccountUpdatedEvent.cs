using Robust.Shared.Serialization;

namespace Content.Shared._WL.Economics.Events
{
    [Serializable, NetSerializable]
    public sealed partial class BankAccountUpdatedEvent : EntityEventArgs
    {
        public readonly BankAccount Account;

        public BankAccountUpdatedEvent(BankAccount account)
        {
            Account = account;
        }
    }
}
