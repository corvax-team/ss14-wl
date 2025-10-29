using Robust.Shared.Serialization;

namespace Content.Shared._WL.Materials.Events;

/// <summary>
/// Raised on item before each of its materials is reclaimed by reclaimer.
/// </summary>
[Serializable, NetSerializable]
public sealed class BeforeItemMaterialReclaimedEvent(
    float efficiency,
    float modifier,
    int amount,
    string? material
    ) : EntityEventArgs
{
    public float Efficiency { get; } = efficiency;
    public float Modifier { get; } = modifier;
    public int Amount { get; set; } = amount;
    public string? Material { get; set; } = material;
}
