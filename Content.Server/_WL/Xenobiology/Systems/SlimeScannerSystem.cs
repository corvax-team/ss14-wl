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
using System.Linq;
using Content.Server._WL.Slimes.Systems;
using Content.Shared._WL.Xenobiology.Events;
using Content.Server._WL.Slimes;
using Content.Shared._WL.Xenobiology;
using Content.Shared._WL.Slimes.Prototypes;
using Content.Shared._WL.Slimes.Components;
using Robust.Shared.Utility;

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
        while (scannerQuery.MoveNext(out var uid, out var component, out _))
        {
            if (component.NextUpdate > _timing.CurTime)
                continue;

            component.NextUpdate = _timing.CurTime + component.UpdateInterval;

            if (component.ScannedEntity != null && component.ScanningEntity != null)
                continue;

            var relPoints = GetRelationships(component.ScanningEntity, component.ScannedEntity) ?? 0;
            UpdateScannedUser(uid, component.ScannedEntity, relPoints);
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

        args.Handled = _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, comp.ScanDelay, new SlimeScannerDoAfterEvent(), uid, target: args.Target, used: uid)
        {
            BreakOnMove = true,
            BlockDuplicate = true,
            DuplicateCondition = DuplicateConditions.SameTarget
        });
    }

    public int? GetRelationships(EntityUid? scanningEntity, EntityUid? slime, SlimeComponent? slimeComp = null)
    {
        if (scanningEntity == null || slime == null)
            return null;

        if (!Resolve(slime.Value, ref slimeComp))
            return null;

        return slimeComp.Relationships.FirstOrNull(x => x.Key.Equals(scanningEntity.Value))?.Value;
    }

    private void OnDoAfter(EntityUid uid, SlimeScannerComponent comp, SlimeScannerDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target == null || !_cell.HasDrawCharge(uid, user: args.User))
            return;

        _audio.PlayPvs(comp.ScanningEndSound, uid);

        OpenUserInterface(args.User, uid);
        BeginScanningSlime(uid, args.User, args.Target.Value);
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

    private void BeginScanningSlime(EntityUid scanner, EntityUid user, EntityUid target, SlimeScannerComponent? comp = null)
    {
        if (!Resolve(scanner, ref comp))
            return;

        comp.ScannedEntity = target;
        comp.ScanningEntity = user;

        _cell.SetPowerCellDrawEnabled(scanner, true);

        UpdateScannedUser(scanner, target, GetRelationships(user, target) ?? 0);
    }

    private void StopScanningSlime(EntityUid scanner, EntityUid target, SlimeScannerComponent? comp = null)
    {
        if (!Resolve(scanner, ref comp))
            return;

        _cell.SetPowerCellDrawEnabled(target, false);

        comp.ScannedEntity = null;
        comp.ScanningEntity = null;
    }

    public void UpdateScannedUser(EntityUid scanner, EntityUid? slime, int relationshipPoints)
    {
        if (!_uiSystem.TryGetUi(scanner, SlimeScannerUiKey.Key, out var ui))
            return;

        if (slime != null &&
            TryComp<SlimeComponent>(slime, out var slimeComp) &&
            TryComp<MobStateComponent>(slime, out var mobState) && mobState.CurrentState is not Shared.Mobs.MobState.Dead)
        {
            //Условия мутаций
            //Ключ словаря - прототип сущности, значение - форматированная строка(описание)
            var strings = new Dictionary<string, string>();
            _protoMan.EnumeratePrototypes<SlimeMutationPrototype>()
                .Where(mutation => mutation.SlimeGroup.Equals(slimeComp.SlimeGroupName, StringComparison.CurrentCultureIgnoreCase))
                .FirstOrDefault()?.MutationsData.ForEach(data =>
                {
                    var conditionsStrings = data.MutationConditions
                        .Select(x => x.GetDescriptionString(EntityManager, _protoMan));

                    strings.Add(data.Prototype, JoinStringWithUnions(conditionsStrings, data.RequiredAll) ?? "");
                });

            //Цена ядра
            var slimeCoreProto = _protoMan.Index<EntityPrototype>(slimeComp.CorePrototype);
            var slimeCoreCost = slimeCoreProto.Components.Values.FirstOrDefault(comp => comp.Component is StaticPriceComponent)?.Component as StaticPriceComponent;
            var slimeCoreResearchCost = slimeCoreProto.Components.Values.FirstOrDefault(comp => comp.Component is SlimeCoreComponent)?.Component as SlimeCoreComponent;

            //Цвет ядра слайма
            var slimeCoreReagentColor =
                (slimeCoreProto.Components.Values.FirstOrDefault(comp => comp.Component is SolutionContainerManagerComponent)?.Component as SolutionContainerManagerComponent)?
                .Solutions?.FirstOrDefault().Value.GetColor(_protoMan) ?? Color.MediumPurple;

            //Текущий голод
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
                relationshipPoints);

            _uiSystem.SendUiMessage(ui, msg);
        }
        else
        {
            _uiSystem.SendUiMessage(ui, new SlimeScannerScannedUserMessage(null));
        }
    }

    private string? JoinStringWithUnions(IEnumerable<string>? @string, bool isAndUnion)
    {
        if (@string is null || !@string.Any())
            return null;

        var separator = Loc.GetString("slime-scanner-window-if-separator", ("andoror", isAndUnion ? "and" : "or"));
        var toReturn = string.Join(separator, @string).Trim();
        return toReturn;
    }
}

