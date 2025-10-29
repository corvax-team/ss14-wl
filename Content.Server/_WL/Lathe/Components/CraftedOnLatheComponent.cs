namespace Content.Server._WL.Lathe.Components;

[RegisterComponent]
public sealed partial class CraftedOnLatheComponent : Component
{
    [ViewVariables]
    public Dictionary<string, int> ConsumedMaterials { get; set; } = new();
}
