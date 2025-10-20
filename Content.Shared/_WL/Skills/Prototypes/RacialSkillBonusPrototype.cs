using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._WL.Skills;

[Prototype("racialSkillBonus")]
public sealed partial class RacialSkillBonusPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("species", customTypeSerializer: typeof(PrototypeIdSerializer<SpeciesPrototype>), required: true)]
    public string Species { get; private set; } = default!;

    [DataField("ageBonuses")]
    public Dictionary<int, int> AgeBonuses { get; private set; } = new();

    public int GetBonusForAge(int age)
    {
        if (AgeBonuses.Count == 0)
            return 0;

        int maxBonus = 0;
        foreach (var (bonusAge, bonus) in AgeBonuses)
        {
            if (bonusAge <= age)
                maxBonus += bonus;
        }

        return maxBonus;
    }
}
