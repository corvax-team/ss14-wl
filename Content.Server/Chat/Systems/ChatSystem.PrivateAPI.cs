using System.Linq;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.IdentityManagement;
using Content.Shared.Radio;
using Content.Shared._WL.Languages;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.Chat.Systems;

public sealed partial class ChatSystem
{
    private void SendEntitySpeak(
        EntityUid source,
        string originalMessage,
        ChatTransmitRange range,
        string? nameOverride,
        bool hideLog = false,
        bool ignoreActionBlocker = false
        )
    {
        if (!_actionBlocker.CanSpeak(source) && !ignoreActionBlocker)
            return;

        var message = TransformSpeech(source, originalMessage);

        if (message.Length == 0)
            return;

        var speech = GetSpeechVerb(source, message);

        // get the entity's apparent name (if no override provided).
        string name;
        if (nameOverride != null)
        {
            name = nameOverride;
        }
        else
        {
            var nameEv = new TransformSpeakerNameEvent(source, Name(source));
            RaiseLocalEvent(source, nameEv);
            name = nameEv.VoiceName;
            // Check for a speech verb override
            if (nameEv.SpeechVerb != null && ProtoMan.Resolve(nameEv.SpeechVerb, out var proto))
                speech = proto;
        }

        name = FormattedMessage.EscapeText(name);

        // WL-Change: Lang X Chat Start
        var pressureCheckEv = new PressureLanguageCheckEvent(message, source);
        RaiseLocalEvent(source, ref pressureCheckEv);
        if (pressureCheckEv.Cancelled)
            return;
        else if (pressureCheckEv.ForceWhisper)
        {
            SendEntityWhisper(source, originalMessage, range, null, nameOverride);
            return;
        }
        message = pressureCheckEv.Message;

        var wrappedMessage = _languages.GetWrappedMessage(message, source, name, speech);
        if (wrappedMessage.Length == 0)
            return;
        var obfuscatedMessage = _languages.ObfuscateMessageFromSource(message, source);

        var isEmoting = _languages.IsObfusEmoting(source, message);

        var obfusWrappedMessage = isEmoting ? _languages.GetEmoteWrappedMessage(obfuscatedMessage, source, name)
            : _languages.GetWrappedMessage(obfuscatedMessage, source, name, speech, false);

        var obfuscationChannel = isEmoting ? ChatChannel.Emotes
            : ChatChannel.Local;

        SendInVoiceRangeObfuscated(ChatChannel.Local, message, wrappedMessage, obfuscationChannel, obfuscatedMessage, obfusWrappedMessage, source, range);
        // WL-Change: Lang X Chat End

        var ev = new EntitySpokeEvent(source, message, originalMessage, null, null, /*WL-Changes: Languages*/obfuscatedMessage, null/*WL-Changes: Languages*/);
        RaiseLocalEvent(source, ev, true);

        // To avoid logging any messages sent by entities that are not players, like vendors, cloning, etc.
        // Also doesn't log if hideLog is true.
        if (!HasComp<ActorComponent>(source) || hideLog)
            return;

        if (originalMessage == message)
        {
            if (name != Name(source))
                _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Say from {source} as {name}: {originalMessage}. Obfuscated: to {obfuscatedMessage}."); //WL-Changes: Languages
            else
                _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Say from {source}: {originalMessage}. Obfuscated: to {obfuscatedMessage}."); //WL-Changes: Languages
        }
        else
        {
            if (name != Name(source))
                _adminLogger.Add(LogType.Chat, LogImpact.Low,
                    $"Say from {source} as {name}, original: {originalMessage}, transformed: {message}. Obfuscated: to {obfuscatedMessage}."); //WL-Changes: Languages
            else
                _adminLogger.Add(LogType.Chat, LogImpact.Low,
                    $"Say from {source}, original: {originalMessage}, transformed: {message}. Obfuscated to: {obfuscatedMessage}."); //WL-Changes: Languages

        }
    }

