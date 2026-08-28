using Content.Shared._WL.Skills;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Records;

[Prototype]
public sealed partial class BrainSourcePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name { get; private set; } = "Unknown";

    [DataField]
    public string Description { get; private set; } = "Unknown";

    [DataField]
    public ProtoId<RacialSkillBonusPrototype> SpecieBonus { get; private set; } = new();
}
