using Content.Server._WL.Nutrition.Components;
using Content.Server._WL.Nutrition.Systems;
using Content.Shared.Prototypes;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests.WLTests.Nutrition;

[TestFixture]
[TestOf(typeof(SuckableFoodSystem))]
public sealed class SuckableFoodTest
{
    [Test]
    public async Task TestEquippedEntityShouldNotHaveSuckableFoodComponent()
    {
        await using var pair = await PoolManager.GetServerClient();

        var server = pair.Server;

        var protoManager = server.ProtoMan;
        var componentFactory = server.ResolveDependency<IComponentFactory>();

        Assert.Multiple(() =>
        {
            foreach (var proto in protoManager.EnumeratePrototypes<EntityPrototype>())
            {
                var compName = componentFactory.GetComponentName<SuckableFoodComponent>();
                if (!proto.Components.TryGetValue(compName, out var suckableCompUncasted) || suckableCompUncasted.Component is not SuckableFoodComponent suckableComp)
                    continue;

                if (suckableComp.EquippedEntityOnDissolve == null)
                    continue;

                var equippedEnt = suckableComp.EquippedEntityOnDissolve.Value;

                var equippedEntityProto = protoManager.Index<EntityPrototype>(equippedEnt);

                var equippedEntityHasSuckableComponent = equippedEntityProto.HasComponent<SuckableFoodComponent>(componentFactory);

                var msg = $"Поле {nameof(SuckableFoodComponent)}.{nameof(SuckableFoodComponent.EquippedEntityOnDissolve)} прототипа {proto.ID} не должно ссылаться на сущность ({equippedEnt}), имеющую {nameof(SuckableFoodComponent)} в своих компонентах!";

                Assert.That(equippedEntityHasSuckableComponent, Is.False, msg);
            }
        });

        await pair.CleanReturnAsync();
    }
}
