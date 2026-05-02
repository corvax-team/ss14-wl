using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Photo.Filters;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PhotoFaceFilterComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public SpriteSpecifier? Visual;
}
