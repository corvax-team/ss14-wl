using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Forensics;
using Content.Shared._WL.BloodClothing;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Inventory;

namespace Content.Server._WL.BloodClothing
{
    public sealed partial class FluidOnClothingSystem : SharedFluidOnClothingSystem
    {
        [Dependency] private readonly ForensicsSystem _forensics = default!;
        [Dependency] private readonly PuddleSystem _puddle = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<InventoryComponent, DamageChangedEvent>(OnDamageChanged, after: [typeof(BloodstreamSystem)]);
            SubscribeLocalEvent<FluidableClothingComponent, SolutionContainerOverflowEvent>(OnClothOverflow);
        }

        private void OnDamageChanged(EntityUid ent, InventoryComponent comp, DamageChangedEvent args)
        {
            if (!args.DamageIncreased)
                return;

            if (args.Origin == null)
                return;

            if (!TryComp<BloodstreamComponent>(ent, out var bloodstreamComp))
                return;

            if (!_solution.TryGetSolution(ent, bloodstreamComp.BloodSolutionName, out _, out var bloodSolution))
                return;

            if (bloodstreamComp.MaxBleedAmount == 0)
                return;

            // Чтоб... не было проблем с остатком float, вех. На всякий короче.
            if (MathHelper.CloseTo(bloodSolution.Volume.Float(), 0f))
                return;

            var origin = args.Origin.Value;

            //TODO: ту... ду... рассчет нормальный интенсивности
            var bleedIntensity = bloodstreamComp.BleedAmount / bloodstreamComp.MaxBleedAmount;
            var bloodFillFraction = bloodSolution.FillFraction;

            var bloodTakeAmount = bleedIntensity * bloodstreamComp.BleedAmount * bloodFillFraction;

            if (!TryFillPlayerClothesFromSolution(origin, bloodSolution, out var clothes, bloodTakeAmount))
                return;

            foreach (var cloth in clothes)
            {
                _forensics.TransferDna(cloth.Owner, ent);
            }
        }

        private void OnClothOverflow(EntityUid ent, FluidableClothingComponent comp, ref SolutionContainerOverflowEvent args)
        {
            if (args.Handled)
                return;

            _puddle.TrySpillAt(Transform(ent).Coordinates, args.Overflow, out var puddle);
            _forensics.TransferDna(puddle, ent, false);

            args.Handled = true;
        }
    }
}
