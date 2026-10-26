using Content.Shared.Humanoid;
using Robust.Shared.GameStates;
using Robust.Shared.Maths;

namespace Content.Shared._WL.Body;

/// <summary>
/// Body layers in back-to-front order for each screen direction. Markings follow their body layer.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DirectionalBodyLayersComponent : Component
{
    [DataField]
    public Dictionary<Direction, List<HumanoidVisualLayers>> Directions = new();
}
