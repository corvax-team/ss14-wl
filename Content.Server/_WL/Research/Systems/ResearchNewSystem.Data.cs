using System.Numerics;
using System.Linq;
using Content.Server.Power.EntitySystems;
using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Methods;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using Content.Shared.Research.Prototypes;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Containers;

namespace Content.Server._WL.Research.Systems;

public sealed partial class ResearchSystemNew
{
    [Dependency] private ItemSlotsSystem _itemSlots = default!;

    public readonly ComponentRegistry StorageFormatComponents = new ComponentRegistry(new Dictionary<string, EntityPrototype.ComponentRegistryEntry>()
        {
             {"PointsDataStorage", new EntityPrototype.ComponentRegistryEntry(new PointsDataStorageComponent()) },
             {"RecipesStorageComponent", new EntityPrototype.ComponentRegistryEntry(new RecipesStorageComponent()) }
        }
    );

    private void InitializeData()
    {
        Subs.BuiEvents<PointsDataStorageComponent>(PointsDataReaderUiKey.Key, subs =>
        {
            subs.Event<PointsTransferMessage>(OnPointsTransferMessage);
        });
    }

    [SubscribeLocalEvent]
    private void OnDataStorageInit(Entity<DataStorageComponent> ent, ref MapInitEvent args)
    {
        TryFormatStorage(ent, true);
    }

    private bool TryFormatStorage(Entity<DataStorageComponent> ent, bool initFormat = false)
    {
        if (!ent.Comp.CanBeFormatted && !initFormat)
            return false;

        EntityManager.RemoveComponents(ent, StorageFormatComponents);

        var formatProto = ProtoMan.Index(ent.Comp.CurrentStorageFormat);

        ent.Comp.LocalSize = ent.Comp.Size / formatProto.StoragesList.Values.Count;

        EntityManager.AddComponents(ent, formatProto.StoragesList);

        return true;
    }

    [SubscribeLocalEvent]
    private void OnPointsStorageStartup(Entity<PointsDataStorageComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<DataStorageComponent>(ent, out var dataStorage))
            return;

        ent.Comp.LocalSize = dataStorage.LocalSize;

