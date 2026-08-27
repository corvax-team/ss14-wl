using Content.Server.Resist;
using Content.Shared.Humanoid;
using Content.Shared.Item;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.AddHeightItem;

public sealed partial class AddHeightItemSystem : EntitySystem
{
    [Dependency] private SharedItemSystem _item = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    private ProtoId<ItemSizePrototype> _ginormousSize = "Ginormous";
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AddHeightItemComponent, ComponentInit>(OnAddHeightItemInit);
    }
    /// <summary>
    /// Add item component depending on height
    /// </summary>
    private void OnAddHeightItemInit(EntityUid uid, AddHeightItemComponent com, ComponentInit args)
    {
        if (!TryComp<HumanoidProfileComponent>(uid, out var humanoid))
            return;

        if (!_proto.TryIndex(humanoid.Species, out var speciesProto))
            return;

        if (speciesProto.MaxItemHeight >= humanoid.Height)
        {
            var item = EnsureComp<ItemComponent>(uid);
            _item.SetSize(uid, _ginormousSize, item);

            EnsureComp<MultiHandedItemComponent>(uid);

            var escape = EnsureComp<CanEscapeInventoryComponent>(uid);
            escape.BaseResistTime = 1f;
        }
    }
}
