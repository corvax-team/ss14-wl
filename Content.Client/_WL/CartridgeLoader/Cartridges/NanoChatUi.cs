using Content.Client.UserInterface.Fragments;
using Content.Shared.CartridgeLoader;
using Content.Shared._WL.CartridgeLoader.Cartridges;
using Content.Shared._WL.NanoChat;
using Robust.Client.UserInterface;

namespace Content.Client._WL.CartridgeLoader.Cartridges;

public sealed partial class NanoChatUi : UIFragment
{
    private NanoChatUiFragment? _fragment;

    public override Control GetUIFragmentRoot() => _fragment!;

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new NanoChatUiFragment();
        _fragment.NewChat += number => Send(userInterface, NanoChatUiMessageType.NewChat, number);
        _fragment.DeleteChat += number => Send(userInterface, NanoChatUiMessageType.DeleteChat, number);
        _fragment.SelectConversation += conversation => Send(userInterface, NanoChatUiMessageType.SelectConversation, conversation: conversation);
        _fragment.SendMessage += (conversation, content) => Send(userInterface, NanoChatUiMessageType.SendMessage, content: content, conversation: conversation);
        _fragment.CreateGroup += (name, members) => Send(userInterface, NanoChatUiMessageType.CreateGroup, content: name, memberNumbers: members);
        _fragment.RenameGroup += (conversation, name) => Send(userInterface, NanoChatUiMessageType.RenameGroup, content: name, conversation: conversation);
        _fragment.AddGroupMember += (conversation, number) => Send(userInterface, NanoChatUiMessageType.AddGroupMember, conversation: conversation, targetNumber: number);
        _fragment.RemoveGroupMember += (conversation, number) => Send(userInterface, NanoChatUiMessageType.RemoveGroupMember, conversation: conversation, targetNumber: number);
        _fragment.SetGroupAdmin += (conversation, number, admin) => Send(userInterface, NanoChatUiMessageType.SetGroupAdmin, conversation: conversation, targetNumber: number, value: admin);
        _fragment.LeaveGroup += conversation => Send(userInterface, NanoChatUiMessageType.LeaveGroup, conversation: conversation);
        _fragment.DeleteGroup += conversation => Send(userInterface, NanoChatUiMessageType.DeleteGroup, conversation: conversation);
        _fragment.ToggleGroupMute += conversation => Send(userInterface, NanoChatUiMessageType.ToggleGroupMute, conversation: conversation);
        _fragment.SetBlocked += (number, blocked) => Send(userInterface,
            blocked ? NanoChatUiMessageType.BlockContact : NanoChatUiMessageType.UnblockContact,
            targetNumber: number);
        _fragment.LoadOlderMessages += conversation => Send(userInterface, NanoChatUiMessageType.LoadOlderMessages, conversation: conversation);
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
        NanoChatConversationId? conversation = null,
        uint? targetNumber = null,
        List<uint>? memberNumbers = null,
        bool? value = null)
    {
        ui.SendMessage(new CartridgeUiMessage(new NanoChatUiMessageEvent(
            type,
            number,
            content,
            conversation,
            targetNumber,
            memberNumbers,
            value)));
    }
}
