using System.Linq;
using Content.Client._WL.Body;
using Content.IntegrationTests.Tests.Interaction;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Inventory;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.GameObjects;
using Robust.Shared.Maths;

namespace Content.IntegrationTests.Tests.Body;

[TestFixture(HumanPrototype)]
[TestFixture(ReptilianPrototype)]
public sealed class DirectionalBodyLayersTest(string playerPrototype) : InteractionTest
{
    private const string HumanPrototype = "MobHuman";
    private const string ReptilianPrototype = "MobReptilian";
    private const string SuitPrototype = "ClothingOuterHardsuitEngineering";
    private const string LeftLegCategory = "LegLeft";
    private const string RightLegCategory = "LegRight";
    private const string LeftLegMarking = "TattooSilverburghLeftLeg";
    private const string RightLegMarking = "TattooSilverburghRightLeg";
    private const string LeftMarkingLayer = LeftLegMarking + "-tattoo_silverburgh_l_leg";
    private const string RightMarkingLayer = RightLegMarking + "-tattoo_silverburgh_r_leg";

    protected override string PlayerPrototype => playerPrototype;

    [TestCase(Direction.South, false)]
    [TestCase(Direction.North, true)]
    [TestCase(Direction.East, true)]
    [TestCase(Direction.West, false)]
    public async Task MarkingsFollowLimbsAndPreserveClothing(Direction direction, bool rightInFront)
    {
        await Server.WaitAssertion(() =>
        {
            SEntMan.System<Content.Server.Body.VisualBodySystem>().ApplyMarkings(SPlayer, new()
            {
                [LeftLegCategory] = new() { [HumanoidVisualLayers.LLeg] = [new Marking(LeftLegMarking, [Color.Red])] },
                [RightLegCategory] = new() { [HumanoidVisualLayers.RLeg] = [new Marking(RightLegMarking, [Color.Blue])] },
            });
            var suit = SEntMan.SpawnEntity(SuitPrototype, SEntMan.GetComponent<TransformComponent>(SPlayer).Coordinates);
            Assert.That(SEntMan.System<InventorySystem>().TryEquip(SPlayer, suit, "outerClothing", force: true), Is.True);
        });
        await RunTicks(5);
        await Client.WaitAssertion(() =>
        {
            var sprites = CEntMan.System<SpriteSystem>();
            var sprite = CEntMan.GetComponent<SpriteComponent>(CPlayer);
            var clothingIndex = sprites.LayerMapGet(CPlayer, "outerClothing");
            var clothing = sprite.AllLayers.ElementAt(clothingIndex);
            var marking = sprite.AllLayers.ElementAt(sprites.LayerMapGet(CPlayer, RightMarkingLayer));
            var state = (marking.Visible, marking.Color);

            CEntMan.System<DirectionalBodyLayersSystem>().ApplyDirection(CPlayer, direction);
            Assert.That(new[]
            {
                sprites.LayerMapGet(CPlayer, rightInFront ? HumanoidVisualLayers.LLeg : HumanoidVisualLayers.RLeg),
                sprites.LayerMapGet(CPlayer, rightInFront ? LeftMarkingLayer : RightMarkingLayer),
                sprites.LayerMapGet(CPlayer, rightInFront ? HumanoidVisualLayers.RLeg : HumanoidVisualLayers.LLeg),
                sprites.LayerMapGet(CPlayer, rightInFront ? RightMarkingLayer : LeftMarkingLayer),
            }, Is.Ordered.Ascending);
            Assert.That(sprites.LayerMapGet(CPlayer, "outerClothing"), Is.EqualTo(clothingIndex));
            Assert.That(sprite.AllLayers.ElementAt(clothingIndex), Is.SameAs(clothing));
            var moved = sprite.AllLayers.ElementAt(sprites.LayerMapGet(CPlayer, RightMarkingLayer));
            Assert.That(moved, Is.SameAs(marking));
            Assert.That((moved.Visible, moved.Color), Is.EqualTo(state));
        });
    }

    [Test]
    public async Task FrameOrderFollowsRenderedRotationAndCameraWithoutSimulationTicks()
    {
        await Client.WaitAssertion(() =>
        {
            var system = CEntMan.System<DirectionalBodyLayersSystem>();
            var sprites = CEntMan.System<SpriteSystem>();
            var transform = CEntMan.System<TransformSystem>();
            var eye = Client.Resolve<IEyeManager>().CurrentEye;
            var originalRotation = eye.Rotation;
            try
            {
                transform.SetWorldRotation(CPlayer, Angle.FromDegrees(90));
                transform.SetRenderRotationOverride(CPlayer, Angle.Zero);
                eye.Rotation = Angle.Zero;
                AssertOrder(false);
                eye.Rotation = Angle.FromDegrees(90);
                AssertOrder(true);
                eye.Rotation = Angle.Zero;
                transform.SetRenderRotationOverride(CPlayer, Angle.FromDegrees(90));
                AssertOrder(true);
            }
            finally
            {
                transform.ClearRenderRotationOverride(CPlayer);
                eye.Rotation = originalRotation;
            }

            void AssertOrder(bool rightInFront)
            {
                system.FrameUpdate(0);
                Assert.That(sprites.LayerMapGet(CPlayer, HumanoidVisualLayers.RLeg) >
                            sprites.LayerMapGet(CPlayer, HumanoidVisualLayers.LLeg), Is.EqualTo(rightInFront));
            }
        });
    }
}
