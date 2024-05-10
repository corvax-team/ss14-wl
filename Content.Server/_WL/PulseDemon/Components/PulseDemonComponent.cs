using Content.Server._WL.PulseDemon.Systems;
using Content.Shared._WL.PulseDemon;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;


namespace Content.Server._WL.PulseDemon.Components;


[RegisterComponent]
[Access(typeof(PulseDemonSystem))]
public sealed partial class PulseDemonComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("particlesSpawnRadius")]
    public float ParticlesSpawnRadius = 3.0f;

    /// <summary>
    /// When spawning a particle, if _robust.Prob() in <see cref="PulseDemonSystem.TimeBasedParticlesSpawn"/> returns true,
    /// Then the particles will spawn
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("particlesSpawnProbability")]
    public float ParticlesSpawnProbability = 0.3f;


    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("particlesPrototypes", customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
    public List<string> ParticlesPrototypes = ["EffectSparks2", "EffectSparks", "EffectSparks3"];

    /// <summary>
    /// The interval after which the particles try to spawn
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("particlesSpawnInterval")]
    public float ParticlesSpawnInterval = 2.29f; //In seconds

    [ViewVariables(VVAccess.ReadOnly), Access(typeof(PulseDemonSystem))]
    public TimeSpan NextParticlesSpawnTime = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadOnly), Access(typeof(PulseDemonSystem))]
    public TimeSpan CurrentTime = TimeSpan.Zero;
    /// <summary>
    /// Is it hiding?
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public bool IsHiding { get; set; }

    /// <summary>
    /// It shows how much the damage from the hit will be multiplied.
    /// Result of the product will be deducted from the battery charge of the demon.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("demonDamageModifier")]
    public float DemonDamageModifier = 100;

    /// <summary>
    /// It stores the actions that will be played when using Electromagnetic Tamper Action.
    /// <see cref="ElectromagneticTamperAction">
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("onElectromagneticTamperActions")]
    public List<ElectromagneticTamperAction> OnElectromagneticTamperActions;

    /// <summary>
    /// Used to check the number of other demons on the map
    /// <see cref="PulseDemonSystem.OnPulseDemonComponentShutdown">
    /// </summary>
    public MapId MapId;

    [ViewVariables(VVAccess.ReadOnly), Access(typeof(PulseDemonSystem))]
    public bool CanExistOutsideCable = false;


    //Characteristics base values

    /// <summary>
    /// How much energy is absorbed in one DoAfter
    /// </summary>
    [DataField("baseAbsorption")]
    public float BaseAbsorption = 100f;

    /// <summary>
    /// Time spent hacking APC
    /// </summary>
    [DataField("baseHijackTime")]
    public float BaseHijackTime = 60f;

    [DataField("baseCapacity")]
    public float BaseCapacity = 10000f;

    /// <summary>
    /// How much damage will be dealed to the demon if he is not on the cable.
    /// Measured as a percentage per second.
    /// </summary>
    [DataField("baseEndurance")]
    public float BaseEndurance = 0.05f;

    /// <summary>
    /// Time of once absorption.
    /// </summary>
    [DataField("baseEfficiency")]
    public float BaseEfficiency = 1.25f;

    /// <summary>
    /// Speed of the demon
    /// </summary>
    [DataField("baseSpeed")]
    public float BaseSpeed = 2.5f;


    //Characteristics level
    [ViewVariables(VVAccess.ReadWrite)]
    public float AbsorptrionLevel = 1;

    [ViewVariables(VVAccess.ReadWrite)]
    public float HijackSpeedLevel = 1;

    [ViewVariables(VVAccess.ReadWrite)]
    public float CapacityLevel = 1;

    [ViewVariables(VVAccess.ReadWrite)]
    public float EnduranceLevel = 1;

    [ViewVariables(VVAccess.ReadWrite)]
    public float EfficiencyLevel = 1;

    [ViewVariables(VVAccess.ReadWrite)]
    public float SpeedLevel = 1;
}
