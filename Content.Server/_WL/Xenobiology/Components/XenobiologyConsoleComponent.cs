using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Xenobiology;

[RegisterComponent]
public sealed partial class XenobiologyConsoleComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public bool IsAvailable = true;

    [DataField]
    public float CameraMaxDistance = 10f;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? User = null;

    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<EntProtoId, int> RehydratableCubes = new();

    [DataField("audio")]
    public SoundSpecifier InsertSound;
}
