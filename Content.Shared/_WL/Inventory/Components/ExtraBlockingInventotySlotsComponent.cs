using Content.Shared.Inventory;

namespace Content.Shared._WL.Inventory;

/// <summary>
/// При добавлении к одежде, эта одежда будет блокировать указанные слоты.
/// </summary>
[RegisterComponent]
public sealed partial class ExtraBlockingInventorySlotsComponent : Component
{
    [DataField]
    public List<SlotFlags> Slots = new();
}
