using Content.Shared._WL.DynamicText;
using Robust.Client.Player;

namespace Content.Client._WL.DynamicText;
public sealed partial class DynamicTextSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public void SaveDynamicText(string text)
    {
        if (!_player.LocalEntity.HasValue)
            return;

        if (!_ent.TryGetNetEntity(_player.LocalEntity.Value, out var netEntity))
            return;

        if (string.IsNullOrEmpty(text))
            return;

        RaiseNetworkEvent(new SetDynamicTextEvent(netEntity.Value, text));
    }
    public void LoadDynamicText()
    {
        if (!_player.LocalEntity.HasValue)
            return;

        if (!_ent.TryGetNetEntity(_player.LocalEntity.Value, out var netEntity))
            return;

        RaiseNetworkEvent(new SetDynamicTextEvent(netEntity.Value, string.Empty));
    }

}
