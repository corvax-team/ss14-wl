namespace Content.Shared._WL.Clothing.Components;

/// <summary>
/// Adds worn layers for active ItemMapper keys. Each key maps inventory slots to their overlays.
/// </summary>
[RegisterComponent]
public sealed partial class ClothingItemMapperComponent : Component
{
    [DataField(required: true)]
    public Dictionary<string, Dictionary<string, List<PrototypeLayerData>>> MapLayers = new();
}
