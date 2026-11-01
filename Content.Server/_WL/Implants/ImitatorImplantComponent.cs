using Content.Shared.DoAfter;

namespace Content.Server._WL.Implants;

[RegisterComponent]
public sealed partial class ImitatorImplantComponent : Component
{
    public DoAfterId? CopyDoAfter;

    [DataField]
    public EntityUid? Image;

    [DataField]
    public EntityUid? Backup;

    [DataField]
    public EntityUid? Wearer;
}
