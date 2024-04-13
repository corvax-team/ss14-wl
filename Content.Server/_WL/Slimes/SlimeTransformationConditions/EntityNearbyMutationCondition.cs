using Content.Shared._WL.Slimes;
using Robust.Shared.Prototypes;
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
    public float Radius = 1;

    public override bool Condition(SlimeTransformationConditionArgs args)
    {
        var EntityManager = args.EntityManager;
        var _lookup = EntityManager.System<EntityLookupSystem>();
        var protoMan = IoCManager.Resolve<IPrototypeManager>();

        if (!EntityManager.TryGetComponent<TransformComponent>(args.Slime, out var transformComp))
            return false;

        return _lookup.GetEntitiesInRange(transformComp.Coordinates, Radius)
            .Any(ent =>
            {
                if (!EntityManager.TryGetComponent<MetaDataComponent>(ent, out var metaData) || metaData.EntityPrototype == null)
                    return false;

                if (EntityBlackList.Contains(metaData.EntityPrototype.ID) || !EntityWhitelist.Contains(metaData.EntityPrototype.ID))
                    return false;

                return true;
            });
    }

    public override string GetDescriptionString(IEntityManager entityManager, IPrototypeManager protoMan)
            => Loc.GetString("slime-transformation-condition-entity-nearby",
                ("radius", Radius),
                ("white", EntityWhitelist.Count == 0 ? "0" : string.Join(", ", EntityWhitelist.Select(x => protoMan.Index<EntityPrototype>(x).Name)).Trim()),
                ("black", EntityBlackList.Count == 0 ? "0" : string.Join(", ", EntityBlackList.Select(x => protoMan.Index<EntityPrototype>(x).Name)).Trim()));
}