    private void SendEntityWhisper(
        EntityUid source,
        string originalMessage,
        ChatTransmitRange range,
        RadioChannelPrototype? channel,
        string? nameOverride,
        bool hideLog = false,
        bool ignoreActionBlocker = false
        )
    {
        if (!_actionBlocker.CanSpeak(source) && !ignoreActionBlocker)
            return;

        var message = TransformSpeech(source, FormattedMessage.RemoveMarkupOrThrow(originalMessage));
        if (message.Length == 0)
            return;

        var obfuscatedMessage = ObfuscateMessageReadability(message, 0.2f);

        // get the entity's name by visual identity (if no override provided).
        string nameIdentity = FormattedMessage.EscapeText(nameOverride ?? Identity.Name(source, EntityManager));
        // get the entity's name by voice (if no override provided).
        string name;
        if (nameOverride != null)
        {
            name = nameOverride;
        }
        else
        {
            var nameEv = new TransformSpeakerNameEvent(source, Name(source));
            RaiseLocalEvent(source, nameEv);
            name = nameEv.VoiceName;
        }
        name = FormattedMessage.EscapeText(name);

                //WL-Changes: Languages start
        var wrappedMessage = _languages.GetWhisperWrappedMessage(message, source, nameIdentity);
        if (wrappedMessage.Length == 0)
            return;

        var wrappedObfuscatedMessage = _languages.GetWhisperWrappedMessage(obfuscatedMessage, source, nameIdentity, false);

        var wrappedUnknownMessage = Loc.GetString("chat-manager-entity-whisper-unknown-wrap-message",
            ("message", FormattedMessage.EscapeText(obfuscatedMessage)));

        var langObfuscatedMessage = _languages.ObfuscateMessageFromSource(message, source);

        string obfusWrappedMessage;

        if (_languages.IsObfusEmoting(source, message))
            obfusWrappedMessage = _languages.GetEmoteWrappedMessage(langObfuscatedMessage, source, nameIdentity);
        else
            obfusWrappedMessage = _languages.GetWhisperWrappedMessage(langObfuscatedMessage, source, nameIdentity, false);

        var fullObfuscatedMessage = ObfuscateMessageReadability(langObfuscatedMessage, 0.2f);
        var wrappedFullObfuscatedMessage = Loc.GetString("chat-manager-entity-whisper-wrap-message",
            ("entityName", nameIdentity), ("message", FormattedMessage.EscapeText(fullObfuscatedMessage)));
        var obfusUnknownMessage = Loc.GetString("chat-manager-entity-whisper-unknown-wrap-message",
            ("message", FormattedMessage.EscapeText(fullObfuscatedMessage)));
        //WL-Changes: Languages end

        foreach (var (session, data) in GetRecipients(source, WhisperMuffledRange))
        {
            EntityUid listener;

            if (session.AttachedEntity is not { Valid: true } playerEntity)
                continue;
            listener = session.AttachedEntity.Value;

            //WL-Changes: Languages start
            var afterMessage = message;
            var afterObfusMessage = obfuscatedMessage;
            var afterWrappedMessage = wrappedMessage;
            var afterWrappedObfuscatedMessage = wrappedObfuscatedMessage;
            var afterUnknownMessage = wrappedUnknownMessage;
            if (!_languages.CanUnderstand(source, listener, message))
            {
                afterMessage = langObfuscatedMessage;
                afterObfusMessage = fullObfuscatedMessage;
                afterWrappedMessage = obfusWrappedMessage;
                afterWrappedObfuscatedMessage = wrappedFullObfuscatedMessage;
                afterUnknownMessage = obfusUnknownMessage;
                if (_languages.IsObfusEmoting(source, message))
                {
                    _chatManager.ChatMessageToOne(ChatChannel.Emotes, afterMessage, afterWrappedMessage, source, false, session.Channel);
                    continue;
                }
            }
            //WL-Changes: Languages end


            if (MessageRangeCheck(session, data, range) != MessageRangeCheckResult.Full)
                continue; // Won't get logged to chat, and ghosts are too far away to see the pop-up, so we just won't send it to them.

            if (data.Range <= WhisperClearRange || data.Observer)
                _chatManager.ChatMessageToOne(ChatChannel.Whisper, /*WL-Changes: Languages*/afterMessage, afterWrappedMessage/*WL-Changes: Languages*/, source, false, session.Channel);
            //If listener is too far, they only hear fragments of the message
            else if (_examineSystem.InRangeUnOccluded(source, listener, WhisperMuffledRange))
                _chatManager.ChatMessageToOne(ChatChannel.Whisper, /*WL-Changes: Languages*/afterObfusMessage, afterWrappedObfuscatedMessage/*WL-Changes: Languages*/, source, false, session.Channel);
            //If listener is too far and has no line of sight, they can't identify the whisperer's identity
            else
                _chatManager.ChatMessageToOne(ChatChannel.Whisper, /*WL-Changes: Languages*/afterObfusMessage, afterUnknownMessage/*WL-Changes: Languages*/, source, false, session.Channel);

        }

        _replay.RecordServerMessage(new ChatMessage(ChatChannel.Whisper, message, wrappedMessage, GetNetEntity(source), null, MessageRangeHideChatForReplay(range)));

        var ev = new EntitySpokeEvent(source, message, originalMessage, channel, obfuscatedMessage, /*WL-Changes: Languages*/fullObfuscatedMessage, langObfuscatedMessage/*WL-Changes: Languages*/);
        RaiseLocalEvent(source, ev, true);
        if (!hideLog)
            if (originalMessage == message)
            {
                if (name != Name(source))
                    _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Whisper from {source} as {name}: {originalMessage}.");
                else
                    _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Whisper from {source}: {originalMessage}.");
            }
            else
            {
                if (name != Name(source))
                    _adminLogger.Add(LogType.Chat, LogImpact.Low,
                    $"Whisper from {source} as {name}, original: {originalMessage}, transformed: {message}.");
                else
                    _adminLogger.Add(LogType.Chat, LogImpact.Low,
                    $"Whisper from {source}, original: {originalMessage}, transformed: {message}.");
            }
    }

