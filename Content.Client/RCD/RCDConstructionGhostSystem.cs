using Content.Client.Hands.Systems;
using Content.Shared.Input;
using Content.Shared.Interaction;
using Content.Shared.RCD;
using Content.Shared.RCD.Components;
using Content.Shared.Verbs;
using Robust.Client.Placement;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.RCD;

/// <summary>
/// System for handling structure ghost placement in places where RCD can create objects.
/// </summary>
public sealed partial class RCDConstructionGhostSystem : EntitySystem
{
    private const string PlacementMode = nameof(AlignRCDConstruction);

    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IPlacementManager _placementManager = default!;
    [Dependency] private IPrototypeManager _protoManager = default!;
    [Dependency] private HandsSystem _hands = default!;
    [Dependency] private IEntityManager _entityManager = default!;
    [Dependency] private IGameTiming _timing = default!;

    private Direction _placementDirection = default;
    private bool _useMirrorPrototype = false;
    private ProtoId<RCDPrototype> _lastProtoId = default!;
    public event EventHandler? FlipConstructionPrototype;
    public override void Initialize()
    {
        base.Initialize();

        // WL-Changes-start: RPD port from Goob-Station
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.EditorFlipObject,
                new PointerInputCmdHandler(HandleFlip, outsidePrediction: true))
            .Register<RCDConstructionGhostSystem>();

        SubscribeLocalEvent<GetVerbsEvent<Verb>>(AddDeconstructVerb);
    }

    private void AddDeconstructVerb(GetVerbsEvent<Verb> args)
    {
        var user = args.User;
        var used = args.Using;
        if (used is not { } rcd
            || !TryComp<RCDComponent>(rcd, out var rcdComp))
            return;

        Verb verb = new()
        {
            Text = Loc.GetString("rcd-deconstruct-verb-text"),
            Icon = new SpriteSpecifier.Rsi(new("/Texture/Objects/Tools/rcd.rsi"), "icon"),
            //ClientExclusive = true,
            Act = () => RaisePredictiveEvent(new RCDDeconstructVerb(GetNetEntity(user), GetNetEntity(args.Target), GetNetEntity(rcd)))

        };
        args.Verbs.Add(verb);
    }

    public override void Shutdown()
    {
        CommandBinds.Unregister<RCDConstructionGhostSystem>();
        base.Shutdown();
    }

    private bool HandleFlip(in PointerInputCmdHandler.PointerInputCmdArgs args)
    {
        if (args.State == BoundKeyState.Down)
        {
            if (!_placementManager.IsActive || _placementManager.Eraser)
                return false;

            var placerEntity = _placementManager.CurrentPermission?.MobUid;

            if (!TryComp<RCDComponent>(placerEntity, out var rcd))
                return false;

            var prototype = _protoManager.Index(rcd.ProtoId);
            if (string.IsNullOrEmpty(prototype.MirrorPrototype))
                return false;

            _useMirrorPrototype = !rcd.UseMirrorPrototype;

            var useProto = _useMirrorPrototype ? prototype.MirrorPrototype : prototype.Prototype;
            CreatePlacer(placerEntity.Value, rcd, useProto, prototype.Mode);

            // tell the server

            RaiseNetworkEvent(new RCDConstructionGhostFlipEvent(GetNetEntity(placerEntity.Value), _useMirrorPrototype));
        }

        return true;
    }
    // WL-Changes-end

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Get current placer data
        var placerEntity = _placementManager.CurrentPermission?.MobUid;
        var placerProto = _placementManager.CurrentPermission?.EntityType;
        var placerIsRCD = HasComp<RCDComponent>(placerEntity);

        // Exit if erasing or the current placer is not an RCD (build mode is active)
        if (_placementManager.Eraser || (placerEntity != null && !placerIsRCD))
            return;

        // Determine if player is carrying an RCD in their active hand
        if (_playerManager.LocalSession?.AttachedEntity is not { } player)
            return;

        var heldEntity = _hands.GetActiveItem(player);

        // Don't open the placement overlay for client-side RCDs.
        // This may happen when predictively spawning one in your hands.
        if (heldEntity != null && IsClientSide(heldEntity.Value))
            return;

        if (!TryComp<RCDComponent>(heldEntity, out var rcd))
        {
            // If the player was holding an RCD, but is no longer, cancel placement
            if (placerIsRCD)
                _placementManager.Clear();

            return;
        }
        var prototype = _protoManager.Index(rcd.ProtoId);

        // Update the direction the RCD prototype based on the placer direction
        if (_placementDirection != _placementManager.Direction)
        {
            _placementDirection = _placementManager.Direction;
            RaiseNetworkEvent(new RCDConstructionGhostRotationEvent(GetNetEntity(heldEntity.Value), _placementDirection));
        }

        // WL-Changes-start: RPD port from Goob-Station
        // If the placer has not changed build it
        var useProto = (_useMirrorPrototype && !string.IsNullOrEmpty(prototype.MirrorPrototype)) ? prototype.MirrorPrototype : prototype.Prototype;
        if (heldEntity != placerEntity || useProto != placerProto)
            CreatePlacer(heldEntity.Value, rcd, useProto, prototype.Mode);

        /* // moved into another method
        // Create a new placer
        var newObjInfo = new PlacementInformation
        {
            MobUid = heldEntity.Value,
            PlacementOption = PlacementMode,
            EntityType = rcd.OverrideProtoId ?? prototype.Prototype,
            Range = (int)Math.Ceiling(SharedInteractionSystem.InteractionRange),
            IsTile = prototype.Mode == RcdMode.ConstructTile,
            UseEditorContext = false,
        };

        _placementManager.Clear();
        _placementManager.BeginPlacing(newObjInfo);
        */
    }

    private void CreatePlacer(EntityUid uid, RCDComponent component, string? prototype, RcdMode mode)
    {
        var newObjInfo = new PlacementInformation
        {
            MobUid = uid,
            PlacementOption = PlacementMode,
            EntityType = component.OverrideProtoId ?? prototype,
            Range = (int)Math.Ceiling(component.Range > 0 ? component.Range : SharedInteractionSystem.MaxRaycastRange),
            IsTile = mode == RcdMode.ConstructTile,
            UseEditorContext = false
        };

        _placementManager.Clear();
        _placementManager.BeginPlacing(newObjInfo);
    }
    // WL-Changes-end
}
