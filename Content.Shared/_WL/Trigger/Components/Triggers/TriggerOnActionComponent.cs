using Content.Shared.Trigger.Components.Triggers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Trigger.Components.Triggers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TriggerOnActionComponent : BaseTriggerOnXComponent
{
    [DataField]
    public EntProtoId Action = "BaseTriggerAction";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;
}
