using Content.Shared.FixedPoint;
using Content.Shared._WL.Slimes.Enums;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Xenobiology
{
    [Serializable, NetSerializable]
    public sealed partial class SlimeScannerScannedUserMessage : BoundUserInterfaceMessage
    {
        public readonly NetEntity? NetEntity;
        public string LifeStage;
        public Dictionary<string, string> FormattedMutationCondDesc;
        public FixedPoint2 MutationProbability;
        public Color CoreReagentColor;
        public FixedPoint2? CoreCost;
        public int? CoreResearchPoints;
        public FixedPoint2 CurrentHunger;
        public FixedPoint2 MaxHunger;
        public FixedPoint2 GrowthStage;
        public FixedPoint2 MaxGrowthStage;
        public int RelationshipPoints;

        public SlimeScannerScannedUserMessage(
            NetEntity? netEntity,
            string lifeStage,
            Dictionary<string, string> formattedMutationCondDesc,
            FixedPoint2 mutationProbability,
            FixedPoint2? coreCost,
            int? coreResearchPoints,
            Color coreReagentColor,
            FixedPoint2 currentHunger,
            FixedPoint2 maxHunger,
            FixedPoint2 growthStage,
            FixedPoint2 maxGrowthStage,
            int relationshipPoints)
        {
            NetEntity = netEntity;
            LifeStage = lifeStage;
            FormattedMutationCondDesc = formattedMutationCondDesc;
            MutationProbability = mutationProbability;
            CoreCost = coreCost;
            CoreResearchPoints = coreResearchPoints;
            CoreReagentColor = coreReagentColor;
            CurrentHunger = currentHunger;
            MaxHunger = maxHunger;
            GrowthStage = growthStage;
            MaxGrowthStage = maxGrowthStage;
            RelationshipPoints = relationshipPoints;
        }

        public SlimeScannerScannedUserMessage(NetEntity? netEntity)
        {
            NetEntity = netEntity;
            FormattedMutationCondDesc = new();
            LifeStage = "invalid";
        }
    }
}
