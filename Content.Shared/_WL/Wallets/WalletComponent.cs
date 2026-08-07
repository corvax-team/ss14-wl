using System.Numerics;
using Content.Shared.Access.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._WL.Wallets;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WalletComponent : Component
{
    [DataField]
    public string IdSlotId = "idSlot";

    /// <summary>
    /// The contained entity with <see cref="IdCardComponent"/>
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? ContainedId;

    /// <summary>
    /// Offsets the main layer sprite when inserted into slot.
    /// </summary>
    [DataField]
    public Vector2 CardOffset = new(-0.07f, -0.03f);

    /// <summary>
    /// Offsets the job icon sprite when inserted into slot.
    /// </summary>
    [DataField]
    public Vector2 IdOffset = new(-0.17f, 0.02f);

    /// <summary>
    /// Determines ability to use id-cards inside the storage of wallet
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool CopyAccesses;
}
