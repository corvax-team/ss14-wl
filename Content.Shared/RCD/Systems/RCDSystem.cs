using System.Linq;
using Content.Shared._Goobstation.Sparks;
using Content.Shared.Administration.Logs;
using Content.Shared.Atmos;
using Content.Shared.Atmos.EntitySystems;
using Content.Shared.Atmos.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Construction;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IgnitionSource;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.RCD.Components;
using Content.Shared.Tag;
using Content.Shared.Tiles;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared.RCD.Systems;

public sealed partial class RCDSystem : EntitySystem
{
    [Dependency] private INetManager _net = default!;
    [Dependency] private ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private ITileDefinitionManager _tileDefMan = default!;
    [Dependency] private FloorTileSystem _floors = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedChargesSystem _sharedCharges = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private TileSystem _tile = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private IPrototypeManager _protoManager = default!;
    [Dependency] private SharedMapSystem _mapSystem = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private TagSystem _tags = default!;
    // WL-Changes-start
    [Dependency] private SharedAtmosPipeLayersSystem _pipeLayersSystem = default!; // rpd port from FunkyStation
    [Dependency] private IEntityManager _entityManager = default!; // rpd port from FunkyStation
    [Dependency] private IRobustRandom _random = default!; // Ignition
    [Dependency] private SharedIgnitionSourceSystem _source = default!; // Ignition
    [Dependency] private SparksSystem _sparks = default!;
    [Dependency] private ExamineSystemShared _examine = default!; // BRPD
    // WL-Changes-end


    private readonly int _instantConstructionDelay = 0;
    private readonly EntProtoId _instantConstructionFx = "EffectRCDConstruct0";
    private readonly ProtoId<RCDPrototype> _deconstructTileProto = "DeconstructTile";
    private readonly ProtoId<RCDPrototype> _deconstructLatticeProto = "DeconstructLattice";
    private static readonly ProtoId<TagPrototype> CatwalkTag = "Catwalk";

    private HashSet<EntityUid> _intersectingEntities = new();

