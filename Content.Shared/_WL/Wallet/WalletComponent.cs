using System.Numerics;

namespace Content.Shared._WL.Wallet;

[RegisterComponent]
public sealed partial class WalletComponent : Component
{
    [DataField]
    public string IdSlotId = "idSlot";

    [ViewVariables]
    public EntityUid? ContainedId;

    [DataField]
    public Vector2 CardOffset = new(-0.07f, -0.04f);

    [DataField]
    public Vector2 IdOffset = new(-0.17f, 0.02f);
}
