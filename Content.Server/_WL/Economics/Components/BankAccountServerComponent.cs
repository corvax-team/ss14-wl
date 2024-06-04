using Content.Shared._WL.Economics;
using Content.Shared.Roles;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._WL.Economics.Components
{
    [RegisterComponent]
    public sealed partial class BankAccountServerComponent : Component
    {
        [ViewVariables(VVAccess.ReadOnly)]
        public readonly List<BankAccount> Accounts = new();

        [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<DepartmentPrototype>))]
        public string? Department = null;
    }
}
