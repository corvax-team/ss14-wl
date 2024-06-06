using Content.Shared.Inventory;

namespace Content.Shared._WL.Inventory
{
    [RegisterComponent]
    public sealed partial class ExtraBlockingInventorySlotsComponent : Component
    {
        [DataField]
        public List<SlotFlags> Slots = new();
    }
}
