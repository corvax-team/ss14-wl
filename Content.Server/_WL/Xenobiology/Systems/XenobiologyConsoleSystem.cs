using Content.Server._WL.Slimes;
using Content.Server.Actions;
using Content.Server.Administration.Commands;
using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Server.Medical.BiomassReclaimer;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Server.Storage.Components;
using Content.Shared._WL.Slimes.Components;
using Content.Shared._WL.Xenobiology;
using Content.Shared._WL.Xenobiology.Events;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Research.Components;
using Content.Shared.Storage;
using Content.Shared.Tag;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Server._WL.Xenobiology.Systems;

public sealed partial class XenobiologyConsoleSystem : EntitySystem
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly BiomassReclaimerSystem _bioReclaimer = default!;
    [Dependency] private readonly ItemSlotsSystem _slot = default!;
    [Dependency] private readonly SolutionContainerSystem _solution = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SlimeScannerSystem _slimeScanner = default!;

    [ValidatePrototypeId<EntityPrototype>]
    private const string XenobioConsoleMarkerEntityPrototype = "XenoConsoleCamera";

    [ValidatePrototypeId<TagPrototype>]
    private const string MonkeyCubeTag = "MonkeyCube";

    [ValidatePrototypeId<TagPrototype>]
    private const string BoxCardboardTag = "BoxCardboard";

    public const string XenobiologyConsoleBeakerSlot = "beakerSlot";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenobiologyConsoleComponent, InteractHandEvent>(OnAfterHandInteract);
        SubscribeLocalEvent<XenobiologyConsoleComponent, AfterInteractUsingEvent>(OnAfterInteract);

        //Returns
        SubscribeLocalEvent<XenobiologyConsoleComponent, AnchorStateChangedEvent>(OnUnanchor);
        SubscribeLocalEvent<XenobiologyConsoleComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<XenobiologyConsoleComponent, DamageChangedEvent>(OnConsoleDamageChanged);
        SubscribeLocalEvent<XenobiologyConsoleComponent, BreakageEventArgs>(OnDestruction);
        SubscribeLocalEvent<ActiveXenobiologyConsoleUserComponent, DamageChangedEvent>(OnUserDamageChanged);
        SubscribeLocalEvent<ActiveXenobiologyConsoleUserComponent, SleepStateChangedEvent>((uid, comp, _) => Return(uid, comp));
        SubscribeLocalEvent<ActiveXenobiologyConsoleUserComponent, MoveEvent>((uid, comp, _) => Return(uid, comp));

        //Actions
        SubscribeLocalEvent<XenobiologyConsoleCameraComponent, XenobioReturnActionEvent>(OnReturnActionEvent);
        SubscribeLocalEvent<XenobiologyConsoleCameraComponent, XenobioEjectEntityActionEvent>(OnEjectActionEvent);
        SubscribeLocalEvent<XenobiologyConsoleCameraComponent, XenobioPickUpEntityActionEvent>(OnPickUpActionEvent);
        SubscribeLocalEvent<XenobiologyConsoleCameraComponent, XenobioEjectPickedEntitiesActionEvent>(OnPickedEjectActionEvent);
        SubscribeLocalEvent<XenobiologyConsoleCameraComponent, XenobioInjectLiquidActionEvent>(OnInjectActionEvent);
        SubscribeLocalEvent<XenobiologyConsoleCameraComponent, XenobioScanActionEvent>(OnScanActionEvent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ActiveXenobiologyConsoleUserComponent>();
        while (query.MoveNext(out var uid, out var activeComp))
        {
            if (activeComp.Camera == null ||
                !TryComp<XenobiologyConsoleComponent>(activeComp.Console, out var consoleComp))
                continue;

            var consoleCoords = Transform(activeComp.Console).Coordinates;
            var markerCoords = Transform(activeComp.Camera.Value).Coordinates;

            if (markerCoords.InRange(EntityManager, _transform, consoleCoords, consoleComp.CameraMaxDistance))
                continue;

            Return(uid, activeComp);
        }

        var queryCameras = EntityQueryEnumerator<XenobiologyConsoleCameraComponent>();
        while (queryCameras.MoveNext(out var uid, out var consoleCameraComp))
        {
            if (consoleCameraComp.ScannedEntity == null)
                continue;

            if (consoleCameraComp.NextUpdate <= _gameTiming.CurTime)
            {
                consoleCameraComp.NextUpdate = _gameTiming.CurTime + consoleCameraComp.UiUpdateInterval;

                _slimeScanner.UpdateScannedUser(uid, consoleCameraComp.ScannedEntity.Value, _slimeScanner.GetRelationships(consoleCameraComp.User, consoleCameraComp.ScannedEntity) ?? 0);
            }

            if (!_ui.IsUiOpen(uid, SlimeScannerUiKey.Key))
                consoleCameraComp.ScannedEntity = null;
        }
    }

    private void OnAfterHandInteract(EntityUid uid, XenobiologyConsoleComponent comp, InteractHandEvent args)
    {
        if (!comp.IsAvailable || args.Handled || comp.User != null)
            return;

        if (!TryComp<ApcPowerReceiverComponent>(uid, out var powerReceiverComponent) || !powerReceiverComponent.Powered)
        {
            _popup.PopupCursor(Loc.GetString("base-computer-ui-component-not-powered", ("machine", uid)), args.User);
            return;
        }

        var coords = Transform(uid).Coordinates;
        var spawned = EntityManager.SpawnEntity(XenobioConsoleMarkerEntityPrototype, coords);

        #region AddActions
        _action.AddAction(spawned, "ActionXenobioConsoleReturn");
        _action.AddAction(spawned, "ActionXenobioConsoleEjectMonkey");
        _action.AddAction(spawned, "ActionXenobioConsolePickUpMonkey");
        _action.AddAction(spawned, "ActionXenobioConsoleEjectPicked");
        _action.AddAction(spawned, "ActionXenobioConsoleInject");
        _action.AddAction(spawned, "ActionXenobioConsoleScanSlime");
        #endregion

        Comp<XenobiologyConsoleCameraComponent>(spawned).User = args.User;

        if (!_mind.TryGetMind(args.User, out var mindId, out var mindComp))
            return;
        _mind.TransferTo(mindId, spawned, mind: mindComp);

        comp.User = args.User;

        var activeUserComp = EnsureComp<ActiveXenobiologyConsoleUserComponent>(args.User);
        activeUserComp.Console = args.Target;
        activeUserComp.Camera = spawned;

        comp.IsAvailable = false;
        args.Handled = true;
    }

    private void OnAfterInteract(EntityUid uid, XenobiologyConsoleComponent comp, AfterInteractUsingEvent args)
    {
        if (!args.CanReach || args.Handled)
            return;

        if (!TryComp<ApcPowerReceiverComponent>(uid, out var powerReceiverComponent) || !powerReceiverComponent.Powered)
        {
            _popup.PopupCursor(Loc.GetString("base-computer-ui-component-not-powered", ("machine", uid)), args.User);
            return;
        }

        var entProtos = GetEntityPrototypesFromCube(args.Used);
        if (entProtos.Count > 0)
        {
            foreach (var entityProto in entProtos)
            {
                AddEntityInBuffer(uid, entityProto, comp);
            }

            _audio.PlayPvs(comp.InsertSound, uid);
            QueueDel(args.Used);
        }
        else if (TryComp<SlimeCoreComponent>(args.Used, out var slimeCoreComp) &&
            TryComp<ResearchClientComponent>(uid, out var client) &&
            TryComp<ResearchServerComponent>(client.Server, out var server) &&
            server != null)
        {
            server.Points += slimeCoreComp.ResearchPoints;
            _audio.PlayPvs(comp.InsertSound, uid);
            QueueDel(args.Used);
        }
        else if (TryComp<StorageComponent>(args.Used, out var storageComp))
        {
            foreach (var cube in storageComp.StoredItems)
            {
                var entities = GetEntityPrototypesFromCube(cube.Key);
                foreach (var entity in entities)
                {
                    AddEntityInBuffer(uid, entity, comp);
                }
            }

            _audio.PlayPvs(comp.InsertSound, uid);
        }
    }

    public List<EntProtoId> GetEntityPrototypesFromCube(EntityUid cube, bool deleteCube = true)
    {
        var toReturn = new List<EntProtoId>();
        if (TryComp<RehydratableComponent>(cube, out var rehydComp))
        {
            toReturn = rehydComp.PossibleSpawns;
        }
        else if (_tag.HasTag(cube, MonkeyCubeTag)
            && TryComp<SpawnItemsOnUseComponent>(cube, out var spawnItemOnUseComp))
        {
            foreach (var item in spawnItemOnUseComp.Items)
            {
                if (item.PrototypeId == null)
                    continue;

                if (_protoMan.Index<EntityPrototype>(item.PrototypeId.Value.Id).Components.Values
                    .FirstOrDefault(comp => comp.Component is RehydratableComponent)?.Component is not RehydratableComponent rehyd)
                    continue;

                foreach (var spawn in rehyd.PossibleSpawns)
                    toReturn.Add(spawn);
            }
        }

        if (deleteCube && toReturn.Count > 0)
            QueueDel(cube);

        return toReturn;
    }

    public void AddEntityInBuffer(EntityUid console, EntProtoId target, XenobiologyConsoleComponent? comp = null)
    {
        if (!Resolve(console, ref comp))
            return;

        if (!comp.RehydratableCubes.TryAdd(target, 1))
            comp.RehydratableCubes[target] += 1;
    }

    private void OnUnanchor(EntityUid uid, XenobiologyConsoleComponent comp, AnchorStateChangedEvent args)
    {
        if (comp.User != null)
            Return(comp.User.Value);
    }

    private void OnPowerChanged(EntityUid uid, XenobiologyConsoleComponent comp, PowerChangedEvent args)
    {
        if (!args.Powered && comp.User != null)
            Return(comp.User.Value);
    }

    private void OnConsoleDamageChanged(EntityUid uid, XenobiologyConsoleComponent comp, DamageChangedEvent args)
    {
        if (comp.User == null || args.DamageIncreased is false)
            return;

        Return(comp.User.Value);
    }

    private void OnDestruction(EntityUid uid, XenobiologyConsoleComponent comp, BreakageEventArgs args)
    {
        if (comp.User == null)
            return;

        Return(comp.User.Value);
    }

    private void OnUserDamageChanged(EntityUid uid, ActiveXenobiologyConsoleUserComponent comp, DamageChangedEvent args)
    {
        if (args.DamageIncreased is false)
            return;

        Return(uid);
    }

    private void OnReturnActionEvent(EntityUid uid, XenobiologyConsoleCameraComponent comp, XenobioReturnActionEvent args)
    {
        if (args.Handled)
            return;

        Return(comp.User);

        args.Handled = true;
    }

    private void OnEjectActionEvent(EntityUid uid, XenobiologyConsoleCameraComponent comp, XenobioEjectEntityActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<ActiveXenobiologyConsoleUserComponent>(comp.User, out var activeComp))
            return;

        if (!TryComp<XenobiologyConsoleComponent>(activeComp.Console, out var consoleComp))
            return;

        var coords = Transform(uid).Coordinates;
        foreach (var proto in consoleComp.RehydratableCubes)
        {
            if (proto.Value <= 0)
                continue;

            consoleComp.RehydratableCubes[proto.Key] -= 1;
            EntityManager.SpawnEntity(proto.Key.Id, coords);
            args.Handled = true;
            return;
        }

        _popup.PopupCursor(Loc.GetString("xenobiology-console-camera-reject-action-warning"), uid);
        args.Handled = false;
    }

    private void OnPickUpActionEvent(EntityUid uid, XenobiologyConsoleCameraComponent comp, XenobioPickUpEntityActionEvent args)
    {
        if (args.Handled)
            return;

        if (TryComp<PullableComponent>(args.Target, out var pullableComp) && pullableComp.Puller is not null)
        {
            _popup.PopupCursor(Loc.GetString("xenobiology-console-camera-pick-up-pulling-entity"), uid);
            return;
        }

        if (comp.Buffer.Contains(args.Target))
            return;

        if (!IsEntityValid(args.Target))
        {
            _popup.PopupCursor(Loc.GetString("xenobiology-console-camera-pick-up-entity-is-not-valid"), uid);
            return;
        }

        comp.Buffer.Add(args.Target);
        _popup.PopupCursor(Loc.GetString("xenobiology-console-camera-picked-entity"), uid);
        args.Handled = true;
    }

    private void OnPickedEjectActionEvent(EntityUid uid, XenobiologyConsoleCameraComponent comp, XenobioEjectPickedEntitiesActionEvent args)
    {
        if (args.Handled)
            return;

        if (comp.Buffer.Count <= 0)
        {
            _popup.PopupCursor(Loc.GetString("xenobiology-console-camera-reject-picked-action-warning"), uid);
            return;
        }

        var bufferEnt = comp.Buffer.First();
        comp.Buffer.Remove(bufferEnt);
        if (!Exists(bufferEnt))
            return;

        var reclaimer = args.Target.GetEntitiesInTile()
            .Where(HasComp<BiomassReclaimerComponent>).FirstOrNull();

        _transform.SetCoordinates(bufferEnt, args.Target);

        if (reclaimer != null)
        {
            var reclaimerEnt = new Entity<BiomassReclaimerComponent>(reclaimer.Value, Comp<BiomassReclaimerComponent>(reclaimer.Value));

            if (_bioReclaimer.CanGib(reclaimerEnt, bufferEnt))
                _bioReclaimer.StartProcessing(bufferEnt, reclaimerEnt);
        }

        args.Handled = true;
    }

    private void OnInjectActionEvent(EntityUid uid, XenobiologyConsoleCameraComponent comp, XenobioInjectLiquidActionEvent args)
    {
        if (args.Handled)
            return;

        if (!IsEntityValid(args.Target))
        {
            _popup.PopupCursor(Loc.GetString("xenobiology-console-camera-pick-up-entity-is-not-valid"), uid);
            return;
        }

        if (!TryComp<ActiveXenobiologyConsoleUserComponent>(comp.User, out var active))
            return;

        //Source solution
        var beaker = _slot.GetItemOrNull(active.Console, SharedChemMaster.InputSlotName);
        if (beaker == null)
        {
            _popup.PopupCursor(Loc.GetString("xenobiology-console-camera-no-beaker"), uid);
            return;
        }

        if (!TryComp<SolutionContainerManagerComponent>(beaker, out var beakerSolContainerManComp))
            return;

        if (!_solution.TryGetSolution((beaker.Value, beakerSolContainerManComp), "beaker", out var beakerSolComp))
            if (!_solution.ResolveSolution((beaker.Value, beakerSolContainerManComp), "beaker", ref beakerSolComp))
                return;

        //Target Solution
        if (!TryComp<SolutionContainerManagerComponent>(args.Target, out var targetSolContainerManComp))
            return;

        if (!_solution.TryGetSolution((args.Target, targetSolContainerManComp), "chemicals", out var targetSolComp))
            if (!_solution.ResolveSolution((args.Target, targetSolContainerManComp), "chemicals", ref targetSolComp))
                return;

        //Transfer solutions
        var transferQuantity = EnsureComp<SolutionTransferComponent>(beaker.Value).TransferAmount;
        var transfered = beakerSolComp.Value.Comp.Solution.SplitSolution(transferQuantity);

        if (transfered.Volume <= 0)
        {
            _popup.PopupCursor(Loc.GetString("xenobiology-console-camera-liquid-end-warning"), uid);
            return;
        }

        targetSolComp.Value.Comp.Solution.AddSolution(transfered, _protoMan);

        var ensuredSolComp = EnsureComp<SolutionComponent>(beaker.Value);
        ensuredSolComp.Solution = beakerSolComp.Value.Comp.Solution;

        if (TryComp<AppearanceComponent>(beaker.Value, out var appearanceComp))
            _solution.UpdateAppearance((beaker.Value, ensuredSolComp, appearanceComp));

        args.Handled = true;
    }

    private void OnScanActionEvent(EntityUid uid, XenobiologyConsoleCameraComponent comp, XenobioScanActionEvent args)
    {
        if (args.Handled)
            return;

        if (!HasComp<SlimeComponent>(args.Target))
        {
            _popup.PopupCursor(Loc.GetString("xenobiology-console-camera-not-slime"), uid, Shared.Popups.PopupType.Small);
            return;
        }

        if (!TryComp<ActorComponent>(uid, out var actor))
            return;

        comp.ScannedEntity = args.Target;

        if (!_ui.IsUiOpen(uid, SlimeScannerUiKey.Key))
        {
            _ui.OpenUi(uid, SlimeScannerUiKey.Key, actor.PlayerSession);
            _slimeScanner.UpdateScannedUser(uid, args.Target, _slimeScanner.GetRelationships(comp.User, comp.ScannedEntity) ?? 0);
        }
        else _slimeScanner.UpdateScannedUser(uid, args.Target, _slimeScanner.GetRelationships(comp.User, comp.ScannedEntity) ?? 0);

        args.Handled = true;
    }

    public bool IsEntityValid(EntityUid entity)
    {
        var pickupEntityProto = Prototype(entity)?.ID;
        var isPickable = _protoMan.EnumeratePrototypes<EntityPrototype>()
            .Any(proto => proto.Components.Values
               .Any(entry => entry.Component is RehydratableComponent rehydratable && rehydratable.PossibleSpawns
                  .Any(spawn => spawn.Equals(pickupEntityProto))));

        if (HasComp<SlimeComponent>(entity) || isPickable)
            return true;

        return false;
    }

    public void Return(EntityUid user, ActiveXenobiologyConsoleUserComponent? comp = null)
    {
        if (!Resolve(user, ref comp))
            return;

        if (comp.Camera == null)
            return;

        if (!_mind.TryGetMind(comp.Camera.Value, out var mindId, out var mindComp))
            return;

        _ui.CloseUi(comp.Camera.Value, SlimeScannerUiKey.Key, mindComp.Session);

        _mind.TransferTo(mindId, user, mind: mindComp);

        var xenobioConsoleComp = Comp<XenobiologyConsoleComponent>(comp.Console);
        xenobioConsoleComp.IsAvailable = true;
        xenobioConsoleComp.User = null;

        RemComp<ActiveXenobiologyConsoleUserComponent>(user);

        QueueDel(comp.Camera);
    }
}
