using Robust.Shared.GameStates;

namespace Content.Shared._WL.Inventory;

/// <summary>
/// Ignores WL inventory slot blocking caused by equipment in the listed slots.
/// Other equip and unequip restrictions still apply.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ClothingSlotBlockingBypassComponent : Component
{
    [DataField]
    public List<string> Blockers = new();
}
