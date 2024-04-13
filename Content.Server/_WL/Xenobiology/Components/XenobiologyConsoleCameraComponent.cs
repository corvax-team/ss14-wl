namespace Content.Server._WL.Xenobiology;

[AutoGenerateComponentPause]
[RegisterComponent]
public sealed partial class XenobiologyConsoleCameraComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid User;

    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> Buffer = new();

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan UiUpdateInterval = TimeSpan.FromSeconds(1);

    [AutoPausedField]
    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ScannedEntity = null;
}