        ent.Comp.Points = new ResearchPointsSpecifier(ent.Comp.AllowedPointsTypes, 0);
    }

    [SubscribeLocalEvent]
    private void OnDiskInserted(Entity<DataReaderComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        UpdateReaderInterface(ent.Owner, ent.Comp);
    }

    [SubscribeLocalEvent]
    private void OnDataUpdated(Entity<DataReaderComponent> ent, ref DataUpdatedEvent args)
    {
        UpdateReaderInterface(ent.Owner, ent.Comp);
    }

    [SubscribeLocalEvent]
    private void OnDataReaderStartup(Entity<DataReaderComponent> ent, ref ComponentStartup args)
    {
        UpdateReaderInterface(ent.Owner, ent.Comp);
    }

    [SubscribeLocalEvent]
    private void OnPointsSizeCheck(Entity<PointsDataStorageComponent> ent, ref RecalcExpiredSizeEvent args)
    {
        args.ExpiredSize += ent.Comp.ExpiredLocalSize;
    }

    [SubscribeLocalEvent]
    private void OnRecipeSizeCheck(Entity<RecipesStorageComponent> ent, ref RecalcExpiredSizeEvent args)
    {
        args.ExpiredSize += ent.Comp.ExpiredLocalSize;
    }

    private void OnPointsTransferMessage(Entity<PointsDataStorageComponent> ent, ref PointsTransferMessage args)
    {
        if (!TryComp<DataReaderComponent>(ent, out var reader) ||
                !_itemSlots.TryGetSlot(ent, reader.SlotId, out var itemSlot) ||
                itemSlot.Item is not { } disk)
            return;

        if (args.Direction)
            TryTransferPoints(ent, disk, args.Points, out _, ent.Comp);
        else
            TryTransferPoints(disk, ent, args.Points, out _, null, ent.Comp);

        UpdatePointsReaderInterface(ent, null, ent.Comp);
    }

    public bool TryDeletePoints(
            EntityUid uid,
            ref ResearchPointsSpecifier deletePoints,
            out ResearchPointsSpecifier deletedPoints,
            PointsDataStorageComponent? storage = null)
    {
        deletedPoints = new();

        if (!Resolve(uid, ref storage))
            return false;

        if (!deletePoints.AnyPositive())
            return false;

        storage.Points -= deletePoints;

        var negativePoints = ResearchPointsSpecifier.GetNegative(storage.Points);
        storage.Points -= negativePoints;
        deletedPoints = deletePoints + negativePoints;

        deletePoints -= deletedPoints;

        RecalcPointsSize(uid, storage);

        return true;
    }

    public bool TryWritePoints(
            EntityUid uid,
            ref ResearchPointsSpecifier writePoints,
            out ResearchPointsSpecifier writedPoints,
            PointsDataStorageComponent? storage = null)
    {
        writedPoints = new();

        if (!Resolve(uid, ref storage))
            return false;

        if (!writePoints.AnyPositive())
            return false;

        writedPoints = storage.Points.SizedAdd(ProtoMan, writePoints, storage.LocalSize - storage.ExpiredLocalSize);
        writePoints -= writedPoints;

        RecalcPointsSize(uid, storage);

        return true;
    }

    public bool TryTransferPoints(
            EntityUid sourceUid,
            EntityUid recipientUid,
            ResearchPointsSpecifier transferPoints,
            out ResearchPointsSpecifier tranferedPoints,
            PointsDataStorageComponent? sourceStorage = null,
            PointsDataStorageComponent? recipientStorage = null)
    {
        tranferedPoints = new();

        if (!Resolve(sourceUid, ref sourceStorage) || !Resolve(recipientUid, ref recipientStorage))
            return false;

        if (!transferPoints.AnyPositive())
            return false;

        if (!TryDeletePoints(sourceUid, ref transferPoints, out var deletedPoints, sourceStorage))
            return false;

        if (!TryWritePoints(recipientUid, ref deletedPoints, out var writedPoints, recipientStorage))
            return false;

        if (!TryWritePoints(sourceUid, ref deletedPoints, out var returnedPoints, sourceStorage))
            return false;

        transferPoints += returnedPoints;

        return true;
    }

    public bool TryWriteRecipes(
            EntityUid uid,
            ref List<ProtoId<LatheRecipePrototype>> writeRecipes,
            out List<ProtoId<LatheRecipePrototype>> writedRecipes,
            RecipesStorageComponent? storage = null)
    {
        writedRecipes = new List<ProtoId<LatheRecipePrototype>>();

        if (!Resolve(uid, ref storage))
            return false;

        if (writeRecipes.Count == 0)
            return true;

        var allowedRecipesCount = storage.SizePerTech > 0
            ? ((storage.LocalSize - storage.ExpiredLocalSize) / storage.SizePerTech).Int()
            : writeRecipes.Count;

        writedRecipes = writeRecipes.GetRange(0, allowedRecipesCount);

        storage.Recipes.AddRange(writedRecipes);

        RecalcRecipesSize(uid, storage);

        return true;
    }

    public bool TryDeleteRecipes(
            EntityUid uid,
            ref List<ProtoId<LatheRecipePrototype>> deleteRecipes,
            out List<ProtoId<LatheRecipePrototype>> deletedRecipes,
            RecipesStorageComponent? storage = null)
    {
        deletedRecipes = new List<ProtoId<LatheRecipePrototype>>();

        if (!Resolve(uid, ref storage))
            return false;

        if (deleteRecipes.Count == 0)
            return true;

        foreach (var recipe in deleteRecipes)
        {
            if (storage.Recipes.Remove(recipe))
            {
                deleteRecipes.Remove(recipe);
                deletedRecipes.Add(recipe);
            }
        }

        RecalcRecipesSize(uid, storage);

        return true;
    }

    public bool TryTransferRecipes(
            EntityUid sourceUid,
            EntityUid recipientUid,
            ref List<ProtoId<LatheRecipePrototype>> transferRecipes,
            out List<ProtoId<LatheRecipePrototype>> tranferedRecipes,
            RecipesStorageComponent? sourceStorage = null,
            RecipesStorageComponent? recipientStorage = null)
    {
        tranferedRecipes = new List<ProtoId<LatheRecipePrototype>>();

        if (!Resolve(sourceUid, ref sourceStorage) || !Resolve(recipientUid, ref recipientStorage))
            return false;

        if (transferRecipes.Count == 0)
            return true;

        if (!TryDeleteRecipes(sourceUid, ref transferRecipes, out var deletedRecipes, sourceStorage))
            return false;

        if (!TryWriteRecipes(recipientUid, ref deletedRecipes, out var writedRecipes, recipientStorage))
            return false;

        if (!TryWriteRecipes(sourceUid, ref deletedRecipes, out var returnedRecipes, sourceStorage))
            return false;

        transferRecipes.AddRange(returnedRecipes);

        return true;
    }

    private void RecalcStorageSize(EntityUid uid, DataStorageComponent? storage = null)
    {
        if (!Resolve(uid, ref storage))
            return;

        var ev = new RecalcExpiredSizeEvent(FixedPoint2.Zero);
        RaiseLocalEvent(uid, ref ev);

        storage.ExpiredSize = ev.ExpiredSize;

        var ev2 = new DataUpdatedEvent();
        RaiseLocalEvent(uid, ref ev2);
    }

    private void RecalcPointsSize(EntityUid uid, PointsDataStorageComponent? storage = null)
    {
        if (!Resolve(uid, ref storage))
            return;

        storage.ExpiredLocalSize = storage.Points.GetSize(ProtoMan);
        RecalcStorageSize(uid);
    }

    private void RecalcRecipesSize(EntityUid uid, RecipesStorageComponent? storage = null)
    {
        if (!Resolve(uid, ref storage))
            return;

        storage.ExpiredLocalSize = storage.SizePerTech > 0
            ? storage.Recipes.Count * storage.SizePerTech
            : 0;

        RecalcStorageSize(uid);
    }

    private void UpdateReaderInterface(EntityUid uid, DataReaderComponent? reader = null)
    {
        if (!Resolve(uid, ref reader))
            return;

        if (TryComp<PointsDataStorageComponent>(uid, out var points))
            UpdatePointsReaderInterface(uid, reader, points);

        if (TryComp<RecipesStorageComponent>(uid, out var recipes))
            UpdateRecipesReaderInterface(uid, reader, recipes);
    }

    private void UpdatePointsReaderInterface(EntityUid uid, DataReaderComponent? reader = null, PointsDataStorageComponent? storage = null)
    {
        if (!Resolve(uid, ref reader, ref storage, false))
            return;

        if (!_itemSlots.TryGetSlot(uid, reader.SlotId, out var itemSlot))
            return;

        var portState = PortState.Empty;
        var diskPoints = new ResearchPointsSpecifier();

        if (itemSlot.Item is { } disk)
        {
            portState = PortState.WrongFormat;
            if (TryComp<PointsDataStorageComponent>(disk, out var diskStorage))
            {
                diskPoints = diskStorage.Points;

                if (TryComp<DataStorageComponent>(disk, out var dataStorage))
                    portState = dataStorage.WriteAllowed ? PortState.ReadWrite : PortState.ReadOnly;
            }
        }

        var state = new PointsDataReadBoundUserInterfaceState(portState, storage.Points, diskPoints);

        _uiSystem.SetUiState(uid, PointsDataReaderUiKey.Key, state);
    }

    private void UpdateRecipesReaderInterface(EntityUid uid, DataReaderComponent? reader = null, RecipesStorageComponent? storage = null)
    {
        if (!Resolve(uid, ref reader, ref storage, false))
            return;

        if (!_itemSlots.TryGetSlot(uid, reader.SlotId, out var itemSlot))
            return;

        var portState = PortState.Empty;
        var diskRecipes = new List<ProtoId<LatheRecipePrototype>>();

        if (itemSlot.Item is { } disk)
        {
            portState = PortState.WrongFormat;
            if (TryComp<RecipesStorageComponent>(disk, out var diskStorage))
            {
                diskRecipes = diskStorage.Recipes;

                if (TryComp<DataStorageComponent>(disk, out var dataStorage))
                    portState = dataStorage.WriteAllowed ? PortState.ReadWrite : PortState.ReadOnly;
            }
        }

        var state = new RecipesReaderBoundUserInterfaceState(portState, storage.Recipes, diskRecipes);

        _uiSystem.SetUiState(uid, RecipesReaderUiKey.Key, state);
    }
}
