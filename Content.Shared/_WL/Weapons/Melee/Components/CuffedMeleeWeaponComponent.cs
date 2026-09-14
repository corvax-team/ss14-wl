using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Weapons.Melee;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CuffedMeleeWeaponComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId WeaponId;

    [DataField, AutoNetworkedField]
    public EntityUid? WeaponUid;
}
