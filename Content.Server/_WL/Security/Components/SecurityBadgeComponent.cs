using Content.Shared.Inventory;

namespace Content.Server._WL.Security.Components;

[RegisterComponent]
public sealed partial class SecurityBadgeComponent : Component, IClothingSlots
{
    [DataField(required: true)]
    public LocId RankLoc { get; set; }

    public SlotFlags Slots => SlotFlags.NECK;
}
