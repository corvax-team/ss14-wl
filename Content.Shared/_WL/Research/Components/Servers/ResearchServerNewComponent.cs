using Content.Shared._WL.Research.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ResearchServerNewComponent : Component
{
    [AutoNetworkedField]
    [DataField("serverName"), ViewVariables(VVAccess.ReadWrite)]
    public string ServerName = "RDMAINSERVER";

    [DataField("points"), AutoNetworkedField]
    public Dictionary<ProtoId<ResearchPointsTypePrototype>, (FixedPoint2, FixedPoint2)> PointsStatistic { get; set; } = new(); // max, per second

    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly)]
    public int Id;

    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> Clients = new();

    [DataField("nextUpdateTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdateTime = TimeSpan.Zero;

    [DataField("researchConsoleUpdateTime"), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan ResearchConsoleUpdateTime = TimeSpan.FromSeconds(1);
}

[ByRefEvent]
public readonly record struct GetResearchSpeedEvent(int Speed);

[ByRefEvent]
public readonly record struct ResearchServerNewUpdatedEvent();
