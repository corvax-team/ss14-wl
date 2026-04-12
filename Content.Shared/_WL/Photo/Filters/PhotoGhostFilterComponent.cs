using Robust.Shared.GameStates;
using Robust.Shared.Utility;
using System.Numerics;

namespace Content.Shared._WL.Photo.Filters;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PhotoGhostFilterComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<Vector2> ViewedGhosts = new();

    [DataField, AutoNetworkedField]
    public SpriteSpecifier Visual = new SpriteSpecifier.Texture(new("/Textures/_WL/Mobs/Ghost/black.png"));

    [DataField]
    public float ViewRange = 20f;

    [DataField]
    public TimeSpan UpdateTime = TimeSpan.FromSeconds(0.1f);
    public TimeSpan NextUpdateTime;
}

