using Content.Server._WL.Xenobiology.Components;
using Content.Server.Cargo.Components;
using Content.Server.DoAfter;
using Content.Server.PowerCell;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Nutrition.Components;
using Content.Shared.PowerCell;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Linq;
using Content.Server._WL.Slimes.Systems;
using Content.Shared._WL.Xenobiology.Events;
using Content.Server._WL.Slimes;
using Content.Shared._WL.Xenobiology;
using Content.Shared._WL.Slimes.Prototypes;
using Content.Shared._WL.Slimes.Components;

namespace Content.Server._WL.Xenobiology.Systems;

public sealed class SlimeScannerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PowerCellSystem _cell = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly SlimeSystem _slime = default!;

    //TODO сделать какую-нибудь отдельную систему и компонент, отвечающие за сканеры, по типу SimpleScannerComponent,
    //Потому что копировать код медицинского сканера каждый раз, когда добавляется новый сканер это жутко. Хотя хз.
    public override void Initialize()
    {
        SubscribeLocalEvent<SlimeScannerComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<SlimeScannerComponent, SlimeScannerDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<SlimeScannerComponent, EntGotInsertedIntoContainerMessage>(OnInsertedIntoContainer);
        SubscribeLocalEvent<SlimeScannerComponent, PowerCellSlotEmptyEvent>(OnPowerCellSlotEmpty);
        SubscribeLocalEvent<SlimeScannerComponent, DroppedEvent>(OnDropped);
    }

    public override void Update(float frameTime)
    {
        var scannerQuery = EntityQueryEnumerator<SlimeScannerComponent, TransformComponent>();
        while (scannerQuery.MoveNext(out var uid, out var component, out var transform))
        {
            if (component.NextUpdate > _timing.CurTime)
                continue;

            UpdateScannedUser(uid, component.ScannedEntity);

            if (component.ScannedEntity is not { } slime)
                continue;

            component.NextUpdate = _timing.CurTime + component.UpdateInterval;

            if (!TryComp<TransformComponent>(slime, out var slimeTransform))
                continue;
        }
    }

    private void OnAfterInteract(EntityUid uid, SlimeScannerComponent comp, AfterInteractEvent args)
    {
        if (args.Target == null || !args.CanReach || !HasComp<MobStateComponent>(args.Target) || !_cell.HasDrawCharge(uid, user: args.User) || !HasComp<SlimeComponent>(args.Target))
        {
            args.Handled = false;
            return;
        }

        _audio.PlayPvs(comp.ScanningBeginSound, uid);

        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, comp.ScanDelay, new SlimeScannerDoAfterEvent(), uid, target: args.Target, used: uid)
        {
            BreakOnMove = true,
            BlockDuplicate = true,
            DuplicateCondition = DuplicateConditions.SameTarget
        });
        args.Handled = true;
    }

    private void OnDoAfter(EntityUid uid, SlimeScannerComponent comp, SlimeScannerDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target == null || !_cell.HasDrawCharge(uid, user: args.User))
            return;

        _audio.PlayPvs(comp.ScanningEndSound, uid);

        OpenUserInterface(args.User, uid);
        BeginScanningSlime(uid, args.Target.Value, comp);
        args.Handled = true;
    }

    private void OnInsertedIntoContainer(EntityUid uid, SlimeScannerComponent comp, EntGotInsertedIntoContainerMessage args)
    {
        if (comp.ScannedEntity is { } slime)
            StopScanningSlime(uid, slime, comp);
    }

    private void OnPowerCellSlotEmpty(EntityUid uid, SlimeScannerComponent comp, PowerCellSlotEmptyEvent args)
    {
        if (comp.ScannedEntity is { } slime)
            StopScanningSlime(uid, slime, comp);
    }

    private void OnDropped(EntityUid uid, SlimeScannerComponent comp, DroppedEvent args)
    {
        if (comp.ScannedEntity is { } slime)
            StopScanningSlime(uid, slime, comp);
    }

    private void OpenUserInterface(EntityUid user, EntityUid scanner)
    {
        if (!TryComp<ActorComponent>(user, out var actor) || !_uiSystem.TryGetUi(scanner, SlimeScannerUiKey.Key, out var ui))
            return;

        _uiSystem.OpenUi(ui, actor.PlayerSession);
    }

    private void BeginScanningSlime(EntityUid uid, EntityUid target, SlimeScannerComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.ScannedEntity = target;

        _cell.SetPowerCellDrawEnabled(uid, true);

        UpdateScannedUser(uid, target);
    }

    private void StopScanningSlime(EntityUid uid, EntityUid target, SlimeScannerComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        _cell.SetPowerCellDrawEnabled(target, false);

        comp.ScannedEntity = null;
    }

    public void UpdateScannedUser(EntityUid entity, EntityUid? slime)
    {
        if (!_uiSystem.TryGetUi(entity, SlimeScannerUiKey.Key, out var ui))
            return;

        if (slime != null &&
            TryComp<SlimeComponent>(slime, out var slimeComp) &&
            TryComp<MobStateComponent>(slime, out var mobState) && mobState.CurrentState is not Shared.Mobs.MobState.Dead)
        {
            //Ключ словаря - прототип сущности, значение - форматированная строка
            var strings = new Dictionary<string, string>();
            _protoMan.EnumeratePrototypes<SlimeMutationPrototype>()
                .Where(mutation => mutation.SlimeGroup.Equals(slimeComp.SlimeGroupName, StringComparison.CurrentCultureIgnoreCase))
                .FirstOrDefault()?.MutationsData.ForEach(data =>
                {
                    strings.Add(data.Prototype, string.Join(Loc.GetString("slime-scanner-window-if-separator", ("andoror", data.RequiredAll ? "and" : "or")),
                        data.MutationConditions.Select(cond => cond.GetDescriptionString(EntityManager, _protoMan).Trim())));
                });

            var slimeCoreProto = _protoMan.Index<EntityPrototype>(slimeComp.CorePrototype);
            var slimeCoreCost = slimeCoreProto.Components.Values.FirstOrDefault(comp => comp.Component is StaticPriceComponent)?.Component as StaticPriceComponent;
            var slimeCoreResearchCost = slimeCoreProto.Components.Values.FirstOrDefault(comp => comp.Component is SlimeCoreComponent)?.Component as SlimeCoreComponent;

            var slimeCoreReagentColor =
                (slimeCoreProto.Components.Values.FirstOrDefault(comp => comp.Component is SolutionContainerManagerComponent)?.Component as SolutionContainerManagerComponent)?
                .Solutions?.FirstOrDefault().Value.GetColor(_protoMan) ?? Color.MediumPurple;

            var hungerComp = EnsureComp<HungerComponent>(slime.Value);

            var msg = new SlimeScannerScannedUserMessage(
                GetNetEntity(slime),
                _slime.GetLocLifeStage(slimeComp.CurrentAge),
                strings,
                slimeComp.CurrentMutationProbability,
                slimeCoreCost?.Price,
                slimeCoreResearchCost?.ResearchPoints,
                slimeCoreReagentColor,
                hungerComp.CurrentHunger,
                hungerComp.Thresholds[HungerThreshold.Okay],
                slimeComp.GrowthStage,
                slimeComp.GrowthData[slimeComp.CurrentAge].GrowthStageBound,
                slimeComp.Relationships.FirstOrNull(rship => rship.Key.Equals(entity))?.Value ?? 0);

            _uiSystem.SendUiMessage(ui, msg);
        }
        else
        {
            _uiSystem.SendUiMessage(ui, new SlimeScannerScannedUserMessage(null));
        }
    }
}

