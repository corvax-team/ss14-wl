using Content.Client._WL.DynamicText.UI;
using Content.Shared._WL.DynamicText;
using Robust.Client.Player;
using Robust.Client.UserInterface;

namespace Content.Client._WL.DynamicText;
public sealed partial class DynamicTextSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<RequestedDynamicTextEvent>(OnDynamicTextReceived);
    }

    public void SaveDynamicText(string text)
    {
        if (!_player.LocalEntity.HasValue)
            return;

        if (!_ent.TryGetNetEntity(_player.LocalEntity.Value, out var netEntity))
            return;

        if (text is null)
            return;

        RaiseNetworkEvent(new SetDynamicTextEvent(netEntity.Value, text));
    }
    public void RequestDynamicText()
    {
        if (!_player.LocalEntity.HasValue)
            return;

        if (!_ent.TryGetNetEntity(_player.LocalEntity.Value, out var netEntity))
            return;

        RaiseNetworkEvent(new RequestDynamicTextEvent(netEntity.Value));
    }
    public void OnDynamicTextReceived(RequestedDynamicTextEvent ev, EntitySessionEventArgs args)
    {
        _userInterfaceManager.GetUIController<DynamicTextUIController>().SetDynamicText(ev.DynamicText);
    }

}
