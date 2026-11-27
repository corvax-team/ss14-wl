using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ship14.CorticalBorer.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CorticalBorerVictimComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Host;

    [DataField, AutoNetworkedField]
    public EntityUid Borer;

    [DataField]
    public EntProtoId ResistAction = "ActionCorticalBorerResist";

    [DataField, AutoNetworkedField]
    public EntityUid? ResistActionEntity;
}
