using Content.Shared.Cloning;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Implants;

[RegisterComponent]
public sealed partial class ImitatorImplantComponent : Component
{
    public DoAfterId? CopyDoAfter;

    [DataField]
    public EntityUid? Image;

    [DataField]
    public EntityUid? Backup;

    [DataField]
    public bool Used = false;

    [DataField]
    public ProtoId<CloningSettingsPrototype> Settings = "ImitatorIdentity";
}
