using Content.Client.Hands.Systems;
using Content.Shared.Input;
using Content.Shared.Interaction;
using Content.Shared.RCD;
using Content.Shared.RCD.Components;
using Robust.Client.Placement;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Prototypes;

namespace Content.Client.RCD;

/// <summary>
/// System for handling structure ghost placement in places where RCD can create objects.
/// </summary>
public sealed partial class RCDConstructionGhostSystem : EntitySystem
{
    private const string PlacementMode = nameof(AlignRCDConstruction);
    private const string RpdPlacementMode = nameof(AlignRPDAtmosPipeLayers); // WL-Changes: rpd port from Fonky Station

    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IPlacementManager _placementManager = default!;
    [Dependency] private IPrototypeManager _protoManager = default!;
    [Dependency] private HandsSystem _hands = default!;

    private Direction _placementDirection = default;
    // WL-Changes-start: rpd port from Fonky Station
    private bool _useMirrorPrototype = false;

    public override void Initialize()
    {
        base.Initialize();

        // bind key
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.EditorFlipObject,
                new PointerInputCmdHandler(HandleFlip, outsidePrediction: true))
            .Register<RCDConstructionGhostSystem>();
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

            if (!TryComp<RCDComponent>(placerEntity, out var rcd) ||
                string.IsNullOrEmpty(_protoManager.Index(rcd.ProtoId).MirrorPrototype))
                return false;

            _useMirrorPrototype = !rcd.UseMirrorPrototype;

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
        // WL-Changes-start
        if (heldEntity != placerEntity)
            _useMirrorPrototype = rcd.UseMirrorPrototype;
        // WL-Changes-start: rpd port from FunkyStation
        // Determine if mirrored
        // WL-Changes-start: update code from port
        var curProto = _protoManager.Index<RCDPrototype>(rcd.ProtoId);
        var wantMirror = _useMirrorPrototype && !string.IsNullOrEmpty(curProto.MirrorPrototype);
        var prototype = wantMirror ? curProto.MirrorPrototype : curProto.Prototype;

        bool isLayered = rcd.IsRpd && curProto.HasLayers;

        var desiredMode = isLayered ? RpdPlacementMode : PlacementMode;
        // WL-Changes-end | x3

        // Update the direction the RCD prototype based on the placer direction
        if (_placementDirection != _placementManager.Direction)
        {
            _placementDirection = _placementManager.Direction;
            RaiseNetworkEvent(new RCDConstructionGhostRotationEvent(GetNetEntity(heldEntity.Value), _placementDirection));
        }

        // If the placer has not changed, exit
        // WL-Changes-start: rpd port from FunkyStation
        if (heldEntity == placerEntity &&
            prototype == placerProto &&
            _placementManager.CurrentPermission?.PlacementOption == desiredMode)
            return;
        // WL-Changes-end

        // Create a new placer
        var newObjInfo = new PlacementInformation
        {
            MobUid = heldEntity.Value,
            // WL-Changes-start: rpd port from FunkyStation
            PlacementOption = desiredMode,
            EntityType = prototype,
            // WL-Changes-end
            Range = (int)Math.Ceiling(SharedInteractionSystem.InteractionRange),
            IsTile = curProto.Mode == RcdMode.ConstructTile, // WL-Changes: rpd port from FunkyStation | update code from port
            UseEditorContext = false,
        };

        _placementManager.Clear();
        _placementManager.BeginPlacing(newObjInfo);
    }
}
