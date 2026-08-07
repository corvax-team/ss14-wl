using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.CombatStand;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ReverseCardComponent : Component
{
    [DataField]
    public LocId? ExamineMessage = "reverse-card-examine";

    [DataField]
    public LocId? ReverseTargetMessage = "reverse-card-target-popup";

    [DataField]
    public bool CanBeExamined = true;

    [DataField]
    public bool OnMelee = true;

    [DataField]
    public bool OnProjectile = true;

    [DataField]
    public bool OnHitscan = true;

    [DataField]
    public bool SwapInHands = true;

    [DataField]
    public bool SwapInInventory = true;

    [DataField, AutoNetworkedField]
    public SoundSpecifier SoundOnSwapCard = new SoundPathSpecifier("/Audio/Weapons/Guns/Hits/laser_sear_wall.ogg")
    {
        Params = AudioParams.Default.WithVariation(0.05f)
    };

    [DataField, AutoNetworkedField]
    public SoundSpecifier SoundOnSwapWeapon = new SoundPathSpecifier("/Audio/Weapons/flash.ogg")
    {
        Params = AudioParams.Default.WithVolume(2f).WithMaxDistance(3f)
    };

    [DataField, AutoNetworkedField]
    public float Probability = 1f;

    [DataField]
    public bool InRightPlace = false;
}
