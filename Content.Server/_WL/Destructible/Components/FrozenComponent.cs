using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Destructible.Components;

[RegisterComponent]
public sealed partial class FrozenComponent : Component
{
    [DataField] public LocId FrozenPrefix = "frozen-entity-prefix";

    [DataField] public LocId FrozenPopup = "frozen-entity-popup";
    [DataField] public LocId FrozenHealthString = "frozen-entity-health-string";

    [DataField] public string BaseName;
    [DataField] public Color BaseSkinColor;

    [DataField] public ProtoId<DamageTypePrototype> FrozenDamage = "Cold";
}
