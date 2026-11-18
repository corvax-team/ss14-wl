using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Implants.Components;

[RegisterComponent]
public sealed partial class RemoteDetonationImplantComponent : Component
{
    [DataField]
    public EntProtoId Action = "ActionDetonateImplantedVictim";

    [DataField]
    public EntityUid? ActionEntity;

    [DataField]
    public EntityUid? Implanter;

    [DataField]
    public EntityUid? Target;
}

public sealed partial class DetonateImplantedVictimActionEvent : InstantActionEvent;
