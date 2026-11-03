using Robust.Shared.GameStates;

namespace Content.Shared._WL.Parry;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ParryComponent : Component
{
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
}
