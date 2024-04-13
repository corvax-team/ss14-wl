using Content.Shared.Chemistry.Reagent;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._WL.Chemistry.ReagentEffects
{
    public sealed partial class ChangeFaction : ReagentEffect
    {
        [DataField("faction", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<NpcFactionPrototype>))]
        public string Faction;

        [DataField("method")]
        public ChangeFactionMethod Method = ChangeFactionMethod.Add;

        /// <summary>
        /// If has value, then the fractions will change for creatures within a given radius.
        /// </summary>
        [DataField("radius")]
        public float? Radius = null;

        [DataField("prob")]
        public float ChangeFactionProbability = 1f;

        public override void Effect(ReagentEffectArgs args)
        {
            var entityManager = args.EntityManager;
            var _npcFaction = entityManager.System<NpcFactionSystem>();
            var _lookup = entityManager.System<EntityLookupSystem>();
            var _random = IoCManager.Resolve<IRobustRandom>();

            var entities = Radius == null
                ? new HashSet<EntityUid>([args.SolutionEntity])
                : _lookup.GetEntitiesInRange(args.SolutionEntity, Radius.Value, LookupFlags.Dynamic);

            foreach (var entity in entities)
            {
                if (!_random.Prob(ChangeFactionProbability))
                    continue;

                if (!entityManager.HasComponent<NpcFactionMemberComponent>(entity))
                    continue;

                switch (Method)
                {
                    case ChangeFactionMethod.Add:
                        _npcFaction.AddFaction(entity, Faction, true);
                        break;
                    case ChangeFactionMethod.Remove:
                        _npcFaction.RemoveFaction(entity, Faction, true);
                        break;
                    case ChangeFactionMethod.Set:
                        _npcFaction.ClearFactions(entity, true);
                        _npcFaction.AddFaction(entity, Faction, true);
                        break;
                }
            }
        }

        protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
            => Loc.GetString("reagent-effect-guidebook-change-faction",
                ("chance", Probability),
                ("faction", Faction.ToLower()),
                ("radius", Radius == null ? 0 : Radius));
    }

    public enum ChangeFactionMethod : byte
    {
        Add,
        Remove,
        Set
    }
}
