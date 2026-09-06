using Content.Shared._WL.Research.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent]
public sealed partial class ResearchMainConsoleComponent : Component
{ }

[Serializable, NetSerializable]
public sealed class ResearchMainConsoleBoundInterfaceState : BoundUserInterfaceState
{
    public Dictionary<ProtoId<ResearchPointsTypePrototype>, (FixedPoint2, FixedPoint2, FixedPoint2)> PointsData;

    public Dictionary<ProtoId<ResearchPrototype>, ResearchState> ResearchesData;

    public ResearchMainConsoleBoundInterfaceState(
            Dictionary<ProtoId<ResearchPointsTypePrototype>, (FixedPoint2, FixedPoint2, FixedPoint2)> pointsData,
            Dictionary<ProtoId<ResearchPrototype>, ResearchState> researchesData)
    {
        PointsData = pointsData;
        ResearchesData = researchesData;
    }
}

[Serializable, NetSerializable]
public sealed class TerminalStartResearchMessage : BoundUserInterfaceMessage
{
    public string Id;

    public TerminalStartResearchMessage(string id)
    {
        Id = id;
    }
}


[Serializable, NetSerializable]
public enum ResearchMainConsoleUiKey : byte
{
    Key
}
