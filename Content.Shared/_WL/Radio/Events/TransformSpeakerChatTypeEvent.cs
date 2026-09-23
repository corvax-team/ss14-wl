using Content.Shared.Chat;

namespace Content.Shared._WL.Radio.Events;

public sealed class TransformSpeakerChatTypeEvent(EntityUid sender, InGameICChatType chatType = InGameICChatType.Speak) : EntityEventArgs
{
    public EntityUid Sender = sender;
    public InGameICChatType ChatType = chatType;
}
