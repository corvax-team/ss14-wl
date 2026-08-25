using Content.Shared.Humanoid.Markings;
using Robust.Shared.GameStates;

namespace Content.Shared._WL.Ghost;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class GhostMarkingsComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<Marking> Markings = new();

    [DataField, AutoNetworkedField]
    public float Alpha = 0.85f;
}
