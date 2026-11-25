using Content.Shared.Alert;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Parry;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ParryComponent : Component
{
    [DataField]
    public EntProtoId ParriedStatusEffect = "StatusEffectAttackProhibition";

    [DataField]
    public ProtoId<AlertPrototype> ParryAlertProtoId = "ParryAlert";

    [DataField, AutoNetworkedField]
    public DamageSpecifier? SuccessfulParryDamage = null;

    [DataField, AutoNetworkedField]
    public float MeleeDamageReduction = 0.6f;

    [DataField, AutoNetworkedField]
    public float MovementSpeedModifier = 0.65f;

    [DataField, AutoNetworkedField]
    public Angle ParryAngle = Angle.FromDegrees(160f);

    [DataField, AutoNetworkedField]
    public TimeSpan ParryDuration = TimeSpan.FromSeconds(0.65f);

    [DataField, AutoNetworkedField]
    public TimeSpan ParryCooldown = TimeSpan.FromSeconds(2.0f);

    [DataField, AutoNetworkedField]
    public TimeSpan NextParryTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan AttackProhibitionDuration = TimeSpan.FromSeconds(1.5f);

    [DataField, AutoNetworkedField]
    public float AngleOffset = 0.55f;

    [DataField, AutoNetworkedField]
    public float ForwardOffset = 0.35f;

    [DataField]
    public SoundSpecifier ParrySound = new SoundPathSpecifier("/Audio/Weapons/block_metal1.ogg");

    /// <summary>
    /// Minimum time a hit from an NPC is held back so that a late parry input can still count
    /// </summary>
    [DataField]
    public TimeSpan MinCompensation = TimeSpan.FromSeconds(0.1f);

    /// <summary>
    /// Maximum hold-back time, regardless of how high the player's ping is
    /// </summary>
    [DataField]
    public TimeSpan MaxCompensation = TimeSpan.FromSeconds(0.3f);
}
