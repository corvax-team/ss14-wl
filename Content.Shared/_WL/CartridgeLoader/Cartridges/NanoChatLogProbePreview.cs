using System;
using System.Collections.Generic;
using System.Linq;
using Content.Shared._WL.NanoChat;

namespace Content.Shared._WL.CartridgeLoader.Cartridges;

/// <summary>
/// Creates a bounded NanoChat snapshot for LogProbe UI state updates while preserving total counts.
/// The complete server-side scan is still used for paper printouts.
/// </summary>
public static class NanoChatLogProbePreview
{
    public static NanoChatData Create(
        NanoChatData source,
        int totalMessageLimit,
        int messagesPerConversation)
    {
        var remaining = Math.Max(0, totalMessageLimit);
        var perConversation = Math.Max(0, messagesPerConversation);
        var messages = new Dictionary<uint, List<NanoChatMessage>>();
        var groupMessages = new Dictionary<uint, List<NanoChatMessage>>();

        var conversations = source.Messages
            .Select(pair => (IsGroup: false, pair.Key, History: pair.Value))
            .Concat(source.GroupMessages.Select(pair => (IsGroup: true, pair.Key, History: pair.Value)))
            .OrderByDescending(conversation => conversation.History.Count == 0
                ? TimeSpan.Zero
                : conversation.History[^1].Timestamp);

        foreach (var conversation in conversations)
        {
            var take = Math.Min(Math.Min(conversation.History.Count, perConversation), remaining);
            if (take == 0)
                continue;

            var target = conversation.IsGroup ? groupMessages : messages;
            target[conversation.Key] = conversation.History.GetRange(conversation.History.Count - take, take);
            remaining -= take;
        }

        return new NanoChatData(
            new Dictionary<uint, NanoChatRecipient>(source.Recipients),
            messages,
            new Dictionary<uint, int>(source.MessageCounts),
            new Dictionary<uint, NanoChatGroup>(source.Groups),
            groupMessages,
            new Dictionary<uint, int>(source.GroupMessageCounts),
            new HashSet<uint>(source.BlockedNumbers),
            source.CardNumber,
            source.Card);
    }
}
