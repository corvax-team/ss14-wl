using Content.Shared._WL.Inventory.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids;
using Content.Shared.Fluids.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Inventory;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Physics.Events;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Shared._WL.BloodClothing
{
    public abstract partial class SharedFluidOnClothingSystem : EntitySystem
    {
        [Dependency] protected readonly SharedSolutionContainerSystem _solution = default!;
        [Dependency] protected readonly InventorySystem _inventory = default!;
        [Dependency] protected readonly InventorySlotsBlockingSystem _invSlotsBlock = default!;

        [Dependency] private readonly SharedPuddleSystem _puddle = default!;

        public const SlotFlags ExcludeSlotFlags =
            SlotFlags.EARS |
            SlotFlags.NECK |
            SlotFlags.BELT |
            SlotFlags.BACK |
            SlotFlags.OUTERCLOTHING;

        [Obsolete("ДОБАВИТЬ ЛОКАЛИЗАЦИЮ И ЦВЕТОВЫЕ ТЕГИ")]
        public static readonly Func<float, string> PersonExaminedPollutionMessage = (float pollution) =>
        {
            return pollution switch
            {
                <= 0.15f => "Одежда носителя выглядит чистой.",
                <= 0.45f => "Одежда носителя выглядит слегка грязной.",
                <= 0.75f => "Одежда носителя выглядит грязной.",
                _ => "Одежда носителя выглядит крайне грязной."
            };
        };

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<PuddleComponent, StartCollideEvent>(OnStartCollide);
            SubscribeLocalEvent<PuddleComponent, EndCollideEvent>(OnEndCollide);

            SubscribeLocalEvent<FluidableClothingComponent, ExaminedEvent>(OnFluidableExamined);
            SubscribeLocalEvent<InventoryComponent, ExaminedEvent>(OnInventoryExamined);

            SubscribeLocalEvent<SolutionContainerManagerComponent, MeleeHitEvent>(OnMeleeHit, [typeof(SharedPuddleSystem), typeof(OpenableSystem)]);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

        }

        private void OnStartCollide(EntityUid puddle, PuddleComponent comp, StartCollideEvent args)
        {
            var other = args.OtherEntity;

            if (!TryComp<InventoryComponent>(other, out var inventoryComp))
                return;

            SetAbsorbingEntityByPlayer((other, inventoryComp), comp.Solution);
        }

        private void OnEndCollide(EntityUid ent, PuddleComponent comp, EndCollideEvent args)
        {
            var other = args.OtherEntity;

            if (!TryComp<InventoryComponent>(other, out var inventoryComp))
                return;

            SetAbsorbingEntityByPlayer((other, inventoryComp), null);
        }

        private void OnFluidableExamined(EntityUid ent, FluidableClothingComponent comp, ExaminedEvent args)
        {
            if (!args.IsInDetailsRange)
                return;

            
        }

        private void OnInventoryExamined(EntityUid ent, InventoryComponent comp, ExaminedEvent args)
        {
            if (!args.IsInDetailsRange)
                return;

            var clothes = _invSlotsBlock.GetAvailableWornClothes<FluidableClothingComponent>(
                    ent: (ent, null, comp),
                    excludeFlags: ExcludeSlotFlags
                );

            if (!clothes.Any())
                return;

            var max = 0f;
            var sum = 0f;

            foreach (var cloth in clothes)
            {
                var clothEntity = cloth.Owner;
                var fluidableComp = cloth.Comp;

                if (!_solution.TryGetSolution(clothEntity, fluidableComp.Solution, out _, out var solution))
                    continue;

                max += solution.MaxVolume.Float();
                sum += solution.Volume.Float();
            }

            var pollutionStageFloatNumber = 0f;
            if (max != 0f)
                pollutionStageFloatNumber = sum / max;

            var msg = PersonExaminedPollutionMessage.Invoke(pollutionStageFloatNumber);

            args.PushMarkup(
                markup: Loc.GetString(msg),
                priority: -3
            );
        }

        private void OnMeleeHit(EntityUid weapon, SolutionContainerManagerComponent weaponSolComp, MeleeHitEvent args)
        {
            if (args.Handled)
                return;

            var targets = args.HitEntities;

            if (!_solution.TryGetDrainableSolution(weapon, out _, out var solution))
                return;

            foreach (var target in targets)
            {
                TryFillPlayerClothesFromSolution(target, solution);
            }
        }

        #region Public
        public bool TryFillPlayerClothesFromSolution(
                Entity<HandsComponent?, InventoryComponent?> player,
                Solution origin,
                [NotNullWhen(true)] out List<Entity<FluidableClothingComponent, SolutionComponent>>? clothesList,
                FixedPoint2? amount = null,
                SlotFlags searchClothingFlags = SlotFlags.All,
                SlotFlags excludeClothingFlags = ExcludeSlotFlags
            )
        {
            clothesList = null;

            var clothes = _invSlotsBlock.GetAvailableWornClothes<FluidableClothingComponent>(player, searchClothingFlags, excludeClothingFlags);
            var clothesCount = clothes.Count();

            if (clothesCount == 0)
            {
                return false;
            }

            var maxFluidTake = (
                amount == null
                    ? origin.Volume
                    : Math.Min(origin.Volume.Float(), amount.Value.Float())
                    ) / clothesCount;

            clothesList = new();

            foreach (var cloth in clothes)
            {
                var clothEntity = cloth.Owner;
                var comp = cloth.Comp;

                if (!_solution.TryGetSolution(clothEntity, comp.Solution, out var clothSolutionEntity, out var clothSolution))
                    continue;

                if (clothSolutionEntity == null)
                    continue;

                clothesList.Add((cloth.Owner, cloth.Comp, clothSolutionEntity.Value.Comp));

                var fluidAmountTaken = maxFluidTake * comp.MaxFluidTakeAtPercentage;









                //ДОДЕЛАТЬ ЛОГИКУ ПЕРЕНОСА ЖИДКОСТИ












                if (clothSolution.Volume + fluidAmountTaken > clothSolution.MaxVolume)
                    continue;
                else
                {
                    _solution.TryTransferSolution(clothSolutionEntity.Value, origin, fluidAmountTaken);
                }
            }

            return clothesCount != 0;
        }

        public bool TryFillPlayerClothesFromSolution(
                Entity<HandsComponent?, InventoryComponent?> player,
                Solution origin,
                FixedPoint2? amount = null,
                SlotFlags searchClothingFlags = SlotFlags.All,
                SlotFlags excludeClothingFlags = ExcludeSlotFlags
            )
        {
            return TryFillPlayerClothesFromSolution(player, origin, out _, amount, searchClothingFlags, excludeClothingFlags);
        }

        public void SetAbsorbingEntityByPlayer(
                Entity<InventoryComponent?> player,
                Entity<SolutionComponent>? absorbingTarget,
                SlotFlags searchFlags = SlotFlags.FEET
            )
        {
            if (!_inventory.TryGetInventoryEntities(player, searchFlags, out var drainableEntities))
                return;

            foreach (var ent in drainableEntities)
            {
                if (!TryComp<FluidableClothingComponent>(ent, out var comp))
                    continue;

                comp.AbsorbingEntity = absorbingTarget;
            }
        }
        #endregion
    }
}
