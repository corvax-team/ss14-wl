using Robust.Shared.GameStates;

namespace Content.Shared._WL.Coin;

[RegisterComponent, NetworkedComponent]
public sealed partial class CoinComponent : Component
{
    [DataField]
    public int CurSide = 1;

    [DataField]
    public int SideCount = 2;

    [DataField]
    public Dictionary<int, string> SideNames = new() { { 1, "coin-component-head" }, { 2, "coin-component-tail" } };
}
