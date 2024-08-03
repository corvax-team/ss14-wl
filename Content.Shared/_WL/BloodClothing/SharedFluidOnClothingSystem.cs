using Content.Shared._WL.Inventory.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Fluids;
using Content.Shared.Hands.Components;
using Content.Shared.Inventory;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Player;
using System.Linq;

namespace Content.Shared._WL.BloodClothing
{
    public abstract partial class SharedFluidOnClothingSystem : EntitySystem
    {
        [Dependency] protected readonly SharedSolutionContainerMixerSystem _solutionMix = default!;
        [Dependency] protected readonly SharedSolutionContainerSystem _solution = default!;
        [Dependency] protected readonly InventorySystem _inventory = default!;
        [Dependency] protected readonly InventorySlotsBlockingSystem _invSlotsBlock = default!;

        public const SlotFlags ExcludeSlotFlags =
            SlotFlags.EYES |
            SlotFlags.MASK |
            SlotFlags.EARS |
            SlotFlags.NECK |
            SlotFlags.BELT |
            SlotFlags.BACK;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<SolutionContainerManagerComponent, MeleeHitEvent>(OnMeleeHit, [typeof(SharedPuddleSystem), typeof(OpenableSystem)]);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);


        }

        private void OnMeleeHit(EntityUid weapon, SolutionContainerManagerComponent weaponSolComp, MeleeHitEvent args)
        {
            if (args.Handled)
                return;

            var targets = args.HitEntities;

            if (!_solution.TryGetDrainableSolution(weapon, out var solutionEntity, out var solution))
                return;

            foreach (var target in targets)
            {
                var clothes = GetWornClothes(target, SlotFlags.All, ExcludeSlotFlags);
                var clothesCount = clothes.Count();

                if (clothesCount == 0)
                    continue;

                var maxFluidTake = solution.MaxVolume / clothesCount;

                foreach (var cloth in clothes)
                {
                    var clothEntity = cloth.Owner;
                    var comp = cloth.Comp;

                    if (!_solution.TryGetSolution(clothEntity, comp.Solution, out var clothSolutionEntity, out var clothSolution))
                        continue;

                    if (clothSolutionEntity == null)
                        continue;

                    var fluidAmountTaken = maxFluidTake * comp.MaxFluidTakeAtPercentage;

                    if (clothSolution.Volume + fluidAmountTaken > clothSolution.MaxVolume)
                        continue;
                    else
                    {
                        _solution.TryTransferSolution(clothSolutionEntity, solution, fluidAmountTaken);
                    }
                }
            }
        }

        #region Public
        public IEnumerable<Entity<FluidableClothingComponent>> GetWornClothes(
            Entity<HandsComponent?, InventoryComponent?> ent,
            SlotFlags searchFlags,
            SlotFlags excludeFlags)
        {
            var list = new List<Entity<FluidableClothingComponent>>();

            if (!Resolve(ent.Owner, ref ent.Comp2))
                return list;

            var blocked = _invSlotsBlock.GetBlockedClothes((ent.Owner, ent.Comp2));

            foreach (var e in _inventory.GetHandOrInventoryEntities(ent, searchFlags))
            {
                if (!TryComp<FluidableClothingComponent>(e, out var comp))
                    continue;

                if (blocked.TryGetValue(e, out var flags) || flags.HasFlag(excludeFlags))
                    continue;

                list.Add(new Entity<FluidableClothingComponent>(e, comp));
            }

            return list;
        }
        #endregion
    }
}
