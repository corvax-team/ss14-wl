using Robust.Shared.GameStates;

namespace Content.Shared._WL.Parry;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedParrySystem))]
public sealed partial class ActiveParryComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Weapon;
}
