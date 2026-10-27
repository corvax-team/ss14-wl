using Content.IntegrationTests.Tests.Interaction;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Inventory;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests.Clothing;

public sealed class ClothingItemMapperTest : InteractionTest
{
    protected override string PlayerPrototype => "MobHuman";

    private const string SheathPrototype = "ClothingBeltSheath";
    private const string SabrePrototype = "CaptainSabre";
    private const string OverlayKey = "clothing-item-mapper-belt-sheath-sabre-0";

    [Test]
    public async Task WornOverlayTracksContents()
    {
        EntityUid sheath = default;
        await Server.WaitAssertion(() =>
        {
            var coordinates = SEntMan.GetComponent<TransformComponent>(SPlayer).Coordinates;
            sheath = SEntMan.SpawnEntity(SheathPrototype, coordinates);
            Assert.That(SEntMan.System<InventorySystem>().TryEquip(SPlayer, sheath, "belt", force: true), Is.True);
        });
        await AssertOverlay(false);

        await Server.WaitAssertion(() =>
        {
            var sabre = SEntMan.SpawnEntity(SabrePrototype, SEntMan.GetComponent<TransformComponent>(SPlayer).Coordinates);
            Assert.That(SEntMan.System<ItemSlotsSystem>().TryInsert((sheath, null), "item", sabre, null), Is.True);
        });
        await AssertOverlay(true);

        await Server.WaitAssertion(() =>
        {
            Assert.That(SEntMan.System<ItemSlotsSystem>().TryEject((sheath, null), "item", null, out _), Is.True);
        });
        await AssertOverlay(false);
    }

    private async Task AssertOverlay(bool expected)
    {
        await RunTicks(5);
        await Client.WaitAssertion(() =>
        {
            var sprites = CEntMan.System<SpriteSystem>();
            Assert.That(sprites.LayerMapTryGet(CPlayer, OverlayKey, out _, false), Is.EqualTo(expected));
            Assert.That(sprites.LayerMapTryGet(CPlayer, "belt-0", out _, false), Is.True);
        });
    }
}
