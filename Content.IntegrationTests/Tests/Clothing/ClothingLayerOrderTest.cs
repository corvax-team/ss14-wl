using System.Linq;
using Content.Client.Inventory;
using Content.IntegrationTests.Tests.Interaction;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests.Clothing;

[TestFixture("MobHuman")]
[TestFixture("MobReptilian")]
public sealed class ClothingLayerOrderTest(string playerPrototype) : InteractionTest
{
    protected override string PlayerPrototype => playerPrototype;

    [TestPrototypes]
    private const string Prototypes = @"
- type: entity
  id: LayerOrderTestBoots
  parent: ClothingShoesBootsMag
  components:
  - type: ClothingLayerOrder
    above: [head, outerClothing, missing-slot, shoes]

- type: entity
  id: LayerOrderChainBoots
  parent: ClothingShoesBootsMag
  components:
  - type: ClothingLayerOrder
    above: [gloves]

- type: entity
  id: LayerOrderChainGloves
  parent: ClothingHandsGlovesColorBlack
  components:
  - type: ClothingLayerOrder
    above: [head]

- type: entity
  id: LayerOrderCycleGloves
  parent: ClothingHandsGlovesColorBlack
  components:
  - type: ClothingLayerOrder
    above: [shoes]
";

    [Test]
    public async Task MultipleTargetsAndMissingSlots()
    {
        await Equip("LayerOrderTestBoots", "shoes");
        await Equip("ClothingHeadHelmetAtmosFire", "head");
        await Equip("ClothingOuterHardsuitEngineering", "outerClothing");
        await AssertAbove("shoes", "head");
        await AssertAbove("shoes", "outerClothing");
    }

    [Test]
    public async Task DependenciesSurviveTargetRefresh()
    {
        await Equip("LayerOrderChainBoots", "shoes");
        var gloves = await Equip("LayerOrderChainGloves", "gloves");
        await Equip("ClothingHeadHelmetAtmosFire", "head");
        await AssertAbove("gloves", "head");
        await AssertAbove("shoes", "gloves");

        await Client.WaitPost(() =>
            CEntMan.System<SharedItemSystem>().VisualsChanged(CEntMan.GetEntity(gloves)));
        await AssertAbove("gloves", "head");
        await AssertAbove("shoes", "gloves");

        await Server.WaitAssertion(() =>
            Assert.That(SEntMan.System<InventorySystem>().TryUnequip(SPlayer, "head", force: true), Is.True));
        await Equip("ClothingHeadHelmetAtmosFire", "head");
        await AssertAbove("gloves", "head");
        await AssertAbove("shoes", "gloves");
    }

    [Test]
    public async Task CyclicConstraintsUseNormalSlotOrder()
    {
        await Equip("LayerOrderChainBoots", "shoes");
        await Equip("LayerOrderCycleGloves", "gloves");
        await AssertAbove("shoes", "gloves");
    }

    private async Task<NetEntity> Equip(string prototype, string slot)
    {
        NetEntity entity = default;
        await Server.WaitAssertion(() =>
        {
            var item = SEntMan.SpawnEntity(prototype, SEntMan.GetComponent<TransformComponent>(SPlayer).Coordinates);
            Assert.That(SEntMan.System<InventorySystem>().TryEquip(SPlayer, item, slot, force: true), Is.True);
            entity = SEntMan.GetNetEntity(item);
        });
        await RunTicks(5);
        return entity;
    }

    private async Task AssertAbove(string upperSlot, string lowerSlot)
    {
        await RunTicks(5);
        await Client.WaitAssertion(() =>
        {
            var slots = CEntMan.GetComponent<InventorySlotsComponent>(CPlayer);
            var sprites = CEntMan.System<SpriteSystem>();
            var upperLayers = slots.VisualLayerKeys[upperSlot];
            var lowerLayers = slots.VisualLayerKeys[lowerSlot];
            Assert.That(upperLayers, Is.Not.Empty);
            Assert.That(lowerLayers, Is.Not.Empty);
            Assert.That(upperLayers.Min(key => sprites.LayerMapGet(CPlayer, key)),
                Is.GreaterThan(lowerLayers.Max(key => sprites.LayerMapGet(CPlayer, key))));
        });
    }

    [TestCase("ClothingShoesBootsMag", true)]
    [TestCase("ClothingShoesBootsMag", false)]
    [TestCase("ClothingShoesBootsSpeed", true)]
    [TestCase("ClothingShoesBootsSpeed", false)]
    public async Task BootsStayAboveOuterClothing(string bootsPrototype, bool bootsFirst)
    {
        EntityUid boots = default;
        await Server.WaitAssertion(() =>
        {
            boots = SEntMan.SpawnEntity(bootsPrototype, SEntMan.GetComponent<TransformComponent>(SPlayer).Coordinates);
            if (bootsPrototype == "ClothingShoesBootsSpeed")
            {
                var cell = SEntMan.SpawnEntity("PowerCellSmall", SEntMan.GetComponent<TransformComponent>(SPlayer).Coordinates);
                Assert.That(SEntMan.System<ItemSlotsSystem>().TryInsert(boots, "cell_slot", cell, SPlayer), Is.True);
            }
        });

        if (bootsFirst)
            await EquipBoots();
        await EquipSuit("ClothingOuterHardsuitEngineering");
        if (!bootsFirst)
            await EquipBoots();
        await AssertLayerOrder();

        await Server.WaitAssertion(() => Assert.That(ItemToggleSys.Toggle(boots, SPlayer), Is.True));
        await AssertLayerOrder();

        await Server.WaitAssertion(() =>
        {
            Assert.That(SEntMan.System<InventorySystem>().TryUnequip(SPlayer, "outerClothing", force: true), Is.True);
        });
        await EquipSuit("ClothingOuterHardsuitAtmos");
        await AssertLayerOrder();

        await Server.WaitAssertion(() => Assert.That(ItemToggleSys.Toggle(boots, SPlayer), Is.True));
        await AssertLayerOrder();

        async Task EquipBoots()
        {
            await Server.WaitAssertion(() =>
                Assert.That(SEntMan.System<InventorySystem>().TryEquip(SPlayer, boots, "shoes", force: true), Is.True));
            await RunTicks(5);
        }

        async Task EquipSuit(string prototype)
        {
            await Server.WaitAssertion(() =>
            {
                var suit = SEntMan.SpawnEntity(prototype, SEntMan.GetComponent<TransformComponent>(SPlayer).Coordinates);
                Assert.That(SEntMan.System<InventorySystem>().TryEquip(SPlayer, suit, "outerClothing", force: true), Is.True);
            });
            await RunTicks(5);
        }

        async Task AssertLayerOrder()
        {
            await RunTicks(5);
            await Client.WaitAssertion(() =>
            {
                var slots = CEntMan.GetComponent<InventorySlotsComponent>(CPlayer);
                var sprites = CEntMan.System<SpriteSystem>();
                var bootsLayers = slots.VisualLayerKeys["shoes"];
                var suitLayers = slots.VisualLayerKeys["outerClothing"];
                Assert.That(bootsLayers, Is.Not.Empty);
                Assert.That(suitLayers, Is.Not.Empty);
                Assert.That(bootsLayers.Min(key => sprites.LayerMapGet(CPlayer, key)),
                    Is.GreaterThan(suitLayers.Max(key => sprites.LayerMapGet(CPlayer, key))));
            });
        }
    }
}
