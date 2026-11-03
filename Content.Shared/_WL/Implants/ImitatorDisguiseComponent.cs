using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
using Robust.Shared.GameStates;
using Content.Shared.DisplacementMap;
using System.Numerics;

namespace Content.Shared._WL.Implants;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ImitatorDisguiseComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Instability;
}

public sealed partial class ImitatorCopyActionEvent : EntityTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class ImitatorCopyDoAfterEvent : SimpleDoAfterEvent;
