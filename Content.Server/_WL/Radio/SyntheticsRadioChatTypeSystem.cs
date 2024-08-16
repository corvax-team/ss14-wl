using Content.Server._WL.Radio.Events;
using Content.Shared.PAI;
using Content.Shared.Silicons.Borgs.Components;

namespace Content.Server._WL.Radio
{
    public sealed partial class SyntheticsRadioChatTypeSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<PAIComponent, TransformSpeakerChatTypeEvent>(OnPAIChatTypeTransform);
            SubscribeLocalEvent<BorgChassisComponent, TransformSpeakerChatTypeEvent>(OnBorgChatTypeTransform);
        }

        private void OnPAIChatTypeTransform(EntityUid pai, PAIComponent _, TransformSpeakerChatTypeEvent ev)
        {
            ev.ChatType = Chat.Systems.InGameICChatType.Speak;
        }

        private void OnBorgChatTypeTransform(EntityUid borg, BorgChassisComponent _, TransformSpeakerChatTypeEvent ev)
        {
            ev.ChatType = Chat.Systems.InGameICChatType.Speak;
        }
    }
}
