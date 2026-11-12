using Content.Shared.FixedPoint;
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._WL.Damage;

[RegisterComponent, NetworkedComponent]
public sealed partial class DamageStatisticsComponent : Component
{
    [DataField]
    public DamageSpecifier DamagePerTime = new();

    [DataField]
    public DamageSpecifier TotalDamage = new();

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdateTime = TimeSpan.Zero;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan UpdateTime = TimeSpan.FromSeconds(2);
}
