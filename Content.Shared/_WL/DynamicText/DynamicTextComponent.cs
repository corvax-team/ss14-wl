using Robust.Shared.GameStates;

namespace Content.Shared._WL.DynamicText;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DynamicTextComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Text = string.Empty;
}
