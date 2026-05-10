using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Skills;

[Prototype]
public sealed partial class SkillPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public SkillType SkillType { get; private set; }

    [DataField(required: true)]
    public int[] Costs { get; private set; } = new[] { 0, 0, 0, 0 };

    [DataField(required: true)]
    public Color Color { get; private set; } = Color.White;
}
