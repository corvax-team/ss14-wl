using Content.Shared._WL.DynamicText;
using Robust.Client.Console;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using static System.Net.Mime.MediaTypeNames;

namespace Content.Client._WL.DynamicText;
public sealed partial class DynamicTextSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public void SaveDynamic(string text)
    {
        if (!_player.LocalEntity.HasValue)
            return;

        if (!_ent.TryGetNetEntity(_player.LocalEntity.Value, out var netEntity))
            return;

        if (string.IsNullOrEmpty(text))
            return;

        RaiseNetworkEvent(new DynamicTextEvent(netEntity.Value, text));
    }
    //public void LoadDynamic()
    //{
    //    if (!_player.LocalEntity.HasValue)
    //        return;

    //    if (!_ent.TryGetNetEntity(_player.LocalEntity.Value, out var netEntity))
    //        return;

    //    RaiseNetworkEvent(new DynamicTextEvent(netEntity.Value, string.Empty));
    //}

}
