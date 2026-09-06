using System.Linq;
using Content.Server.Power.EntitySystems;
using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Research.Systems;

public sealed partial class ResearchSystemNew
{
    private void InitializeServer()
    {
        SubscribeLocalEvent<ResearchServerNewComponent, ComponentStartup>(OnServerStartup);
        SubscribeLocalEvent<ResearchServerNewComponent, ComponentShutdown>(OnServerShutdown);
    }

    private void OnServerStartup(Entity<ResearchServerNewComponent> ent, ref ComponentStartup args)
    {
        if (TryComp<PointsDataStorageComponent>(ent, out var storage))
        {
            foreach (var type in storage.AllowedPointsTypes)
            {
                ent.Comp.PointsStatistic.TryAdd(type, (0, 0));
            }
        }

        var unusedId = EntityQuery<ResearchServerNewComponent>(true)
            .Max(s => s.Id) + 1;
        ent.Comp.Id = unusedId;

        Dirty(ent, ent.Comp);
    }

    private void OnServerShutdown(Entity<ResearchServerNewComponent> ent, ref ComponentShutdown args)
    {
        foreach (var client in new List<EntityUid>(ent.Comp.Clients))
        {
            UnregisterClient(client, ent, dirtyServer: false);
        }
    }

    private bool CanRun(EntityUid uid)
    {
        return this.IsPowered(uid, EntityManager);
    }

    private void UpdateServer(EntityUid uid, int time, ResearchServerNewComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!CanRun(uid))
            return;

        if (TryComp<ResearchDataServerComponent>(uid, out var dataServer))
            MakeResearch(uid, component, dataServer);

        if (TryComp<TechnologyServerComponent>(uid, out var techServer))
            TryUpdateResearches(uid, component, techServer);
    }

    private void TryUpdateResearches(EntityUid uid, ResearchServerNewComponent? server = null, TechnologyServerComponent? techServer = null)
    {
        if (!Resolve(uid, ref server) || !Resolve(uid, ref techServer))
            return;

        UpdateResearchesProgress(uid, techServer, server);
        UpdateResearchesStatus(uid, techServer, server);
    }

    private void MakeResearch(
            EntityUid ent,
            ResearchServerNewComponent? server = null,
            ResearchDataServerComponent? data = null,
            PointsDataStorageComponent? pointsStorage = null)
    {
        if (!Resolve(ent, ref server, ref data, ref pointsStorage))
            return;

        foreach (var (key, (maxValue, _)) in server.PointsStatistic)
        {
            server.PointsStatistic[key] = (maxValue, 0);
        }

        var ev = new GetServerResearchEvent(ent);
        foreach (var client in server.Clients)
        {
            RaiseLocalEvent(client, ref ev);
        }

        foreach (var (categoryId, (rawData, pointsData)) in ev.ResearchData)
        {
            if (!data.ResearchedData.ContainsKey(categoryId))
            {
                var categoryProto = ProtoMan.Index(categoryId);
                data.ResearchedData.Add(categoryId, new ResearchField(categoryProto.ResearchTypes.ToArray(), categoryProto.MaxPoints));
            }

            var pointsSum = pointsData.GetTotal();
            if (pointsSum <= 0)
                continue;

            var addValue = data.ResearchedData[categoryId].ResearchData(rawData, pointsSum);

            var pointsCoof = addValue / pointsSum;

            TryModifyPoints(ent, pointsData * pointsCoof, true, server);
        }

        Dirty(ent, server);

        var ev2 = new ResearchServerNewUpdatedEvent();
        foreach (var client in server.Clients)
        {
            RaiseLocalEvent(client, ref ev2);
        }
    }

    private bool TryModifyPoints(EntityUid uid, ResearchPointsSpecifier pointsData, bool modifyStatistic = false, ResearchServerNewComponent? server = null, PointsDataStorageComponent? storage = null)
    {
        if (!Resolve(uid, ref server) || !Resolve(uid, ref storage))
            return false;

        TryWritePoints(uid, ref pointsData, out var writedData, storage);

        if (!modifyStatistic)
            return true;

        foreach (var (type, value) in writedData.PointsDict)
        {
            if (!server.PointsStatistic.ContainsKey(type))
                continue;

            if (modifyStatistic)
            {
                var (maxPoints, pointsPS) = server.PointsStatistic[type];
                server.PointsStatistic[type] = (maxPoints + value, pointsPS + value);
            }
        }

        Dirty(uid, server);

        return true;
    }

    public int GetResearchSpeed(EntityUid uid, ResearchServerNewComponent? server = null)
    {
        if (!Resolve(uid, ref server))
            return 0;

        var ev = new GetResearchSpeedEvent(1);

        foreach (var client in server.Clients)
        {
            RaiseLocalEvent(client, ref ev);
        }

        return ev.Speed;
    }

    /// <summary>
    /// Registers a client to the specified server.
    /// </summary>
    /// <param name="client">The client being registered</param>
    /// <param name="server">The server the client is being registered to</param>
    /// <param name="clientComponent"></param>
    /// <param name="serverComponent"></param>
    /// <param name="dirtyServer">Whether or not to dirty the server component after registration</param>
    public void RegisterClient(EntityUid client, EntityUid server, ResearchClientNewComponent? clientComponent = null,
        ResearchServerNewComponent? serverComponent = null,  bool dirtyServer = true)
    {
        if (!Resolve(client, ref clientComponent, false) || !Resolve(server, ref serverComponent, false))
            return;

        if (serverComponent.Clients.Contains(client))
            return;

        serverComponent.Clients.Add(client);
        clientComponent.Server = server;

        if (dirtyServer && !TerminatingOrDeleted(server))
            Dirty(server, serverComponent);

        var ev = new ResearchRegistrationNewChangedEvent(server);
        RaiseLocalEvent(client, ref ev);
    }

    /// <summary>
    /// Unregisterse a client from its server
    /// </summary>
    /// <param name="client"></param>
    /// <param name="clientComponent"></param>
    /// <param name="dirtyServer"></param>
    public void UnregisterClient(EntityUid client, ResearchClientNewComponent? clientComponent = null, bool dirtyServer = true)
    {
        if (!Resolve(client, ref clientComponent))
            return;

        if (clientComponent.Server is not { } server)
            return;

        UnregisterClient(client, server, clientComponent, dirtyServer: dirtyServer);
    }

    /// <summary>
    /// Unregisters a client from its server
    /// </summary>
    /// <param name="client"></param>
    /// <param name="server"></param>
    /// <param name="clientComponent"></param>
    /// <param name="serverComponent"></param>
    /// <param name="dirtyServer"></param>
    public void UnregisterClient(EntityUid client, EntityUid server, ResearchClientNewComponent? clientComponent = null,
        ResearchServerNewComponent? serverComponent = null, bool dirtyServer = true)
    {
        if (!Resolve(client, ref clientComponent, false) || !Resolve(server, ref serverComponent, false))
            return;

        serverComponent.Clients.Remove(client);
        clientComponent.Server = null;

        if (dirtyServer && !TerminatingOrDeleted(server))
        {
            Dirty(server, serverComponent);
        }

        var ev = new ResearchRegistrationNewChangedEvent(null);
        RaiseLocalEvent(client, ref ev);
    }
}
