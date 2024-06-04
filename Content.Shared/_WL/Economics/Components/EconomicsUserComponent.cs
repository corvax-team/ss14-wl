namespace Content.Shared._WL.Economics.Components
{
    [RegisterComponent]
    public sealed partial class EconomicsUserComponent : Component
    {
        public Entity<BankAccountHolderComponent>? BankCard;
    }
}
