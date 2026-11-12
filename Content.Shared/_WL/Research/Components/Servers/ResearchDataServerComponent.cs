using System.Linq;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent]
public sealed partial class ResearchDataServerComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<ProtoId<ResearchCategoryPrototype>, ResearchField> ResearchedData = new();

    [DataField]
    public HashSet<string> SimpleData = new();
}

[ByRefEvent]
public record struct GetServerResearchEvent
{
    public EntityUid Server;
    public Dictionary<ProtoId<ResearchCategoryPrototype>, ResearchDataFrame> ResearchData;

    public GetServerResearchEvent(EntityUid server)
    {
        Server = server;
        ResearchData = new();
    }
}

public sealed class ResearchDataFrame
{
    public Dictionary<ProtoId<ResearchTypePrototype>, FixedPoint4> Data { get; private set; } = new();

    public HashSet<EntityUid> Detectors { get; private set; } = new();

    private List<ResearchPointsSpecifier> Points = new();

    public ResearchDataFrame() { }

    public bool TryInsertData(Dictionary<ProtoId<ResearchTypePrototype>, FixedPoint4> data, EntityUid detector, ResearchPointsSpecifier points)
    {
        if (Detectors.Contains(detector) || points.Empty)
            return false;

        var dataInserted = false;

        foreach (var (key, value) in data)
        {
            dataInserted |= Data.TryAdd(key, value);
        }

        if (!dataInserted)
            return false;

        Detectors.Add(detector);
        Points.Add(points);

        return true;
    }

    public ResearchPointsSpecifier GetPoints()
    {
        var sum = new ResearchPointsSpecifier();

        if (Points.Count == 0)
            return sum;

        foreach (var points in Points)
        {
            sum += points;
        }

        return sum / Points.Count;
    }
}
