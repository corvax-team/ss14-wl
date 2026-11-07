using Content.Client.Actions;
using Content.IntegrationTests.Tests.Interaction;
using Content.Shared._WL.Body;
using Content.Shared.Actions.Components;
using Content.Shared.Body;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Inventory;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests.Humanoid;

public sealed class HideUnderClothingTest : InteractionTest
{
    protected override string PlayerPrototype => "MobVulpkanin";

    [Test]
    public async Task DefaultActionFollowsClothingAndMarkingChanges()
    {
        await RunTicks(5);
        await Server.WaitAssertion(() =>
        {
            var comp = SEntMan.GetComponent<HideUnderClothingComponent>(SPlayer);
            Assert.That(comp.Action, Is.Not.Null);
            Assert.That(SEntMan.GetComponent<ActionComponent>(comp.Action!.Value).Enabled, Is.False);
            SetMarkings(true);
            Equip("ClothingUniformJumpsuitColorGrey", "jumpsuit");
            Toggle();
        });
        await CheckVisibility(true, false);
        await Server.WaitAssertion(() => Equip("ClothingHeadHatTophat", "head"));
        await CheckVisibility(false, false);
        await Server.WaitAssertion(() => SetMarkings(true));
        await CheckVisibility(false, false);
        await Server.WaitAssertion(() => SEntMan.System<InventorySystem>().TryUnequip(SPlayer, "head", force: true));
        await CheckVisibility(true, false);
        await Server.WaitAssertion(() => SEntMan.System<InventorySystem>().TryUnequip(SPlayer, "jumpsuit", force: true));
        await CheckVisibility(true, true);
        await Server.WaitAssertion(() => { Equip("ClothingHeadHatTophat", "head"); Toggle(); });
        await CheckVisibility(false, true);
        await Server.WaitAssertion(() => SetMarkings(false));
        await RunTicks(5);
        await Server.WaitAssertion(() =>
        {
            var comp = SEntMan.GetComponent<HideUnderClothingComponent>(SPlayer);
            Assert.That(comp.Action, Is.Null);
            Assert.That(comp.Hidden, Is.False);
        });
    }

    [Test]
    public async Task ClientPredictsVisibilityBeforeServerConfirms()
    {
        await Server.WaitAssertion(() =>
        {
            SetMarkings(true);
            Equip("ClothingUniformJumpsuitColorGrey", "jumpsuit");
            Equip("ClothingHeadHatTophat", "head");
        });
        await RunTicks(5);
        await Client.WaitAssertion(() =>
        {
            var comp = CEntMan.GetComponent<HideUnderClothingComponent>(CPlayer);
            Assert.That(comp.Hidden, Is.False);
            Assert.That(comp.Action, Is.Not.Null);
            var action = comp.Action!.Value;
            CEntMan.System<ActionsSystem>().TriggerAction((action, CEntMan.GetComponent<ActionComponent>(action)));
            Assert.That(comp.Hidden, Is.True);
            Assert.That(CEntMan.GetComponent<ActionComponent>(action).Toggled, Is.True);
            AssertVisibility(false, false);
        });
        await Server.WaitAssertion(() =>
            Assert.That(SEntMan.GetComponent<HideUnderClothingComponent>(SPlayer).Hidden, Is.False));
        await RunTicks(10);
        await Server.WaitAssertion(() =>
            Assert.That(SEntMan.GetComponent<HideUnderClothingComponent>(SPlayer).Hidden, Is.True));
        await CheckVisibility(false, false);
    }

    private void SetMarkings(bool eligible) =>
        SEntMan.System<SharedVisualBodySystem>().ApplyMarkings(SPlayer, new()
        {
            ["Head"] = new() { [HumanoidVisualLayers.HeadTop] = eligible
                ? [new("FoxEar", 1), new("LizardHornsCurled", 1)] : [new("LizardHornsCurled", 1)] },
            ["Torso"] = new() { [HumanoidVisualLayers.Tail] = eligible ? [new("FoxTail", 1)] : [] },
        });

    private void Equip(string prototype, string slot)
    {
        var item = SEntMan.SpawnEntity(prototype, SEntMan.GetComponent<TransformComponent>(SPlayer).Coordinates);
        Assert.That(SEntMan.System<InventorySystem>().TryEquip(SPlayer, item, slot, force: true), Is.True);
    }

    private void Toggle()
    {
        var ev = new ToggleMarkingsUnderClothingEvent();
        SEntMan.EventBus.RaiseLocalEvent(SPlayer, ev);
        Assert.That(ev.Handled, Is.True);
    }

    private async Task CheckVisibility(bool earsVisible, bool tailVisible)
    {
        await RunTicks(5);
        await Client.WaitAssertion(() => AssertVisibility(earsVisible, tailVisible));
    }

    private void AssertVisibility(bool earsVisible, bool tailVisible)
    {
        var sprites = CEntMan.System<SpriteSystem>();
        var sprite = CEntMan.GetComponent<SpriteComponent>(CPlayer);
        var prototypes = Client.Resolve<IPrototypeManager>();
        foreach (var (id, visible) in new[] { ("FoxEar", earsVisible), ("FoxTail", tailVisible), ("LizardHornsCurled", true) })
        foreach (var layer in prototypes.Index<MarkingPrototype>(id).Sprites)
            Assert.That(sprite[sprites.LayerMapGet(CPlayer, layer.GetLayerID(id))].Visible, Is.EqualTo(visible), id);
    }
}
