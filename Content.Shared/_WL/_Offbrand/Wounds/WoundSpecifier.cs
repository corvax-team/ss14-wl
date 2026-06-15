using Content.Shared.Damage;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL._Offbrand.Wounds;

[DataDefinition, Serializable, NetSerializable]
public sealed partial class SimpleWoundSpecifier
{
    /// <summary>
    /// The wound to inflict
    /// </summary>
    [DataField(required: true)]
    public EntProtoId WoundPrototype;

    /// <summary>
    /// The damages to inflict it with
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier WoundDamages;
}
