using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Parry;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ParryComponent : Component
{
    [DataField]
    public EntProtoId ParriedStatusEffect = "StatusEffectCantMeleeAttack";

    [DataField]
    public TimeSpan AttackProhibitionDuration = TimeSpan.FromSeconds(1f);

    [DataField, AutoNetworkedField]
    public float MeleeDamageReduction = 0.6f;

    [DataField, AutoNetworkedField]
    public float MovementSpeedModifier = 0.65f;

    [DataField, AutoNetworkedField]
    public Angle ParryAngle = Angle.FromDegrees(120f);

    [DataField, AutoNetworkedField]
    public TimeSpan ParryDuration = TimeSpan.FromSeconds(1000f);

    [DataField, AutoNetworkedField]
    public TimeSpan ParryCooldown = TimeSpan.FromSeconds(2.0f);

    [DataField, AutoNetworkedField]
    public TimeSpan NextParryTime = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public float AngleOffset = 0.55f;

    [DataField, AutoNetworkedField]
    public float ForwardOffset = 0.35f;

    /// <summary>
    /// Minimum time (server only) a hit from an NPC is held back so that a late parry input can still count
    /// </summary>
    [DataField]
    public TimeSpan MinCompensation = TimeSpan.FromSeconds(0.1f);

    /// <summary>
    /// Maximum hold-back time, regardless of how high the player's ping is
    /// </summary>
    [DataField]
    public TimeSpan MaxCompensation = TimeSpan.FromSeconds(0.3f);
}
