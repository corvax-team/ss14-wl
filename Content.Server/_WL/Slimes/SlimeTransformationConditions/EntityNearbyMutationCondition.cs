using Content.Shared._WL.Slimes;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;
using System.Linq;

namespace Content.Server._WL.Slimes.SlimeTransformationConditions;

public sealed partial class EntityNearbyMutationCondition : SlimeTransformationCondition
{
    [DataField("whitelist", customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
    public List<string> EntityWhitelist = new();

    [DataField("blacklist", customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
    public List<string> EntityBlackList = new();

    [DataField("radius")]
    public FixedPoint2 Radius = 1;

    public override bool Condition(SlimeTransformationConditionArgs args)
    {
        var EntityManager = args.EntityManager;
        var _lookup = EntityManager.System<EntityLookupSystem>();
        var protoMan = IoCManager.Resolve<IPrototypeManager>();

        if (!EntityManager.TryGetComponent<TransformComponent>(args.Slime, out var transformComp))
            return false;

        return _lookup.GetEntitiesInRange(transformComp.Coordinates, Radius.Float())
            .Any(ent =>
            {
                if (!EntityManager.TryGetComponent<MetaDataComponent>(ent, out var metaData) || metaData.EntityPrototype == null)
                    return false;

                if (EntityBlackList.Contains(metaData.EntityPrototype.ID) || !EntityWhitelist.Contains(metaData.EntityPrototype.ID))
                    return false;

                return true;
            });
    }

    public override SlimeTransformationCondition GetRandomCondition(IEntityManager entMan, IPrototypeManager protoMan, IRobustRandom random)
    {
        var whitelist = protoMan.EnumeratePrototypes<EntityPrototype>()
            .Where(proto => !proto.HideSpawnMenu && !proto.Abstract && !proto.NoSpawn
                && proto.Components.Values.Any(comp => comp.Component is MobStateComponent))
            .ToList();

        var pickTimes = random.Next(1, 5);
        var toReturn = new List<string>();
        for (var i = 1; i < pickTimes; i++)
        {
            toReturn.Add(random.PickAndTake(whitelist).ID);
        }

        return new EntityNearbyMutationCondition()
        {
            EntityWhitelist = toReturn,
            Radius = random.NextFloat(0.5f, 6f)
        };
    }

    public override string GetDescriptionString(IEntityManager entityManager, IPrototypeManager protoMan)
            => Loc.GetString("slime-transformation-condition-entity-nearby",
                ("radius", Radius),
                ("white", EntityWhitelist.Count == 0 ? 0 : string.Join(", ", EntityWhitelist.Select(x => protoMan.Index<EntityPrototype>(x).Name)).Trim()),
                ("black", EntityBlackList.Count == 0 ? 0 : string.Join(", ", EntityBlackList.Select(x => protoMan.Index<EntityPrototype>(x).Name)).Trim()));
}


