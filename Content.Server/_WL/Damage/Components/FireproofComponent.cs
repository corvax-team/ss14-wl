using Content.Shared.Damage;

namespace Content.Server._WL.Damage.Components;

[RegisterComponent]
public sealed partial class FireproofComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public float HeatDamageThresholdCache;

    [ViewVariables(VVAccess.ReadOnly)]
    public float FirestackFadeCache;

    [ViewVariables(VVAccess.ReadOnly)]
    public DamageSpecifier HeatDamageCache;
}