    [Access(Other = AccessPermissions.Read)]
    public Dictionary<string, (string Tooltip, SpriteSpecifier? Sprite)> PrototypesGroupingInfo = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RCDComponent, MapInitEvent>(OnMapInit);
        // WL-Changes-start: rpd port from FunkyStation
        SubscribeLocalEvent<RCDComponent, GetVerbsEvent<UtilityVerb>>(OnGetUtilityVerb);
        SubscribeLocalEvent<RCDComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAlternativeVerb);
        // WL-Changes-end
        SubscribeLocalEvent<RCDComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<RCDComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<RCDComponent, RCDDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<RCDComponent, DoAfterAttemptEvent<RCDDoAfterEvent>>(OnDoAfterAttempt);
        SubscribeLocalEvent<RCDComponent, RCDSystemMessage>(OnRCDSystemMessage);
        SubscribeNetworkEvent<RCDConstructionGhostRotationEvent>(OnRCDconstructionGhostRotationEvent);
        // WL-Changes-start
        SubscribeNetworkEvent<RCDConstructionGhostFlipEvent>(OnRCDConstructionGhostFlipEvent); // rpd port from FunkyStation
        SubscribeNetworkEvent<RPDEyeRotationEvent>(OnRPDEyeRotationEvent); // rpd port from FunkyStation
        SubscribeNetworkEvent<RDChangeModeEvent>(OnRDChangeModeEvent); // Ignition
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnProtoReload); // dehardcode
        SubscribeNetworkEvent<RPDLayerUpdateEvent>(OnLayerChanged);
        UpdateProtoList(); // dehardcode
        // WL-Changes-end
    }

    #region Event handling

    private void OnMapInit(EntityUid uid, RCDComponent component, MapInitEvent args)
    {
        // On init, set the RCD to its first available recipe
        if (component.AvailablePrototypes.Count > 0)
        {
            component.ProtoId = component.AvailablePrototypes.ElementAt(0);
            Dirty(uid, component);

            return;
        }

        // The RCD has no valid recipes somehow? Get rid of it
        QueueDel(uid);
    }

    private void OnRCDSystemMessage(EntityUid uid, RCDComponent component, RCDSystemMessage args)
    {
        // Exit if the RCD doesn't actually know the supplied prototype
        if (!component.AvailablePrototypes.Contains(args.ProtoId))
            return;

        if (!_protoManager.Resolve<RCDPrototype>(args.ProtoId, out var prototype))
            return;

        // Set the current RCD prototype to the one supplied
        component.ProtoId = args.ProtoId;

        _adminLogger.Add(LogType.RCD, LogImpact.Low, $"{args.Actor} set RCD mode to: {prototype.Mode} : {prototype.Prototype}");

        Dirty(uid, component);
    }

    // WL-Changes-start
    private void OnProtoReload(PrototypesReloadedEventArgs args) // dehardcode
    {
        if (args.WasModified<RDGroupPrototype>())
            UpdateProtoList();
    }

    private void UpdateProtoList() // deharcode
    {
        PrototypesGroupingInfo.Clear();
        var enume = _protoManager.EnumeratePrototypes<RDGroupPrototype>();
        foreach (var proto in enume)
        {
            if (proto.Name == null)
                continue;
            PrototypesGroupingInfo.Add(proto.ID, (proto.Name, proto.Sprite));
        }
    }

    private void OnLayerChanged(RPDLayerUpdateEvent ev, EntitySessionEventArgs session)
    {
        if (session.SenderSession.AttachedEntity is not { } player)
            return;

        var rcd = GetEntity(ev.NetEntity);

        if (!_hands.TryGetActiveItem(player, out var held) || held != rcd)
            return;

        if (!TryComp<RCDComponent>(rcd, out var rcdComp) || rcdComp.CurrentLayer == ev.NewLayer)
            return;

        rcdComp.CurrentLayer = ev.NewLayer;
        Dirty(rcd, rcdComp);
    }

    private void OnRDChangeModeEvent(RDChangeModeEvent ev, EntitySessionEventArgs session) // Ignition
    {
        if (session.SenderSession.AttachedEntity is not { } player)
            return;

        var rcd = GetEntity(ev.Rcd);


        if (!_hands.TryGetActiveItem(player, out var held) || held != rcd)
            return;

        if (!TryComp<RCDComponent>(rcd, out var rcdComp) || !rcdComp.EnableIgnite)
            return;

        if (_random.Prob(rcdComp.IgniteChance))
        {
            _source.SetIgnited((rcd, null), true);
            _sparks.DoSparks(Transform(rcd).Coordinates);
            Timer.Spawn(rcdComp.IgnitedTime, () => _source.SetIgnited((rcd, null), false));
        }
    }
    // WL-Changes-end

    private void OnExamine(EntityUid uid, RCDComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var prototype = _protoManager.Index(component.ProtoId);

        var msg = Loc.GetString("rcd-component-examine-mode-details", ("mode", Loc.GetString(prototype.SetName)));

        if (prototype.Mode == RcdMode.ConstructTile || prototype.Mode == RcdMode.ConstructObject)
        {
            var name = Loc.GetString(prototype.SetName);

            if (prototype.Prototype != null &&
                _protoManager.TryIndex(prototype.Prototype, out var proto)) // don't use Resolve because this can be a tile
                name = proto.Name;

            msg = Loc.GetString("rcd-component-examine-build-details", ("name", name));
        }

        args.PushMarkup(msg);

        // WL-Changes-start: rpd port from FunkyStation
        if (component.IsRpd)
        {
            var modeLoc = $"rcd-rpd-mode-{component.CurrentMode.ToString().ToLowerInvariant()}";
            args.PushMarkup(Loc.GetString("rcd-component-examine-rpd-mode", ("mode", Loc.GetString(modeLoc))));
        }
        // WL-Changes-end
    }

    // WL-Changes-start: rpd port from FunkyStation
    private void OnRPDEyeRotationEvent(RPDEyeRotationEvent ev, EntitySessionEventArgs session)
    {
        var uid = GetEntity(ev.NetEntity);

        if (session.SenderSession.AttachedEntity is not { } player)
            return;

        if (_hands.GetActiveItem(player) != uid)
            return;

        if (!TryComp<RCDComponent>(uid, out var rcd))
            return;

        // Update the layer if different
        if (rcd.LastKnownEyeRotation != ev.EyeRotation)
        {
            rcd.LastKnownEyeRotation = ev.EyeRotation;
            Dirty(uid, rcd); // WL-Changes: Sync
        }
    }

    private void OnGetUtilityVerb(EntityUid uid, RCDComponent component, GetVerbsEvent<UtilityVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !component.IsRpd)
            return;

        var verb = new UtilityVerb
        {
            Act = () => SwitchPipeMode(uid, component, args.User),
            Text = Loc.GetString("rcd-verb-switch-mode"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/settings.svg.192dpi.png")),
            Impact = LogImpact.Low
        };

        args.Verbs.Add(verb);
    }

    private void OnGetAlternativeVerb(EntityUid uid, RCDComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !component.IsRpd || !args.Using.HasValue)
            return;

        // Only show when alt-clicking the RPD itself (args.Using is the held item)
        if (args.Using.Value != uid)
            return;

        var verb = new AlternativeVerb
        {
            Act = () => SwitchPipeMode(uid, component, args.User),
            Text = Loc.GetString("rcd-verb-switch-mode"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/settings.svg.192dpi.png")),
            Impact = LogImpact.Low
        };

        args.Verbs.Add(verb);
    }
    // WL-Changes-end
    private void OnAfterInteract(EntityUid uid, RCDComponent component, AfterInteractEvent args)
    {
        // WL-Changes-start: BRPD
        var distance = component.Range > 0 ? component.Range : SharedInteractionSystem.MaxRaycastRange;
        var location = args.ClickLocation;
        if (args.Handled || !args.CanReach && !_transform.InRange(Transform(uid).Coordinates, location, distance))
            return;
        // WL-Changes-end

        var user = args.User;
        var prototype = _protoManager.Index(component.ProtoId);

        // Initial validity checks
        if (!location.IsValid(EntityManager))
            return;

        // Get grid corresponding to user's click location.
        // If that doesn't exist, try using the one they're standing on.
        // In the future we might want to also check adjacent spaces for grids,
        // in case the user is floating in space for whatever reason.
        var clickGridUid = _transform.GetGrid(location);
        var userGridUid = _transform.GetGrid(user);
        var gridUid = clickGridUid.HasValue ? clickGridUid : userGridUid;

        if (!TryComp<MapGridComponent>(gridUid, out var mapGrid))
        {
            _popup.PopupClient(Loc.GetString("rcd-component-no-valid-grid"), uid, user);
            return;
        }
        var tile = _mapSystem.GetTileRef(gridUid.Value, mapGrid, location);
        var position = _mapSystem.TileIndicesFor(gridUid.Value, mapGrid, location);

        if (!IsRCDOperationStillValid(uid, component, gridUid.Value, mapGrid, tile, position, component.ConstructionDirection, args.Target, args.User))
            return;

        if (!_net.IsServer)
            return;

        // Get the starting cost, delay, and effect from the prototype
        var cost = prototype.Cost;
        var delay = prototype.Delay;
        var effectPrototype = prototype.Effect;

        #region: Operation modifiers

        // Deconstruction modifiers
        switch (prototype.Mode)
        {
            case RcdMode.Deconstruct:

                // Deconstructing an object
                if (args.Target != null)
                {
                    if (TryComp<RCDDeconstructableComponent>(args.Target, out var destructible))
                    {
                        cost = destructible.Cost;
                        delay = destructible.Delay;
                        effectPrototype = destructible.Effect;
                    }
                }

                // Deconstructing a tile
                else
                {
                    var deconstructedTile = _mapSystem.GetTileRef(gridUid.Value, mapGrid, location);
                    var protoName = !_turf.IsSpace(deconstructedTile) ? _deconstructTileProto : _deconstructLatticeProto;

                    if (_protoManager.Resolve(protoName, out var deconProto))
                    {
                        cost = deconProto.Cost;
                        delay = deconProto.Delay;
                        effectPrototype = deconProto.Effect;
                    }
                }

                break;

            case RcdMode.ConstructTile:

                // If replacing a tile, make the construction instant
                var contructedTile = _mapSystem.GetTileRef(gridUid.Value, mapGrid, location);

                if (!contructedTile.Tile.IsEmpty)
                {
                    delay = _instantConstructionDelay;
                    effectPrototype = _instantConstructionFx;
                }

                break;
        }

        #endregion

        // Try to start the do after
        var effect = Spawn(effectPrototype, _mapSystem.ToCenterCoordinates(tile, mapGrid));
        var ev = new RCDDoAfterEvent(
            GetNetCoordinates(location),
            GetNetEntity(gridUid.Value),
            component.ConstructionDirection,
            component.ProtoId,
            cost,
            GetNetEntity(effect));
        var doAfterArgs = new DoAfterArgs(EntityManager, user, delay, ev, uid, target: args.Target, used: uid)
        {
            NeedHand = true,
            BreakOnDamage = true,
            BreakOnHandChange = true,
            BreakOnMove = true,
            AttemptFrequency = AttemptFrequency.EveryTick,
            CancelDuplicate = false,
            BlockDuplicate = false,
            DistanceThreshold = distance // WL-Changes: BRPD
        };

        args.Handled = true;

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
            QueueDel(effect);
    }

    private void OnDoAfterAttempt(EntityUid uid, RCDComponent component, DoAfterAttemptEvent<RCDDoAfterEvent> args)
    {
        if (args.Event?.DoAfter?.Args == null)
            return;

        // Exit if the RCD prototype has changed
        if (component.ProtoId != args.Event.StartingProtoId)
        {
            args.Cancel();
            return;
        }

        // Ensure the RCD operation is still valid
        var gridUid = GetEntity(args.Event.TargetGridId);

        if (!TryComp<MapGridComponent>(gridUid, out var mapGrid))
        {
            args.Cancel();
            return;
        }

        var location = GetCoordinates(args.Event.Location);
        var tile = _mapSystem.GetTileRef(gridUid, mapGrid, location);
        var position = _mapSystem.TileIndicesFor(gridUid, mapGrid, location);

        if (!IsRCDOperationStillValid(uid, component, gridUid, mapGrid, tile, position, args.Event.Direction, args.Event.Target, args.Event.User))
            args.Cancel();
    }

    private void OnDoAfter(EntityUid uid, RCDComponent component, RCDDoAfterEvent args)
    {
        if (args.Cancelled)
        {
            // Delete the effect entity if the do-after was cancelled (server-side only)
            if (_net.IsServer)
                QueueDel(GetEntity(args.Effect));
            return;
        }

        if (args.Handled)
            return;

        args.Handled = true;

        var gridUid = GetEntity(args.TargetGridId);

        if (!TryComp<MapGridComponent>(gridUid, out var mapGrid))
            return;

        var location = GetCoordinates(args.Location);
        var tile = _mapSystem.GetTileRef(gridUid, mapGrid, location);
        var position = _mapSystem.TileIndicesFor(gridUid, mapGrid, location);

        // Ensure the RCD operation is still valid
        if (!IsRCDOperationStillValid(uid, component, gridUid, mapGrid, tile, position, args.Direction, args.Target, args.User))
        {
            return;
        }

        // Finalize the operation (this should handle prediction properly)
        FinalizeRCDOperation(uid, component, gridUid, mapGrid, tile, position, args.Direction, args.Target, args.User);

        // Play audio and consume charges
        _audio.PlayPredicted(component.SuccessSound, uid, args.User);
        _sharedCharges.AddCharges(uid, -args.Cost);
    }

    private void OnRCDconstructionGhostRotationEvent(RCDConstructionGhostRotationEvent ev, EntitySessionEventArgs session)
    {
        var uid = GetEntity(ev.NetEntity);

        // Determine if player that send the message is carrying the specified RCD in their active hand
        if (session.SenderSession.AttachedEntity is not { } player)
            return;

        if (_hands.GetActiveItem(player) != uid)
            return;

        if (!TryComp<RCDComponent>(uid, out var rcd))
            return;

        // Update the construction direction
        rcd.ConstructionDirection = ev.Direction;
        Dirty(uid, rcd);
    }

    // WL-Changes-start: rpd port from FunkyStation
    private void OnRCDConstructionGhostFlipEvent(RCDConstructionGhostFlipEvent ev, EntitySessionEventArgs session)
    {
        var uid = GetEntity(ev.NetEntity);

        if (session.SenderSession.AttachedEntity is not { } player)
            return;

        if (_hands.GetActiveItem(player) != uid)
            return;

        if (!TryComp<RCDComponent>(uid, out var rcd))
            return;

        rcd.UseMirrorPrototype = ev.UseMirrorPrototype;
        Dirty(uid, rcd);
    }

    private void SwitchPipeMode(EntityUid uid, RCDComponent component, EntityUid? user = null)
    {
        if (!component.IsRpd)
            return;

        // Cycle through modes
        component.CurrentMode = component.CurrentMode switch
        {
            RpdMode.Primary => RpdMode.Secondary,
            RpdMode.Secondary => RpdMode.Tertiary,
            RpdMode.Tertiary => RpdMode.Free,
            RpdMode.Free => RpdMode.Primary,
            _ => RpdMode.Free
        };

        Dirty(uid, component);

        if (user != null)
            _audio.PlayPredicted(component.SoundSwitchMode, uid, user.Value);
    }
    // WL-Changes-end: rpd port from FunkyStation

    #endregion

    #region Entity construction/deconstruction rule checks

    public bool IsRCDOperationStillValid(EntityUid uid, RCDComponent component, EntityUid gridUid, MapGridComponent mapGrid, TileRef tile, Vector2i position, EntityUid? target, EntityUid user, bool popMsgs = true)
    {
        return IsRCDOperationStillValid(uid, component, gridUid, mapGrid, tile, position, component.ConstructionDirection, target, user, popMsgs);
    }

    public bool IsRCDOperationStillValid(EntityUid uid, RCDComponent component, EntityUid gridUid, MapGridComponent mapGrid, TileRef tile, Vector2i position, Direction direction, EntityUid? target, EntityUid user, bool popMsgs = true)
    {
        var prototype = _protoManager.Index(component.ProtoId);

        // Check that the RCD has enough ammo to get the job done
        var charges = _sharedCharges.GetCurrentCharges(uid);

        // Both of these were messages were suppose to be predicted, but HasInsufficientCharges wasn't being checked on the client for some reason?
        if (charges == 0)
        {
            if (popMsgs)
                _popup.PopupClient(Loc.GetString("rcd-component-no-ammo-message"), uid, user);

            return false;
        }

        if (prototype.Cost > charges)
        {
            if (popMsgs)
                _popup.PopupClient(Loc.GetString("rcd-component-insufficient-ammo-message"), uid, user);

            return false;
        }

        // WL-Changes-start: BRPD
        var fail = false;

        // Exit if the target / target location is obstructed
        if (component.Range > 0)
        {
            var unobstructedBasic = target == null
            ? _interaction.InRangeUnobstructed(user, _mapSystem.GridTileToWorld(gridUid, mapGrid, position), component.Range)
            : _interaction.InRangeUnobstructed(user, target.Value, component.Range);
            fail = !unobstructedBasic;
        }
        else
        {
            var unobstructedVision = target == null
            ? _examine.InRangeUnOccluded(user, _mapSystem.GridTileToWorld(gridUid, mapGrid, position), component.Range)
            : _examine.InRangeUnOccluded(user, target.Value, component.Range);
            var unobstructedBasic = target == null
            ? _interaction.InRangeUnobstructed(user, _mapSystem.GridTileToWorld(gridUid, mapGrid, position), component.Range)
            : _interaction.InRangeUnobstructed(user, target.Value, component.Range);

            fail = !unobstructedVision && !unobstructedBasic;
        }

        if (fail)
        {
            if (popMsgs)
                _popup.PopupClient(Loc.GetString("interaction-system-user-interaction-cannot-reach"), user, user);
            return false;
        }
        // WL-Changes-end

        // Return whether the operation location is valid
        switch (prototype.Mode)
        {
            case RcdMode.ConstructTile:
            case RcdMode.ConstructObject:
                return IsConstructionLocationValid(uid, component, gridUid, mapGrid, tile, position, direction, user, popMsgs);
            case RcdMode.Deconstruct:
                return IsDeconstructionStillValid(uid, component, tile, target, user, popMsgs); // WL-Changes: rpd port from FunkyStation
        }

        return false;
    }

    private bool IsConstructionLocationValid(EntityUid uid, RCDComponent component, EntityUid gridUid, MapGridComponent mapGrid, TileRef tile, Vector2i position, Direction direction, EntityUid user, bool popMsgs = true)
    {
        var prototype = _protoManager.Index(component.ProtoId);

        // Check rule: Must build on empty tile
        if (prototype.ConstructionRules.Contains(RcdConstructionRule.MustBuildOnEmptyTile) && !tile.Tile.IsEmpty)
        {
            if (popMsgs)
                _popup.PopupClient(Loc.GetString("rcd-component-must-build-on-empty-tile-message"), uid, user);

            return false;
        }

        // Check rule: Must build on non-empty tile
        if (!prototype.ConstructionRules.Contains(RcdConstructionRule.CanBuildOnEmptyTile) && tile.Tile.IsEmpty)
        {
            if (popMsgs)
                _popup.PopupClient(Loc.GetString("rcd-component-cannot-build-on-empty-tile-message"), uid, user);

            return false;
        }

        // Check rule: Must place on subfloor
        if (prototype.ConstructionRules.Contains(RcdConstructionRule.MustBuildOnSubfloor) && !_turf.GetContentTileDefinition(tile).IsSubFloor)
        {
            if (popMsgs)
                _popup.PopupClient(Loc.GetString("rcd-component-must-build-on-subfloor-message"), uid, user);

            return false;
        }

        // Tile specific rules
        if (prototype.Mode == RcdMode.ConstructTile)
        {
            // Check rule: Tile placement is valid
            if (!_floors.CanPlaceTile(gridUid, mapGrid, tile.GridIndices, out var reason))
            {
                if (popMsgs)
                    _popup.PopupClient(reason, uid, user);

                return false;
            }

            var tileDef = _turf.GetContentTileDefinition(tile);

            // Check rule: Respect baseTurf and baseWhitelist
            if (prototype.Prototype != null && _tileDefMan.TryGetDefinition(prototype.Prototype, out var replacementDef))
            {
                var replacementContentDef = (ContentTileDefinition) replacementDef;

                if (replacementContentDef.BaseTurf != tileDef.ID && !replacementContentDef.BaseWhitelist.Contains(tileDef.ID))
                {
                    if (popMsgs)
                        _popup.PopupClient(Loc.GetString("rcd-component-cannot-build-on-empty-tile-message"), uid, user);

                    return false;
                }
            }

            // Check rule: Tiles can't be identical
            if (tileDef.ID == prototype.Prototype)
            {
                if (popMsgs)
                    _popup.PopupClient(Loc.GetString("rcd-component-cannot-build-identical-tile"), uid, user);

                return false;
            }

            // Ensure that all construction rules shared between tiles and object are checked before exiting here
            return true;
        }

        // Entity specific rules

        // Check rule: The tile is unoccupied
        var isWindow = prototype.ConstructionRules.Contains(RcdConstructionRule.IsWindow);
        var isCatwalk = prototype.ConstructionRules.Contains(RcdConstructionRule.IsCatwalk);

        _intersectingEntities.Clear();
        _lookup.GetLocalEntitiesIntersecting(gridUid, position, _intersectingEntities, -0.05f, LookupFlags.Uncontained);

        // WL-Changes-start
        var proto = (component.UseMirrorPrototype && !string.IsNullOrEmpty(prototype.MirrorPrototype))
                    ? prototype.MirrorPrototype
                    : prototype.Prototype;

        if (component.IsRpd && prototype.HasLayers && proto != null &&
            _protoManager.TryIndex<EntityPrototype>(proto, out var entityProto) &&
            entityProto.TryGetComponent<AtmosPipeLayersComponent>(out var atmosPipeLayers, _entityManager.ComponentFactory) &&
            _pipeLayersSystem.TryGetAlternativePrototype(atmosPipeLayers, component.CurrentLayer, out var newProtoId))
            proto = newProtoId;
        // WL-Changes-end

        foreach (var ent in _intersectingEntities)
        {
            // If the entity is the exact same prototype as what we are trying to build, then block it.
            // This is to prevent spamming objects on the same tile (e.g. lights)
            if (proto != null && MetaData(ent).EntityPrototype?.ID == proto) // WL-Changes
            {
                var isIdentical = true;

                if (prototype.AllowMultiDirection)
                {
                    var entDirection = Transform(ent).LocalRotation.GetCardinalDir();
                    if (entDirection != direction)
                        isIdentical = false;
                }

                // WL-Changes-start: pipes for rpd
                if (prototype.AllowCrossDirection)
                {
                    var entDirection = Transform(ent).LocalRotation.GetCardinalDir();
                    if (entDirection is Direction.South or Direction.North)
                    {
                        if (entDirection is Direction.East or Direction.West)
                            isIdentical = false;
                    }
                    else if (entDirection is Direction.East or Direction.West)
                    {
                        if (entDirection is Direction.South or Direction.North)
                            isIdentical = false;
                    }
                }
                // WL-Changes-end

                if (isIdentical)
                {
                    if (popMsgs)
                        _popup.PopupClient(Loc.GetString("rcd-component-cannot-build-identical-entity"), uid, user);

                    return false;
                }
            }

            if (isWindow && HasComp<SharedCanBuildWindowOnTopComponent>(ent))
                continue;

            if (isCatwalk && _tags.HasTag(ent, CatwalkTag))
            {
                if (popMsgs)
                    _popup.PopupClient(Loc.GetString("rcd-component-cannot-build-on-occupied-tile-message"), uid, user);

                return false;
            }

            if (prototype.CollisionMask != CollisionGroup.None && TryComp<FixturesComponent>(ent, out var fixtures))
            {
                foreach (var fixture in fixtures.Fixtures.Values)
                {
                    // Continue if no collision is possible
                    if (!fixture.Hard || fixture.CollisionLayer <= 0 || (fixture.CollisionLayer & (int) prototype.CollisionMask) == 0)
                        continue;

                    // Continue if our custom collision bounds are not intersected
                    if (prototype.CollisionPolygon != null &&
                        !DoesCustomBoundsIntersectWithFixture(prototype.CollisionPolygon, component.ConstructionTransform, ent, fixture))
                        continue;

                    // Collision was detected
                    if (popMsgs)
                        _popup.PopupClient(Loc.GetString("rcd-component-cannot-build-on-occupied-tile-message"), uid, user);

                    return false;
                }
            }
        }

        return true;
    }

    private bool IsDeconstructionStillValid(EntityUid uid, RCDComponent component, TileRef tile, EntityUid? target, EntityUid user, bool popMsgs = true) // WL-Changes: rpd port from FunkyStation
    {
        // Attempt to deconstruct a floor tile
        if (target == null)
        {
            // WL-Changes-start: rpd port from FunkyStation
            /* commented by WL, if u need - uncomment
            if (component.IsRpd)
            {
                if (popMsgs)
                    _popup.PopupClient(Loc.GetString("rcd-component-deconstruct-target-not-on-whitelist-message"), uid, user);

                return false;
            }
            */
            // WL-Changes-end

            // The tile is empty
            if (tile.Tile.IsEmpty)
            {
                if (popMsgs)
                    _popup.PopupClient(Loc.GetString("rcd-component-nothing-to-deconstruct-message"), uid, user);

                return false;
            }

            // The tile has a structure sitting on it
            if (_turf.IsTileBlocked(tile, CollisionGroup.MobMask))
            {
                if (popMsgs)
                    _popup.PopupClient(Loc.GetString("rcd-component-tile-obstructed-message"), uid, user);

                return false;
            }

            // The tile cannot be destroyed
            var tileDef = _turf.GetContentTileDefinition(tile);

            if (tileDef.Indestructible)
            {
                if (popMsgs)
                    _popup.PopupClient(Loc.GetString("rcd-component-tile-indestructible-message"), uid, user);

                return false;
            }
        }

        // Attempt to deconstruct an object
        else
        {
            // WL-Changes-start: rpd port from FunkyStation
            // The object is not in the RPD whitelist
            if (!TryComp<RCDDeconstructableComponent>(target, out var deconstructible) || !deconstructible.RpdDeconstructable && component.IsRpd)
            {
                if (popMsgs)
                    _popup.PopupClient(Loc.GetString("rcd-component-deconstruct-target-not-on-whitelist-message"), uid, user);

                return false;
            }

            // The object is not in the whitelist
            if (!deconstructible.Deconstructable)
            // WL-Changes-end
            {
                if (popMsgs)
                    _popup.PopupClient(Loc.GetString("rcd-component-deconstruct-target-not-on-whitelist-message"), uid, user);

                return false;
            }
        }

        return true;
    }

    #endregion

    #region Entity construction/deconstruction

    private void FinalizeRCDOperation(EntityUid uid, RCDComponent component, EntityUid gridUid, MapGridComponent mapGrid, TileRef tile, Vector2i position, Direction direction, EntityUid? target, EntityUid user)
    {
        if (!_net.IsServer)
            return;

        var prototype = _protoManager.Index(component.ProtoId);

        if (prototype.Prototype == null)
            return;

        switch (prototype.Mode)
        {
            case RcdMode.ConstructTile:
                if (!_tileDefMan.TryGetDefinition(prototype.Prototype, out var tileDef))
                    return;

                _tile.ReplaceTile(tile, (ContentTileDefinition) tileDef, gridUid, mapGrid);
                _adminLogger.Add(LogType.RCD, LogImpact.High, $"{ToPrettyString(user):user} used RCD to set grid: {gridUid} {position} to {prototype.Prototype}");
                break;

            case RcdMode.ConstructObject:
                // WL-Changes-start: rpd port from FunkyStation
                var proto = (component.UseMirrorPrototype && !string.IsNullOrEmpty(prototype.MirrorPrototype))
                    ? prototype.MirrorPrototype
                    : prototype.Prototype;

                var setLayer = false; // WL_changes

                if (component.IsRpd && prototype.HasLayers)
                {
                    if (_protoManager.TryIndex<EntityPrototype>(proto, out var entityProto) &&
                        entityProto.TryGetComponent<AtmosPipeLayersComponent>(out var atmosPipeLayers, _entityManager.ComponentFactory)) // WL-changes
                    {
                        if (_pipeLayersSystem.TryGetAlternativePrototype(atmosPipeLayers, component.CurrentLayer, out var newProtoId))
                            proto = newProtoId;
                        else
                            setLayer = true; // WL-changes
                    }
                }

                // Calculate rotation before spawn
                var rotation = prototype.Rotation switch
                {
                    RcdRotation.Fixed => Angle.Zero,
                    RcdRotation.Camera => Transform(uid).LocalRotation,
                    RcdRotation.User => direction.ToAngle(),
                    _ => Angle.Zero // Fallback
                };

                var entityCoords = _mapSystem.GridTileToLocal(gridUid, mapGrid, position);
                var mapCoords = _transform.ToMapCoordinates(entityCoords);

                var ent = Spawn(proto, mapCoords, rotation: rotation);
                // WL-Changes-end

                switch (prototype.Rotation)
                {
                    case RcdRotation.Fixed:
                        Transform(ent).LocalRotation = Angle.Zero;
                        break;
                    case RcdRotation.Camera:
                        Transform(ent).LocalRotation = Transform(uid).LocalRotation;
                        break;
                    case RcdRotation.User:
                        Transform(ent).LocalRotation = direction.ToAngle();
                        break;
                }

                if (setLayer && TryComp<AtmosPipeLayersComponent>(ent, out var layers)) // WL-changes
                    _pipeLayersSystem.SetPipeLayer((ent, layers), component.CurrentLayer);

                _adminLogger.Add(LogType.RCD, LogImpact.High, $"{ToPrettyString(user):user} used RCD to spawn {ToPrettyString(ent)} at {position} on grid {gridUid}");
                break;

            case RcdMode.Deconstruct:

                if (target == null)
                {
                    // Deconstruct tile, don't drop tile as item
                    if (_tile.DeconstructTile(tile, spawnItem: false))
                        _adminLogger.Add(LogType.RCD, LogImpact.High, $"{ToPrettyString(user):user} used RCD to set grid: {gridUid} tile: {position} open to space");
                }
                else
                {
                    // Deconstruct object
                    _adminLogger.Add(LogType.RCD, LogImpact.High, $"{ToPrettyString(user):user} used RCD to delete {ToPrettyString(target):target}");
                    QueueDel(target);
                }

                break;
        }
    }

    #endregion

    #region Utility functions

    private bool DoesCustomBoundsIntersectWithFixture(PolygonShape boundingPolygon, Transform boundingTransform, EntityUid fixtureOwner, Fixture fixture)
    {
        var entXformComp = Transform(fixtureOwner);
        var entXform = new Transform(new(), entXformComp.LocalRotation);

        return boundingPolygon.ComputeAABB(boundingTransform, 0).Intersects(fixture.Shape.ComputeAABB(entXform, 0));
    }

    #endregion
}

[Serializable, NetSerializable]
public sealed partial class RCDDoAfterEvent : DoAfterEvent
{
    [DataField(required: true)]
    public NetCoordinates Location { get; private set; }

    [DataField(required: true)]
    public NetEntity TargetGridId { get; private set; }

    [DataField]
    public Direction Direction { get; private set; }

    [DataField]
    public ProtoId<RCDPrototype> StartingProtoId { get; private set; }

    [DataField]
    public int Cost { get; private set; } = 1;

    [DataField("fx")]
    public NetEntity? Effect { get; private set; }

    private RCDDoAfterEvent() { }

    public RCDDoAfterEvent(
        NetCoordinates location,
        NetEntity targetGridId,
        Direction direction,
        ProtoId<RCDPrototype>
        startingProtoId,
        int cost,
        NetEntity? effect = null)
    {
        Location = location;
        TargetGridId = targetGridId;
        Direction = direction;
        StartingProtoId = startingProtoId;
        Cost = cost;
        Effect = effect;
    }

    public override DoAfterEvent Clone()
    {
        return this;
    }
}
