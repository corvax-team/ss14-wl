using Robust.Shared.Audio;
using Robust.Shared.Serialization;

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

        [DataField]
        public float IonStormSlownessFactor = 0.3f;

        [DataField]
        public float IonStormSlownessProbability = 0.78f;

        [DataField]
        public string IonStormPopupMessage = "Вы чувствуете из-за электромагнитных колебаний ваше тело становится тяжелее, а мысли запутаннее";

        [DataField]
        public bool IsUnderIonStorm = false;

        public TimeSpan NextTime = TimeSpan.Zero;

        [DataField]
        public SoundSpecifier ToggleLightSound = new SoundPathSpecifier("/Audio/Machines/button.ogg");
        [DataField("lightPrototype")]
        public String LightEntityPrototype = "AndroidLightMarker";
        [DataField]
        public string TogglelLightAction = "ActionToggleAndroidLight";
        [DataField]
        public float LightBaseRadius = 3.5f;

        [ViewVariables(VVAccess.ReadWrite)]
        public EntityUid? LightEntity;
        [ViewVariables(VVAccess.ReadWrite)]
        public EntityUid? ToggleLightActionEntity;
    }
}
