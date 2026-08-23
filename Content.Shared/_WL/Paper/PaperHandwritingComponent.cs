using Robust.Shared.GameStates;

namespace Content.Shared._WL.Paper;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PaperHandwritingComponent : Component
{
    [DataField]
    [AutoNetworkedField]
    public PaperHandwritingStyle Style = PaperHandwritingStyle.Default;
}
