using Content.Client.UserInterface.Fragments;
using Content.Shared.CartridgeLoader;
using Content.Shared._WL.CartridgeLoader.Cartridges;
using Robust.Client.UserInterface;

namespace Content.Client._WL.CartridgeLoader.Cartridges;

public sealed partial class NanoChatUi : UIFragment
{
    private NanoChatUiFragment? _fragment;

    public override Control GetUIFragmentRoot() => _fragment!;

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new NanoChatUiFragment();
        _fragment.NewChat += (number, name, job) => Send(userInterface, NanoChatUiMessageType.NewChat, number, name, job);
        _fragment.SelectChat += number => Send(userInterface, NanoChatUiMessageType.SelectChat, number);
        _fragment.DeleteChat += number => Send(userInterface, NanoChatUiMessageType.DeleteChat, number);
        _fragment.SendMessage += (number, content) => Send(userInterface, NanoChatUiMessageType.SendMessage, number, content);
        _fragment.ToggleMute += () => Send(userInterface, NanoChatUiMessageType.ToggleMute);
        _fragment.ToggleListNumber += () => Send(userInterface, NanoChatUiMessageType.ToggleListNumber);
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is NanoChatUiState nanoChat)
            _fragment?.UpdateState(nanoChat);
    }

    private static void Send(BoundUserInterface ui,
        NanoChatUiMessageType type,
        uint? number = null,
        string? content = null,
        string? recipientJob = null)
    {
        ui.SendMessage(new CartridgeUiMessage(new NanoChatUiMessageEvent(type, number, content, recipientJob)));
    }
}
