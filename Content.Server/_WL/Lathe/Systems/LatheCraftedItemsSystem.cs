using Content.Server._WL.Lathe.Components;
using Content.Shared._WL.Materials.Events;
using Content.Shared.Lathe;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Lathe.Systems;
public sealed class LatheCraftedItemsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CraftedOnLatheComponent, BeforeItemMaterialReclaimedEvent>(OnCraftedItemReclaim);
        SubscribeLocalEvent<CraftedOnLatheComponent, StackSplitEvent>(OnStackSplit);
        SubscribeLocalEvent<LatheGetResultEvent>(OnLatheCraft);
    }

    private void OnCraftedItemReclaim(EntityUid item, CraftedOnLatheComponent comp, BeforeItemMaterialReclaimedEvent ev)
    {
        var material = ev.Material;
        if (material == null)
            return;

        if (!comp.ConsumedMaterials.TryGetValue(material, out var consumedAmount))
        {
            ev.Amount = 0;
            return;
        }

        ev.Amount = Math.Clamp(ev.Amount, 0, consumedAmount);
    }

    private void OnStackSplit(EntityUid item, CraftedOnLatheComponent itemComp, StackSplitEvent ev)
    {
        var newItem = ev.NewId;
        var newItemComp = EnsureComp<CraftedOnLatheComponent>(newItem);
        newItemComp.ConsumedMaterials = new(itemComp.ConsumedMaterials);
    }

    private void OnLatheCraft(LatheGetResultEvent ev)
    {
        if (!_protoMan.TryIndex(ev.Recipe, out var recipe))
            return;

        var latheComp = ev.Lathe.Comp;

        var comp = EnsureComp<CraftedOnLatheComponent>(ev.ResultItem);

        var consumedMaterials = new Dictionary<string, int>();
        foreach (var (material, amount) in recipe.Materials)
        {
            var consumeAmount = recipe.ApplyMaterialDiscount ? (int)(amount * latheComp.MaterialUseMultiplier) : amount;
            consumedMaterials[material] = consumeAmount;
        }

        comp.ConsumedMaterials = consumedMaterials;
    }
}
