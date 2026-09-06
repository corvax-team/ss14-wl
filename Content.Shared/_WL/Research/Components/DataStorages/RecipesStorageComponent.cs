using Content.Shared.Research.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Research.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RecipesStorageComponent : BaseDataStorageComponent
{
    [DataField, AutoNetworkedField]
    public List<ProtoId<LatheRecipePrototype>> Recipes = new();

    [DataField]
    public FixedPoint2 SizePerTech = FixedPoint2.New(0.2);
}

[Serializable, NetSerializable]
public enum RecipesReaderUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class RecipesReaderBoundUserInterfaceState : BoundUserInterfaceState
{
    public PortState PortState;

    public List<ProtoId<LatheRecipePrototype>> StorageRecipesData;

    public List<ProtoId<LatheRecipePrototype>> DiskRecipesData;

    public RecipesReaderBoundUserInterfaceState(
            PortState portState,
            List<ProtoId<LatheRecipePrototype>> storageRecipesData,
            List<ProtoId<LatheRecipePrototype>>? diskRecipesData = null)
    {
        PortState = portState;
        StorageRecipesData = storageRecipesData;
        DiskRecipesData = diskRecipesData ?? new List<ProtoId<LatheRecipePrototype>>();
    }
}

/*
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
*/
