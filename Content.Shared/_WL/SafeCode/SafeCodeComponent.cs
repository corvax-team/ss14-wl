using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._WL.SafeCode;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SafeCodeComponent : Component
{
    [DataField]
    public string Code = "00000";

    [DataField, AutoNetworkedField]
    public bool Locked = true;

    [DataField, AutoNetworkedField]
    public bool Broken = false;

    [DataField]
    public int CodeLength = 5; //Probably no larger than a double.

    [DataField]
    public SoundSpecifier FailSound = new SoundPathSpecifier("/Audio/Machines/custom_deny.ogg");
}


