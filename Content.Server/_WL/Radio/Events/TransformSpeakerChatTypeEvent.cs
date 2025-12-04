using Content.Server.Chat.Systems;
using Content.Shared.Chat;

namespace Content.Server._WL.Radio.Events
{
    public sealed class TransformSpeakerChatTypeEvent : EntityEventArgs
    {
        public EntityUid Sender;
        public InGameICChatType ChatType;

        public TransformSpeakerChatTypeEvent(EntityUid sender, InGameICChatType chatType = InGameICChatType.Speak)
        {
            Sender = sender;
            ChatType = chatType;
        }
    }
}
