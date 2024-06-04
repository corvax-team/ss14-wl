using Content.Shared.Inventory;

namespace Content.Shared._WL.Economics.Components
{
    [RegisterComponent]
    public sealed partial class BankAccountHolderComponent : Component, IClothingSlots
    {
        [ViewVariables(VVAccess.ReadOnly)]
        public BankAccount? Account = null;

        public Entity<EconomicsUserComponent>? CurrentUser = null;

        [ViewVariables]
        [DataField]
        public float StartBalance = 2000;

        [ViewVariables(VVAccess.ReadOnly)]
        [DataField]
        public BankAccountStatus StartStatus = BankAccountStatus.Active;

        public SlotFlags Slots => SlotFlags.All;
    }
}
