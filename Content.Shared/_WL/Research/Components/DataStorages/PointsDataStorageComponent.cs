using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent]
public sealed partial class PointsDataStorageComponent : BaseDataStorageComponent
{
    [ViewVariables(VVAccess.ReadOnly)]
    public ResearchPointsSpecifier Points { get; set; } = new();

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public List<ProtoId<ResearchPointsTypePrototype>> AllowedPointsTypes = new();
}

[Serializable, NetSerializable]
public sealed class PointsTransferMessage : BoundUserInterfaceMessage
{
    public readonly ResearchPointsSpecifier Points;
    public readonly bool Direction;

    public PointsTransferMessage(ResearchPointsSpecifier points, bool direction)
    {
        Points = points;
        Direction = direction;
    }
}

[Serializable, NetSerializable]
public enum PointsDataReaderUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class PointsDataReadBoundUserInterfaceState : BoundUserInterfaceState
{
    public PortState PortState;

    public ResearchPointsSpecifier StoragePointsData;

    public ResearchPointsSpecifier DiskPointsData;

    public PointsDataReadBoundUserInterfaceState(
            PortState portState,
            ResearchPointsSpecifier storagePointsData,
            ResearchPointsSpecifier? diskPointsData = null)
    {
        PortState = portState;
        StoragePointsData = storagePointsData;
        DiskPointsData = diskPointsData ?? new ResearchPointsSpecifier();
    }
}
