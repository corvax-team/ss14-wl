using Content.Server._WL.Slimes.Systems;
using Content.Shared._WL.Slimes;
using Content.Shared._WL.Slimes.Enums;
using Content.Shared.Whitelist;
using JetBrains.Annotations;

namespace Content.Server._WL.Slimes;

[RegisterComponent]
[AutoGenerateComponentPause]
public sealed partial class SlimeComponent : Component
{
    [DataField("slimeGroupName", required: true)]
    public string SlimeGroupName = "grey";

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("currentAge"), Access(typeof(SlimeSystem))]
    public SlimeLifeStage CurrentAge = SlimeLifeStage.Young;

    /// <summary>
    /// Contains the IDs of the entities that were nearby when slime ate, and also contains the number of points.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<EntityUid, int> Relationships = new();

    /// <summary>
    /// Will slime be aggressive when his hunger drops to a minimum.
    /// It is necessary for pets. The Smile for example.
    /// </summary>
    [DataField("canBeAngry")]
    public bool CanBeAngry = true;

    #region Growth
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("growCheckInterval")]
    public TimeSpan GrowCheckInterval = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Probability of increment <see cref="GrowStage"/> after <see cref="GrowCheckInterval"/>.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("growProbability")]
    public float GrowProbability = 0.5f;

    /// <summary>
    /// The current stage of growth.
    /// When it reaches <see cref="SlimeChangeGrowData.GrowthStageBound"/>, slime will increase <see cref="CurrentAge"/>.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("growStage")]
    public int GrowthStage = 0;

    /// <summary>
    /// Data for each stage of slime's life.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("growthData")]
    public Dictionary<SlimeLifeStage, SlimeChangeGrowData> GrowthData = new();
    #endregion

    #region Humanoid
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("canBecomeHumanoid")]
    public bool CanBecomeHumanoid = false;

    /// <summary>
    /// Skin color of the creature that will appear when the slime reaches the Humanoid stage.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("humanoidStageSkinColor")]
    public Color HumanoidStageSkinColor = new(0, 0, 0, 255);

    /// <summary>
    /// A prototype of an entity that will spawn instead of slime when it reaches the Humanoid stage.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("humanoidPrototype")]
    public string HumanoidPrototype = "MobSlimePerson";
    #endregion

    #region Eating
    [ViewVariables(VVAccess.ReadOnly), Access(typeof(SlimeSystem))]
    public bool IsEating = false;

    /// <summary>
    /// The probability that slime will start eating. Check <see cref="SlimeSystem.Update(float)"/>.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("eatProbability")]
    public float EatProbability = 0.2f;

    /// <summary>
    /// Contains components of entities that slime can start eating. Check <see cref="SlimeSystem.MakeSlimeToEatTarget(EntityUid, EntityUid, double, SlimeComponent?)"/>.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("tastePreferences"), Access(typeof(SlimeSystem))]
    public EntityWhitelist TastePreferences = new();

    /// <summary>
    /// How long does one eating DoAfter take.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("eatingTime"), Access(typeof(SlimeSystem))]
    public float EatingTime = 1f;
    #endregion

    #region Commands
    /// <summary>
    /// The probability that slime will execute the <see cref="CommandToCommit"/>.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("responseProbability")]
    public float ResponseProbability = 0.7f;

    /// <summary>
    /// The current command that the slime should execute. Check <see cref="SlimeSystem.Update(float)"/>.
    /// </summary>
    [Access(typeof(SlimeSystem))]
    [ViewVariables(VVAccess.ReadOnly)]
    public (SlimeCommand, SlimeCommandArgs)? CommandToCommit = null;
    #endregion

    #region Core
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("corePrototype", required: true), Access(typeof(SlimeSystem))]
    public string CorePrototype;

    public int CoreAmount;
    #endregion

    #region Mutation
    /// <summary>
    /// The minimum bound for the probability of slime mutation during split.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("minMutationProbability")]
    public float MinMutationProbabilityBound = 0.17f;

    /// <summary>
    /// The maximum bound for the probability of slime mutation during split.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("maxMutationProbability")]
    public float MaxMutationProbabilityBound = 0.4f;

    /// <summary>
    /// Randomly generated probability along the lower and upper bounds.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float CurrentMutationProbability;

    /// <summary>
    /// Contains settings for transferring the chance of mutation to descendant slimes.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("passMutationProperties")]
    public PassMutationData PassMutationPropertiesNextGeneration = new();
    #endregion

    /// <summary>
    /// Needed for tracking <see cref="GrowCheckInterval"/> in <see cref="SlimeSystem.Update(float)"/>.
    /// </summary>
    [AutoPausedField]
    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan LastTime = TimeSpan.Zero;

    public EntityUid? EatActionContainer;


    [DataDefinition]
    [UsedImplicitly]
    public sealed partial class PassMutationData
    {
        /// <summary>
        /// If true, it transfers the probability of mutation to the next generation.
        /// </summary>
        [DataField("passMutationProb")]
        public bool PassMutationProbability = false;

        /// <summary>
        /// If true, the probability will be carried over to all generations.
        /// </summary>
        [DataField("recursive")]
        public bool Recursive = false;
    }
}

[MeansImplicitUse, DataDefinition]
public sealed partial class SlimeChangeGrowData
{
    /// <summary>
    /// The cost of splitting a slime is subtracted from <see cref="SplitPointsAmount"/>.
    /// </summary>
    [DataField("splitCost")]
    public int SplitCost = 1;

    /// <summary>
    /// Amount of cores that fall out during splitting.
    /// </summary>
    [DataField("coreAmount")]
    public int CoreAmount = 1;

    /// <summary>
    /// How many splitting points are there in total at this stage. <see cref="SplitCost"/>.
    /// </summary>
    [DataField("splitPointsAmount")]
    public int SplitPointsAmount = 4;

    /// <summary>
    /// The boundary, upon reaching which the slime increases its <see cref="SlimeComponent.CurrentAge"/>.
    /// </summary>
    [DataField("growthStageBound")]
    public float GrowthStageBound = 10;

    #region Growth Modifiers
    /// <summary>
    /// Changes the damage when the growth stage changes.
    /// </summary>
    [DataField("damage")]
    public float Damage = 7;

    /// <summary>
    /// Changes the attack speed when the growth stage changes.
    /// </summary>
    [DataField("attackRate")]
    public float AttackRate = 1f;

    /// <summary>
    /// Changes the speed when the growth stage changes.
    /// </summary>
    [DataField("walkSpeed")]
    public float WalkSpeed = 3f;

    /// <summary>
    /// Changes the speed when the growth stage changes.
    /// </summary>
    [DataField("sprintSpeed")]
    public float SprintSpeed = 3f;

    /// <summary>
    /// Changes the size of the fixture shape.
    /// </summary>
    [DataField("fixtureShape")]
    public float FixtureShape = 0.4f;
    #endregion

    /// <summary>
    /// The damage that is dealed on the target that the slime eats in one DoAfter
    /// </summary>
    [DataField("eatingDamage")]
    public float EatingDamage = 6.5f;

    /// <summary>
    /// The number of replenished hunger points when eating a target with a slime in one DoAfter.
    /// </summary>
    [DataField("hungerSupply")]
    public float HungerSupply = 4f;
}
