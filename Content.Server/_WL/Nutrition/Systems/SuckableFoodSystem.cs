using Content.Server._WL.Nutrition.Components;
using Content.Server._WL.Nutrition.Events;
using Content.Server.Body.Systems;
using Content.Server.Chemistry.EntitySystems;
using Content.Server.Forensics;
using Content.Server.Popups;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Prototypes;
using Robust.Server.Containers;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server._WL.Nutrition.Systems;

public sealed partial class SuckableFoodSystem : EntitySystem
{
    [Dependency] private ReactiveSystem _reactiveSystem = default!;
    [Dependency] private SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private ContainerSystem _container = default!;
    [Dependency] private ForensicsSystem _forensics = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private IPrototypeManager _protoMan = default!;
    [Dependency] private IComponentFactory _componentFactory = default!;
    [Dependency] private FlavorProfileSystem _flavor = default!;

    private const float UpdatePeriod = 2f; // in seconds
    private float _updateTimer = 0f;

    private static readonly LocId PutInMouthLoc = "food-sweets-put-in-mouth-popup-message";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SuckableFoodComponent, GotEquippedEvent>(OnEquip);
        SubscribeLocalEvent<SuckableFoodComponent, GotUnequippedEvent>(ResetSucker);

        SubscribeLocalEvent<SuckableFoodComponent, ComponentShutdown>(ResetSucker);

        SubscribeLocalEvent<SuckableFoodComponent, SuckableFoodDissolvedEvent>(OnDissolved);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _updateTimer += frameTime;

        var isNewLoop = _updateTimer >= UpdatePeriod;

        var query = EntityQueryEnumerator<SuckableFoodComponent, SolutionManagerComponent>();
        while (query.MoveNext(out var food, out var suckableComp, out var solContainerManComp))
        {
            if (!Exists(suckableComp.SuckingEntity))
            {
                suckableComp.SuckingEntity = null;
                continue;
            }

            if (isNewLoop)
            {
                var sucker = suckableComp.SuckingEntity.Value;

                if (!TryComp<BloodstreamComponent>(sucker, out var bloodstreamComp))
                    continue;

                suckableComp.CanSuck = _mobState.IsAlive(sucker); // TODO: вынести в отдельное событие
                if (!suckableComp.IsSucking)
                    continue;

                if (!EnsureSolutionEntity((food, suckableComp, solContainerManComp), out var solutionEntity, out var solution))
                    continue;

                var dissolvedSol = _solutionContainerSystem.SplitSolution(solutionEntity.Value, suckableComp.DissolveAmount * UpdatePeriod);

                if (solution.Volume == FixedPoint2.Zero)
                {
                    if (_container.TryGetContainingContainer(food, out var container))
                    {
                        var ev = new SuckableFoodDissolvedEvent((food, suckableComp), container, sucker);

                        RaiseLocalEvent(food, ev);
                        RaiseLocalEvent(ev);
                    }

                    continue;
                }

                _reactiveSystem.DoEntityReaction(sucker, dissolvedSol, ReactionMethod.Ingestion);
                _bloodstreamSystem.TryAddToBloodstream((sucker, bloodstreamComp), dissolvedSol);
            }
        }

        if (isNewLoop)
            _updateTimer -= UpdatePeriod;
    }

    public void SetState(Entity<SuckableFoodComponent> foodEnt, EntityUid? sucker)
    {
        var (food, comp) = foodEnt;

        comp.SuckingEntity = sucker;
    }

    public bool EnsureSolutionEntity(
        Entity<SuckableFoodComponent, SolutionManagerComponent?> foodEnt,
        [NotNullWhen(true)] out Entity<SolutionComponent>? solEnt,
        [NotNullWhen(true)] out Solution? solution)
    {
        solEnt = null;
        solution = null;

        if (!Resolve(foodEnt, ref foodEnt.Comp2, false))
            return false;

        if (!_solutionContainerSystem.EnsureSolution((foodEnt, foodEnt.Comp2), foodEnt.Comp1.Solution, out var ent))
            return false;

        solEnt = ent;
        solution = ent.Comp.Solution;

        return true;
    }

    private void OnEquip(EntityUid food, SuckableFoodComponent comp, GotEquippedEvent ev)
    {
        if (ev.SlotFlags.HasFlag(SlotFlags.MASK))
            _forensics.TransferDna(food, ev.EquipTarget);

        SetState((food, comp), ev.EquipTarget);

        if (!EnsureSolutionEntity((food, comp), out _, out var sol))
            return;

        var flavor = _flavor.GetLocalizedFlavorsMessage(food, ev.EquipTarget, sol);
        if (string.IsNullOrEmpty(flavor))
            return;

        var msg = Loc.GetString(PutInMouthLoc, ("flavor", flavor), ("entity", Identity.Name(food, EntityManager, ev.EquipTarget)));

        _popup.PopupEntity(msg, ev.EquipTarget, Filter.Entities(ev.EquipTarget), false);
    }

    private void ResetSucker<T>(EntityUid food, SuckableFoodComponent comp, T ev)
    {
        SetState((food, comp), null);
    }


    private void OnDissolved(EntityUid food, SuckableFoodComponent comp, SuckableFoodDissolvedEvent ev)
    {
        if (comp.DeleteOnEmpty)
        {
            _inventory.TryUnequip(ev.Sucker, ev.Container.ID, true, true);

            var msg = Loc.GetString("food-sweets-got-dissolved-popup-message", ("entity", Identity.Name(food, EntityManager)));
            _popup.PopupEntity(msg, ev.Sucker, Filter.Entities(ev.Sucker), true, Shared.Popups.PopupType.Medium);

            TryQueueDel(food);
        }

        if (comp.EquippedEntityOnDissolve != null)
        {
            if (_protoMan.TryIndex(comp.EquippedEntityOnDissolve.Value, out var proto)
                && proto.HasComponent<SuckableFoodComponent>(_componentFactory))
            {
                Log.Error($"EquippedEntityOnDissolve {comp.EquippedEntityOnDissolve.Value} on entity {ToPrettyString(food)} has {nameof(SuckableFoodComponent)}!");
                return;
            }

            var ent = SpawnNextToOrDrop(comp.EquippedEntityOnDissolve.Value, ev.Sucker, overrides: comp.ComponentsOverride);
            _inventory.TryEquip(ev.Sucker, ent, ev.Container.ID, true);
        }
    }
}
