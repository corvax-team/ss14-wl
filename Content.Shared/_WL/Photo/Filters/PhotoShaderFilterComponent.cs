using Robust.Shared.GameStates;

namespace Content.Shared._WL.Photo.Filters;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PhotoShaderFilterComponent : Component
{
    [DataField, AutoNetworkedField]
    public string? Shader;
}
