using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Systems;
using Content.Shared._WL.Research;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Timing;

namespace Content.Server._WL.Research.Systems;

public sealed partial class ResearchSystemNew : SharedResearchNewSystem
{
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private UserInterfaceSystem _uiSystem = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeClient();
        InitializeServer();
        InitializeSource();
        InitializeTerminal();
        InitializeData();
        InitializeCommands();
    }

    public HashSet<Entity<ResearchServerNewComponent>> GetServers(EntityUid client)
    {
        var clientXform = Transform(client);
        if (clientXform.GridUid is not { } grid)
            return [];

        var set = new HashSet<Entity<ResearchServerNewComponent>>();
        _lookup.GetGridEntities(grid, set);
        return set;
    }

    public bool TryGetServerById(EntityUid client, int id, [NotNullWhen(true)] out EntityUid? serverUid, [NotNullWhen(true)] out ResearchServerNewComponent? serverComponent)
    {
        serverUid = null;
        serverComponent = null;

        var query = GetServers(client);
        foreach (var (uid, server) in query)
        {
            if (server.Id != id)
                continue;
            serverUid = uid;
            serverComponent = server;
            return true;
        }
        return false;
    }

    public string[] GetServerNames(EntityUid client)
    {
        return GetServers(client).Select(x => x.Comp.ServerName).ToArray();
    }

    public int[] GetServerIds(EntityUid client)
    {
        return GetServers(client).Select(x => x.Comp.Id).ToArray();
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<ResearchServerNewComponent>();
        while (query.MoveNext(out var uid, out var server))
        {
            if (server.NextUpdateTime > _timing.CurTime)
                continue;
            server.NextUpdateTime = _timing.CurTime + server.ResearchConsoleUpdateTime;

            UpdateServer(uid, (int) server.ResearchConsoleUpdateTime.TotalSeconds, server);
        }
    }
}
