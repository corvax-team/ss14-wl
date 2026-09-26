using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Power.EntitySystems;
using Content.Shared._WL.Research.Components;
using Content.Shared.Research.Components;
using Robust.Shared.Utility;

namespace Content.Server._WL.Research.Systems;

public sealed partial class ResearchSystemNew
{
    private void InitializeClient()
    {
        SubscribeLocalEvent<ResearchClientNewComponent, TerminalServerSelectionMessage>(OnTerminalSelect);

        SubscribeLocalEvent<ResearchClientNewComponent, ResearchClientSyncMessage>(OnClientSyncMessage);
        SubscribeLocalEvent<ResearchClientNewComponent, ResearchClientServerSelectedMessage>(OnClientSelected);
        SubscribeLocalEvent<ResearchClientNewComponent, ResearchClientServerDeselectedMessage>(OnClientDeselected);
    }

    private void OnClientSelected(EntityUid uid, ResearchClientNewComponent component, ResearchClientServerSelectedMessage args)
    {
        if (!TryGetServerById(uid, args.ServerId, out var serveruid, out var serverComponent))
            return;

        // Validate that we can access this server.
        if (!GetServers(uid).Contains((serveruid.Value, serverComponent)))
            return;

        UnregisterClient(uid, component);
        RegisterClient(uid, serveruid.Value, component, serverComponent);
    }

    private void OnClientDeselected(EntityUid uid, ResearchClientNewComponent component, ResearchClientServerDeselectedMessage args)
    {
        UnregisterClient(uid, clientComponent: component);
    }

    private void OnClientSyncMessage(EntityUid uid, ResearchClientNewComponent component, ResearchClientSyncMessage args)
    {
        UpdateClientInterface(uid, component);
    }

    [SubscribeLocalEvent]
    private void OnClientRegistrationChanged(Entity<ResearchClientNewComponent> ent, ref ResearchRegistrationNewChangedEvent args)
    {
        UpdateClientInterface(ent, ent.Comp);
    }

    [SubscribeLocalEvent]
    private void OnClientMapInit(Entity<ResearchClientNewComponent> ent, ref MapInitEvent args)
    {
        if (GetServers(ent).FirstOrNull() is { } server)
            RegisterClient(ent, server, ent.Comp, server);
    }

    [SubscribeLocalEvent]
    private void OnClientShutdown(Entity<ResearchClientNewComponent> ent, ref ComponentShutdown args)
    {
        UnregisterClient(ent, ent.Comp);
    }

    [SubscribeLocalEvent]
    private void OnClientUIOpen(Entity<ResearchClientNewComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateClientInterface(ent, ent.Comp);
    }

    [SubscribeLocalEvent]
    private void OnClientAnchorStateChanged(Entity<ResearchClientNewComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (args.Anchored)
        {
            if (ent.Comp.Server is not null)
                return;

            if (GetServers(ent).FirstOrNull() is { } server)
                RegisterClient(ent, server, ent, server);
        }
        else
        {
            UnregisterClient(ent, ent.Comp);
        }
    }

    private void OnTerminalSelect(EntityUid uid, ResearchClientNewComponent component, TerminalServerSelectionMessage args)
    {
        if (!this.IsPowered(uid, EntityManager))
            return;

        _uiSystem.TryToggleUi(uid, ResearchClientUiKey.Key, args.Actor);
    }

    private void UpdateClientInterface(EntityUid uid, ResearchClientNewComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        TryGetClientServer(uid, out _, out var serverComponent, component);

        var names = GetServerNames(uid);
        var state = new ResearchClientBoundInterfaceState(
            names.Length,
            names,
            GetServerIds(uid),
            serverComponent?.Id ?? -1);

        _uiSystem.SetUiState(uid, ResearchClientUiKey.Key, state);
    }

    public bool TryGetClientServer(EntityUid uid,
        [NotNullWhen(true)] out EntityUid? server,
        [NotNullWhen(true)] out ResearchServerNewComponent? serverComponent,
        ResearchClientNewComponent? component = null)
    {
        server = null;
        serverComponent = null;

        if (!Resolve(uid, ref component, false))
            return false;

        if (component.Server == null)
            return false;

        if (!TryComp(component.Server, out serverComponent))
            return false;

        server = component.Server;
        return true;
    }
}
