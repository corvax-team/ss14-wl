using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Ship14.CorticalBorer.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class CorticalBorerComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Host;

    [DataField, AutoNetworkedField]
    public float Chemicals;

    [DataField]
    public float MaxChemicals = 250f;

    [DataField]
    public float ChemicalRegeneration = 1f;

    [DataField]
    public float HostedHealing = 1f;

    [DataField]
    public ProtoId<AlertPrototype> ChemicalsAlert = "CorticalBorerChemicals";

    [ViewVariables]
    public short LastChemicalSeverity = -1;

    [ViewVariables]
    public short LastChemicalAmount = -1;

    [DataField, AutoNetworkedField]
    public bool ParalyzingBiteArmed;

    [DataField, AutoNetworkedField]
    public bool SugarSuppressed;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan SugarSleepUntil;

    [DataField, AutoNetworkedField]
    public bool ControllingHost;

    [DataField, AutoNetworkedField]
    public CorticalBorerMenuMode MenuMode;

    [DataField]
    public EntProtoId ParalyzeAction = "ActionCorticalBorerParalyze";

    [DataField]
    public EntProtoId InfestAction = "ActionCorticalBorerInfest";

    [DataField]
    public EntProtoId ConverseAction = "ActionCorticalBorerConverse";

    [DataField]
    public EntProtoId PunishAction = "ActionCorticalBorerPunish";

    [DataField]
    public EntProtoId InjectAction = "ActionCorticalBorerInject";

    [DataField]
    public EntProtoId ScreechAction = "ActionCorticalBorerScreech";

    [DataField]
    public EntProtoId ReproduceAction = "ActionCorticalBorerReproduce";

    [DataField]
    public EntProtoId ControlAction = "ActionCorticalBorerControl";

    [DataField]
    public EntProtoId LeaveAction = "ActionCorticalBorerLeave";

    [DataField]
    public EntProtoId ReleaseAction = "ActionCorticalBorerRelease";

    [DataField, AutoNetworkedField]
    public EntityUid? ParalyzeActionEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? InfestActionEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? ConverseActionEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? PunishActionEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? InjectActionEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? ScreechActionEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? ReproduceActionEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? ControlActionEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? LeaveActionEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? ReleaseActionEntity;
}
