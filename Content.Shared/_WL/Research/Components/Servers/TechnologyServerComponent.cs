using Content.Shared.Lathe;
using Content.Shared.Research.Prototypes;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared._WL.Research.Systems;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TechnologyServerComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public ProtoId<ResearchPrototype> RootResearch;

    [DataField, AutoNetworkedField]
    public int BaseResearchSpeed = 1;

    [AutoNetworkedField]
    public Dictionary<ProtoId<ResearchPrototype>, ResearchState> Researches = new();

    [AutoNetworkedField]
    public List<ProtoId<ResearchPrototype>> ResearchQueue = new();
}

[ByRefEvent]
public readonly record struct ResearchDatabaseModifiedEvent(List<string>? NewlyUnlockedRecipes);

[Serializable, NetSerializable]
public struct ResearchState
{
    public ResearchStatus Status { get; set; }

    public ResearchDepsStatus DepsState { get; set; }

    public FixedPoint2 ResearchedPackages = 0;

    public FixedPoint2 PackagesCostModed = 0;

    public ProtoId<ResearchModePrototype> ModeId = "Default";

    public ProtoId<ResearchPrototype>? Parent = null;

    public ResearchState()
    {
        new ResearchState(null);
    }

    public ResearchState(ProtoId<ResearchPrototype>? parent)
    {
        Status = ResearchStatus.NotResearched;
        DepsState = ResearchDepsStatus.Allowed;
        Parent = parent;
    }
}

[Serializable, NetSerializable]
public enum ResearchStatus: byte
{
    NotResearched = 0,
    Researching = 1,
    InQueue = 2,
    Researched = 3,
    Invalid = 4
}

[Serializable, NetSerializable]
public enum ResearchDepsStatus: byte
{
    Allowed = 0,
    SpecialReq = 1,
    ParentsReq = 2,
    PointsReq = 3,
    StorageReq = 4,
    OnResearch = 5,
    Invalid = 6
}
