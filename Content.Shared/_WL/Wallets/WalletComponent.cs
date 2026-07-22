using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._WL.Wallet;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WalletComponent : Component
{
    [DataField]
    public string IdSlotId = "idSlot";

    [ViewVariables, AutoNetworkedField]
    public EntityUid? ContainedId;

    [DataField]
    public Vector2 CardOffset = new(-0.07f, -0.03f);

    [DataField]
    public Vector2 IdOffset = new(-0.17f, 0.02f);
}
