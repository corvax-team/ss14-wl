using Content.Server.Chat.Systems;
using Content.Shared._WL.Slimes;

namespace Content.Server._WL.Slimes.Commands;

public sealed partial class SaySlimeCommand : SlimeCommand
{
    public override bool Command(SlimeCommandArgs args)
    {
        var _entMan = args.EntityManager;
        var _chat = _entMan.System<ChatSystem>();

        if (args.ChatMessage == null)
            return false;

        List<char> toSay = new();
        bool write = false;
        for (var i = 0; i < args.ChatMessage.Length; i++)
        {
            if (args.ChatMessage[i].Equals('"'))
            {
                if (write == true)
                {
                    _chat.TrySendInGameICMessage(args.Slime, new string(toSay.ToArray()), InGameICChatType.Speak, false, true, checkRadioPrefix: false);
                    return true;
                }
                else write = !write;

                continue;
            }

            if (write == true)
                toSay.Add(args.ChatMessage[i]);
        }

        return false;
    }
}
