using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Photo.Filters;

[RegisterComponent]
public sealed partial class PhotoCameraFilterComponent : Component
{
    [DataField()]
    public ComponentRegistry FilterComponents = new();
}
