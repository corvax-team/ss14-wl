using Content.Shared._WL.Economics;

namespace Content.Server._WL.Economics.Components
{
    [RegisterComponent]
    public sealed partial class BankAccountHolderComponent : Component
    {
        [ViewVariables(VVAccess.ReadOnly)]
        public BankAccount Account;

        public EntityUid? CurrentUser = null;

        [ViewVariables]
        [DataField]
        public float StartBalance = 2000;

        [DataField]
        public BankAccountStatus StartStatus = BankAccountStatus.Active;
    }
}
