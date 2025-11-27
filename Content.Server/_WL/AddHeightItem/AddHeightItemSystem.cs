using Content.Server.Resist;
using Content.Shared.Humanoid;
using Content.Shared.Item;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.AddHeightItem
{
    public sealed partial class AddHeightItemSystem : EntitySystem
    {
        [Dependency] private readonly SharedItemSystem _item = default!;
        [Dependency] private readonly IPrototypeManager _proto = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AddHeightItemComponent, ComponentInit>(OnADHI);
        }
        /// <summary>
        /// Add item component depending on height
        /// </summary>
        private void OnADHI(EntityUid uid, AddHeightItemComponent com, ComponentInit args)
        {
            if (!TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
                return;

            if (!_proto.TryIndex(humanoid.Species, out var speciesProto))
                return;

            if (speciesProto.MaxItemHeight >= humanoid.Height)
            {
                var size1 = "Ginormous";

                var item = EnsureComp<ItemComponent>(uid);
                _item.SetSize(uid, size1, item);

                EnsureComp<MultiHandedItemComponent>(uid);

                var escape = EnsureComp<CanEscapeInventoryComponent>(uid);
                escape.BaseResistTime = 1f;
            }
        }
    }
}
