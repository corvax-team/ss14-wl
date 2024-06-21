using Content.Shared.Damage.Components;
using Content.Shared.Nutrition.Components;

namespace Content.Shared._WL.Stamina
{
    [RegisterComponent]
    public sealed partial class StaminaWasterComponent : Component
    {
        [DataField]
        public float StaminaPerSecond = 2.5f + 0.3f; //0.3f нужно в качестве штрафа задержки.

        [DataField]
        public float PenaltyForOneMassUnit = 0.025f;

        [DataField]
        public float MinStaminaBoundPercentage = 0.5f;

        [DataField]
        public float ThrowPenaltyForOneMassUnit = 0.2f;

        [DataField]
        public Dictionary<HungerThreshold, float> HungerPenalties = new();

        [Access(typeof(StaminaWasterSystem))]
        public float? Update = null;
    }
}
