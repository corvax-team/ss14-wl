using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Combat;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AdvancedCombatComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool InCombat;

    [DataField, AutoNetworkedField]
    public bool EnableByIndicator = true;

    [DataField("actions")]
    public Dictionary<CombatAction, float> ActionsChances = new();

    [DataField("baseResist")]
    public Dictionary<CombatResist, float> BaseResistChances = new();

    [DataField("combatResist")]
    public Dictionary<CombatResist, float> CombatResistChances = new();

    #region Disarm

    [DataField, AutoNetworkedField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(1);

    [DataField]
    public float PushSpeed = 10f;

    [DataField]
    public float DisarmStaminaDamage = 5f;

    [DataField("disarmSuccessSound")]
    public SoundSpecifier DisarmSuccessSound = new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg");

    #endregion

    #region Grab

    [DataField, AutoNetworkedField]
    public GrabStage GrabStage = GrabStage.None;

    [DataField("grabCooldowns")]
    public Dictionary<GrabStage, TimeSpan> GrabStagesCooldowns = new();

    [DataField("grabRespiration")]
    public Dictionary<GrabStage, float> GrabRespiratonRatios = new();

    [DataField]
    public Dictionary<GrabStage, float> GrabMaxDistances = new();

    [DataField, AutoNetworkedField]
    public TimeSpan LastGrab;

    [DataField, AutoNetworkedField]
    public float RespiratorRatio = 1f;

    [DataField, AutoNetworkedField]
    public GrabStage GrabbedStage = GrabStage.None;

    [DataField]
    public HashSet<GrabStage> ThrowableStages = new();

    [DataField]
    public float ThrowSpeed = 10f;

    [DataField("grabSuccessSound")]
    public SoundSpecifier GrabSuccessSound = new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg");

    #endregion
}

[Serializable, NetSerializable]
public enum CombatIntent {
    None = 0,
    Disarm = 1,
    Grab = 2,
    Harm = 3
}

[Serializable, NetSerializable]
public enum CombatAction {
    None = 0,
    Push = 1,
    Disarm = 2,
    Knockdown = 3
}

[Serializable, NetSerializable]
public enum CombatResist {
    None = 0,
    Push = 1,
    Disarm = 2,
    Knockdown = 3
}

[Serializable, NetSerializable]
public enum GrabStage {
    None = 0,
    Passive = 1,
    Active = 2,
    Hard = 3
}
