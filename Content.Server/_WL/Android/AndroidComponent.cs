namespace Content.Server._WL.Android
{
    [RegisterComponent]
    public sealed partial class AndroidComponent : Component
    {
        [DataField]
        public float ChargeRate = 30f;

        [DataField]
        public float TargetDecreaseFactor = 30f;

        [DataField]
        public float ForcedSleepChance = 0.25f;

        [DataField]
        public TimeSpan TimeBetweenChecks = TimeSpan.FromSeconds(20f);

        [DataField]
        public TimeSpan SleepTimeMin = TimeSpan.FromSeconds(5f);

        [DataField]
        public TimeSpan SleepTimeMax = TimeSpan.FromSeconds(10f);

        public TimeSpan NextTime = TimeSpan.Zero;
    }
}
