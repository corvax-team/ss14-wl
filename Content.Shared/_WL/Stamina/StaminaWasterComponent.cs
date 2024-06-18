namespace Content.Shared._WL.Stamina
{
    [RegisterComponent]
    public sealed partial class StaminaWasterComponent : Component
    {
        [DataField]
        public float StaminaPerSecond = 2.5f + 0.3f; //0.3f нужно в качестве штрафа задержки.

        [DataField]
        public float PenaltyForOneMassUnit = 0.01f;

        public float? Update = null;
    }
}
