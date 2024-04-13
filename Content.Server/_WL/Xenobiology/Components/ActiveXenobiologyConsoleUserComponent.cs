namespace Content.Server._WL.Xenobiology;

[RegisterComponent]
public sealed partial class ActiveXenobiologyConsoleUserComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid Console;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Camera = null;
}
