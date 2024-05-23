using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.VendingMachines
{
    [Serializable, NetSerializable, Prototype("vendingMachineInventory")]
    public sealed partial class VendingMachineInventoryPrototype : IPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField("startingInventory")]
        public List<VendingMachineInventory> StartingInventory { get; private set; } = new();

        [DataField("emaggedInventory")]
        public List<VendingMachineInventory>? EmaggedInventory { get; private set; }

        [DataField("contrabandInventory")]
        public List<VendingMachineInventory>? ContrabandInventory { get; private set; }
    }

    [Serializable, NetSerializable]
    [DataDefinition]
    [UsedImplicitly]
    public sealed partial class VendingMachineInventory
    {
        [DataField("id", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>), required: true)]
        public string Prototype = string.Empty;

        [DataField(required: true)]
        public uint Amount;

        [DataField]
        public float Cost = 0f;

        public void Deconstruct(out string prototype, out uint amount, out float cost)
        {
            prototype = Prototype;
            amount = Amount;
            cost = Cost;
        }
    }
}
