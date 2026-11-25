using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Parry;

[RegisterComponent, NetworkedComponent]
public sealed partial class MeleeWeaponLevelComponent : Component
{
    [DataField]
    public ParryWeaponLevel Level = ParryWeaponLevel.Middle;
}

[Serializable, NetSerializable]
public enum ParryWeaponLevel : byte
{
    Light,
    Middle,
    Heavy
}

