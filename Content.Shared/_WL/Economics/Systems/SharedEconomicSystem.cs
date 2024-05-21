using Content.Shared._WL.Economics.Components;

namespace Content.Shared._WL.Economics
{
    public abstract partial class SharedEconomicSystem : EntitySystem
    {
        [Dependency] protected readonly SharedTransformSystem _transform = default!;

        public override void Initialize()
        {
            base.Initialize();

            InitializeBankAccounts();
        }

        public (EntityUid User, EconomicsUserComponent EconomicUserComponent)? GetPlayer(EntityUid target, int naxDepth = 4)
        {
            var entity = target;

            for (var i = 1; i <= naxDepth; i++)
            {
                if (TryComp<EconomicsUserComponent>(entity, out var comp))
                    return (entity, comp);

                entity = _transform.GetParentUid(entity);
            }

            return null;
        }

        public void AddPenalty(EntityUid? holder, EconomicPenalty penalty)
        {
            if (holder == null)
                return;

            if (TryComp<EconomicsUserComponent>(holder, out var economicUserComp))
            {

            }
        }
    }
}
