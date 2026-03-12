namespace Content.Shared._WL.Tray;

[RegisterComponent]
public sealed partial class OnTrayComponent : Component
{
    [ViewVariables]
    public EntityUid? TrayEntity;

    [ViewVariables]
    public bool Storaged = false;
}
