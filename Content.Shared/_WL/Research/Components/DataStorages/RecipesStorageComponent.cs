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

[Serializable, NetSerializable]
public sealed class RecipesTransferMessage : BoundUserInterfaceMessage
{
    public readonly List<ProtoId<LatheRecipePrototype>> Recipes;
    public readonly bool Direction;
    public readonly bool Copy;

    public RecipesTransferMessage(List<ProtoId<LatheRecipePrototype>> recipes, bool direction, bool copy)
    {
        Recipes = recipes;
        Direction = direction;
        Copy = copy;
    }
}

[ByRefEvent]
public record struct RecipesWritedEvent(List<ProtoId<LatheRecipePrototype>> Recipes);
