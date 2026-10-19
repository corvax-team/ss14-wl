using Content.IntegrationTests.Tests.Interaction;
using Content.Shared.Inventory;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests.Clothing;

public sealed class ClothingSlotBlockingBypassTest : InteractionTest
{
    protected override string PlayerPrototype => "MobHuman";

    [TestPrototypes]
    private const string Prototypes = @"
- type: entity
  id: SlotBlockingTestHelmet
  parent: ClothingHeadHelmetAtmosFire
  components:
  - type: ExtraBlockingInventorySlots
    slots: [FEET]
";

    [TestCase("ClothingShoesBootsMag")]
    [TestCase("ClothingShoesBootsMagAdv")]
    [TestCase("ClothingShoesBootsMagSci")]
    [TestCase("ClothingShoesBootsMagBlinding")]
    [TestCase("ClothingShoesBootsMagSyndie")]
    [TestCase("ClothingShoesBootsMagERT")]
    [TestCase("ClothingShoesBootsSpeed")]
    public async Task BootsCanBeEquippedAndRemovedOverHardsuit(string prototype)
    {
        await Server.WaitAssertion(() =>
        {
            var inventory = SEntMan.System<InventorySystem>();
            Assert.That(inventory.TryEquip(SPlayer, SpawnEquipment("ClothingOuterHardsuitEngineering"), "outerClothing", silent: true), Is.True);
            var boots = SpawnEquipment(prototype);
            Assert.That(inventory.TryEquip(SPlayer, boots, "shoes", silent: true), Is.True);
            Assert.That(inventory.TryUnequip(SPlayer, "shoes", silent: true), Is.True);
            Assert.That(inventory.TryEquip(SPlayer, boots, "head", silent: true), Is.False);
        });
        await RunTicks(5);
    }

    [Test]
    public async Task OrdinaryBootsRemainBlocked()
    {
        await Server.WaitAssertion(() =>
        {
            var inventory = SEntMan.System<InventorySystem>();
            var suit = SpawnEquipment("ClothingOuterHardsuitEngineering");
            var boots = SpawnEquipment("ClothingShoesBootsWork");
            Assert.That(inventory.TryEquip(SPlayer, suit, "outerClothing", silent: true), Is.True);
            Assert.That(inventory.TryEquip(SPlayer, boots, "shoes", silent: true), Is.False);
            Assert.That(inventory.TryUnequip(SPlayer, "outerClothing", silent: true), Is.True);
            Assert.That(inventory.TryEquip(SPlayer, boots, "shoes", silent: true), Is.True);
            Assert.That(inventory.TryEquip(SPlayer, suit, "outerClothing", silent: true), Is.True);
            Assert.That(inventory.TryUnequip(SPlayer, "shoes", silent: true), Is.False);
        });
        await RunTicks(5);
    }

    [Test]
    public async Task OtherBlockingSlotsStillApply()
    {
        await Server.WaitAssertion(() =>
        {
            var inventory = SEntMan.System<InventorySystem>();
            Assert.That(inventory.TryEquip(SPlayer, SpawnEquipment("ClothingOuterHardsuitEngineering"), "outerClothing", silent: true), Is.True);
            var helmet = SpawnEquipment("SlotBlockingTestHelmet");
            var boots = SpawnEquipment("ClothingShoesBootsMag");
            Assert.That(inventory.TryEquip(SPlayer, helmet, "head", silent: true), Is.True);
            Assert.That(inventory.TryEquip(SPlayer, boots, "shoes", silent: true), Is.False);
            Assert.That(inventory.TryUnequip(SPlayer, "head", silent: true), Is.True);
            Assert.That(inventory.TryEquip(SPlayer, boots, "shoes", silent: true), Is.True);
            Assert.That(inventory.TryEquip(SPlayer, helmet, "head", silent: true), Is.True);
            Assert.That(inventory.TryUnequip(SPlayer, "shoes", silent: true), Is.False);
        });
        await RunTicks(5);
    }

    private EntityUid SpawnEquipment(string prototype)
    {
        return SEntMan.SpawnEntity(prototype, SEntMan.GetComponent<TransformComponent>(SPlayer).Coordinates);
    }
}