    protected override void SendEntityEmote(
        EntityUid source,
        string action,
        ChatTransmitRange range,
        string? nameOverride,
        bool hideLog = false,
        bool checkEmote = true,
        bool ignoreActionBlocker = false,
        NetUserId? author = null
        )
    {
        if (!_actionBlocker.CanEmote(source) && !ignoreActionBlocker)
            return;

        // get the entity's apparent name (if no override provided).
        var ent = Identity.Entity(source, EntityManager);
        string name = FormattedMessage.EscapeText(nameOverride ?? Name(ent));

        // Emotes use Identity.Name, since it doesn't actually involve your voice at all.
        var wrappedMessage = Loc.GetString("chat-manager-entity-me-wrap-message",
            ("entityName", name),
            ("entity", ent),
            ("message", FormattedMessage.RemoveMarkupOrThrow(action)));

        if (checkEmote &&
            !TryEmoteChatInput(source, action))
            return;

        SendInVoiceRange(ChatChannel.Emotes, action, wrappedMessage, source, range, author);
        if (!hideLog)
            if (name != Name(source))
                _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Emote from {source} as {name}: {action}");
            else
                _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Emote from {source}: {action}");
    }

    // ReSharper disable once InconsistentNaming
    private void SendLOOC(EntityUid source, ICommonSession player, string message, bool hideChat)
    {
        var name = FormattedMessage.EscapeText(Identity.Name(source, EntityManager));

        if (_adminManager.IsAdmin(player))
        {
            if (!_adminLoocEnabled) return;
        }
        else if (!_loocEnabled) return;

        // If crit player LOOC is disabled, don't send the message at all.
        if (!_critLoocEnabled && _mobStateSystem.IsCritical(source))
            return;

        var wrappedMessage = Loc.GetString("chat-manager-entity-looc-wrap-message",
            ("entityName", name),
            ("message", FormattedMessage.EscapeText(message)));

        SendInVoiceRange(ChatChannel.LOOC, message, wrappedMessage, source, hideChat ? ChatTransmitRange.HideChat : ChatTransmitRange.Normal, player.UserId);
        _adminLogger.Add(LogType.Chat, LogImpact.Low, $"LOOC from {source}: {message}");
    }

    private void SendDeadChat(EntityUid source, ICommonSession player, string message, bool hideChat)
    {
        if (!_adminManager.IsAdmin(player) && !_deadChatEnabled)
            return;

        var clients = GetDeadChatClients();
        var playerName = Name(source);
        string wrappedMessage;
        if (_adminManager.IsAdmin(player))
        {
            wrappedMessage = Loc.GetString("chat-manager-send-admin-dead-chat-wrap-message",
                ("adminChannelName", Loc.GetString("chat-manager-admin-channel-name")),
                ("userName", player.Channel.UserName),
                ("message", FormattedMessage.EscapeText(message)));
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Admin dead chat from {source}: {message}");
        }
        else
        {
            wrappedMessage = Loc.GetString("chat-manager-send-dead-chat-wrap-message",
                ("deadChannelName", Loc.GetString("chat-manager-dead-channel-name")),
                ("playerName", (playerName)),
                ("message", FormattedMessage.EscapeText(message)));
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Dead chat from {source}: {message}");
        }

        _chatManager.ChatMessageToMany(ChatChannel.Dead, message, wrappedMessage, source, hideChat, true, clients.ToList(), author: player.UserId);
    }
}
