using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL._Offbrand.StatusEffects;

[RegisterComponent]
[Access(typeof(GibOnDamageStatusEffectSystem))]
public sealed partial class GibOnDamageStatusEffectComponent : Component
{
    [DataField(required: true)]
    public float MinimumDamage;

    [DataField(required: true)]
    public ProtoId<DamageTypePrototype> DamageType;

    [DataField]
    public bool ShowPopup = true;

    [DataField]
    public string PopupText = "gib-effect-popup";
}
