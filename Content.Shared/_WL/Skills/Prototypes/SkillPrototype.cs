using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Skills;

[Prototype("skill")]
public sealed partial class SkillPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("skillType", required: true)]
    public SkillType SkillType { get; private set; }

    [DataField("costs", required: true)]
    public int[] Costs { get; private set; } = new[] { 0, 0, 0, 0 };

    [DataField("color", required: true)]
    public Color Color { get; private set; } = Color.White;
}
