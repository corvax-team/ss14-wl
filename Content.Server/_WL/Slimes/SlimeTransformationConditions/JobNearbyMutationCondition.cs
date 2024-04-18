using Content.Server.Mind;
using Content.Server.Roles.Jobs;
using Content.Shared._WL.Slimes;
using Content.Shared.FixedPoint;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;
using System.Linq;

namespace Content.Server._WL.Slimes.SlimeTransformationConditions;

public sealed partial class JobNearbyMutationCondition : SlimeTransformationCondition
{
    [DataField(customTypeSerializer: typeof(PrototypeIdListSerializer<JobPrototype>))]
    public List<string> Whitelist = new();

    [DataField("requiredAll")]
    public bool WhitelistRequiredAll = true;

    [DataField(customTypeSerializer: typeof(PrototypeIdListSerializer<JobPrototype>))]
    public List<string> Blacklist = new();

    [DataField]
    public FixedPoint2 Radius = 1f;

    public override bool Condition(SlimeTransformationConditionArgs args)
    {
        var entityManager = args.EntityManager;
        var jobSystem = entityManager.System<JobSystem>();
        var mindSystem = entityManager.System<MindSystem>();
        var entityLookup = entityManager.System<EntityLookupSystem>();

        var entities = entityLookup.GetEntitiesInRange(args.Slime, Radius.Float(), LookupFlags.Dynamic);

        var workers = 0;
        var successes = 0;
        foreach (var entity in entities)
        {
            if (!mindSystem.TryGetMind(entity, out var mindId, out _))
                continue;

            if (!jobSystem.MindTryGetJob(mindId, out _, out var jobProto))
                continue;

            workers++;

            if (Blacklist.Count > 0)
                if (Blacklist.Contains(jobProto.ID))
                    continue;

            if (Whitelist.Count > 0)
                if (!Whitelist.Contains(jobProto.ID))
                    continue;

            successes++;
        }

        return workers != 0 && (WhitelistRequiredAll
                ? workers == successes
                : successes > 0);
    }

    public override SlimeTransformationCondition GetRandomCondition(IEntityManager entMan, IPrototypeManager protoMan, IRobustRandom random)
    {
        var jobs = protoMan.EnumeratePrototypes<JobPrototype>()
            .ToList();
        var toReturn = new List<string>();

        var pickTimes = random.Next(1, 4);
        for (var i = 0; i < pickTimes; i++)
        {
            toReturn.Add(random.PickAndTake(jobs).ID);
        }


        return new JobNearbyMutationCondition()
        {
            Whitelist = toReturn,
            WhitelistRequiredAll = random.Prob(0.5f),
            Radius = random.NextFloat(1, 6)
        };
    }

    public override string GetDescriptionString(IEntityManager entityManager, IPrototypeManager protoMan)
            => Loc.GetString("slime-transformation-condition-job-nearby",
                ("radius", Radius),
                ("all", WhitelistRequiredAll ? 1 : 0),
                ("white", Whitelist.Count == 0 ? 0 : string.Join(", ", Whitelist.Select(x => protoMan.Index<JobPrototype>(x).LocalizedName))),
                ("black", Blacklist.Count == 0 ? 0 : string.Join(", ", Blacklist.Select(x => protoMan.Index<JobPrototype>(x).LocalizedName))));
}
