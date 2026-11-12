using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class DAnalyzerComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string SlotId = "danalyzer_slot";

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    [AutoPausedField]
    public TimeSpan EndTime;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan Duration = TimeSpan.FromSeconds(3);

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public DAnalyzeType CurrentType = DAnalyzeType.None;
}

[Serializable, NetSerializable]
public enum DAnalyzerVisuals : byte
{
    Packing
}

[Serializable, NetSerializable]
public enum DAnalyzerUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class DAnalyzerBoundUserInterfaceState : BoundUserInterfaceState
{
    public EntProtoId? ObjectId;

    public DAnalyzeState ObjectState;

    public ResearchPointsSpecifier PointsData;

    public DAnalyzerBoundUserInterfaceState(
            EntProtoId? objectId = null,
            DAnalyzeState objectState = DAnalyzeState.Invalid,
            ResearchPointsSpecifier? pointsData = null)
    {
        ObjectId = objectId;
        ObjectState = objectState;
        PointsData = pointsData ?? new ResearchPointsSpecifier();
    }
}

[Serializable, NetSerializable]
public sealed class DAnalyzeMessage : BoundUserInterfaceMessage
{
    public readonly DAnalyzeType Type;

    public DAnalyzeMessage(DAnalyzeType type)
    {
        Type = type;
    }
}
