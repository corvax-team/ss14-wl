namespace Content.Shared._WL.Skills.Components;

[RegisterComponent]
[Access(typeof(SharedSkillsSystem))]
public sealed partial class InitialSkillsComponent : Component
{
    /// <summary>
    /// Skills that are set to exact levels when the entity is created.
    /// </summary>
    [DataField("initialSkills")]
    public Dictionary<SkillType, int> InitialSkills = new();

    /// <summary>
    /// Skills that are set to random levels when the entity is created.
    /// </summary>
    [DataField("randomSkills")]
    public List<SkillType> RandomSkills = new();

    /// <summary>
    /// Skills that are added to existing skills (if entity already has skills component).
    /// </summary>
    [DataField("addSkills")]
    public Dictionary<SkillType, int> AddSkills = new();

    /// <summary>
    /// Whether to randomize ALL skills when the entity is created.
    /// </summary>
    [DataField]
    public bool RandomizeAllSkills = false;

    /// <summary>
    /// Whether to override existing skills or add to them.
    /// </summary>
    [DataField]
    public bool OverrideExisting = true;

    /// <summary>
    /// Minimum level for randomized skills (1-4).
    /// </summary>
    [DataField]
    public int RandomMinLevel = 1;

    /// <summary>
    /// Maximum level for randomized skills (1-4).
    /// </summary>
    [DataField]
    public int RandomMaxLevel = 4;
}
