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
}

[ByRefEvent]
public record struct GetServerResearchEvent
{
    public EntityUid Server;
    public Dictionary<ProtoId<ResearchCategoryPrototype>, (Dictionary<ProtoId<ResearchTypePrototype>, FixedPoint2>, ResearchPointsSpecifier)> ResearchData;

    public GetServerResearchEvent(EntityUid server)
    {
        Server = server;
        ResearchData = new();
    }
}
