using Robust.Shared.GameStates;

namespace Content.Shared._WL.Shuttles;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RadarMarkerComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Visible = true;

    [DataField, AutoNetworkedField]
    public Color Color = Color.Orange;
    [DataField, AutoNetworkedField]
    public float Size = 1f;
}
