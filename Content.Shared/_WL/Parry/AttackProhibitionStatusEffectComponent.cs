using Robust.Shared.GameStates;

namespace Content.Shared._WL.Parry;


[RegisterComponent, NetworkedComponent, Access(typeof(SharedParrySystem))]
public sealed partial class AttackProhibitionStatusEffectComponent : Component;
