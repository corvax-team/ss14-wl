using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Content.Shared.Throwing;
using Robust.Shared.Timing;

namespace Content.Shared._WL.Coin;

public sealed partial class SharedCoinSystem : EntitySystem
{
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CoinComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<CoinComponent, LandEvent>(OnLand);
        SubscribeLocalEvent<CoinComponent, ExaminedEvent>(OnExamined);
    }

    private void OnUseInHand(Entity<CoinComponent> ent, ref UseInHandEvent ev)
    {
        if (ev.Handled)
            return;

        Roll(ent);
        ev.Handled = true;
    }

    private void OnLand(Entity<CoinComponent> ent, ref LandEvent ev)
    {
        Roll(ent);
    }

    private void OnExamined(Entity<CoinComponent> ent, ref ExaminedEvent ev)
    {
        var side = Loc.GetString(ent.Comp.SideNames.TryGetValue(Math.Max(ent.Comp.CurSide, 0), out var value) ? value : "coin-component-head");
        ev.PushMarkup(Loc.GetString("coin-system-cur-side", ("side", side)));
    }

    private void Roll(Entity<CoinComponent> ent)
    {
        var rand = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(ent));

        var sideNum = rand.Next(1, Math.Max(ent.Comp.SideCount, 0) + 1);
        var side = Loc.GetString(ent.Comp.SideNames.TryGetValue(sideNum, out var value) ? value : "coin-component-head");

        _popup.PopupEntity(side, ent);
        ent.Comp.CurSide = sideNum;
        Dirty(ent);
    }
}
