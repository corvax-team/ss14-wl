using Robust.Shared.Serialization;

namespace Content.Shared._WL.Materials.Events;

[Serializable, NetSerializable]
public sealed class BeforeItemReclaimedEvent(
    float efficiency,
    float modifier,
    int amount,
    string? material
    ) : EntityEventArgs
{
    public float Efficiency { get; set; } = efficiency;
    public float Modifier { get; set; } = modifier;
    public int Amount { get; set; } = amount;
    public string? Material { get; set; } = material;
}
