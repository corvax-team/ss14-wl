using Content.Shared._WL.Languages;
using Content.Shared._WL.Languages.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Radio;
using Content.Shared.Speech;
using Content.Shared.Trigger.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._WL.Languages;

public sealed class LanguagesSystem : SharedLanguagesSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IEntityManager _ent = default!;

    /// <summary>
    /// Потому что <see cref="Shared.Chat.ChatChannelExtensions.TextColor(Shared.Chat.ChatChannel)" />.
    /// </summary>
    private static readonly Color DefaultChatTextColor = Color.LightGray;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LanguagesComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<LanguagesComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<AddLanguagesComponent, ComponentInit>(OnAddInit);

        SubscribeNetworkEvent<LanguageChangeEvent>(OnGlobalLanguageChange);
        SubscribeNetworkEvent<LanguagesSyncEvent>(OnLanguagesSync);
    }

    public void OnMapInit(EntityUid ent, LanguagesComponent component, ref MapInitEvent args)
    {
        var langs = component.Speaking;
        if (langs.Count == 0)
            return;
        foreach (ProtoId<LanguagePrototype> protoid in langs)
        {
            var proto = GetLanguagePrototype(protoid);
            if (proto != null)
            {
                if (TryChangeLanguage(_ent.GetNetEntity(ent), protoid))
                    return;
            }
        }
    }

    public void AddLanguage(EntityUid ent, string language)
    {
        if (!TryComp<LanguagesComponent>(ent, out var comp))
            return;
        comp.Speaking.Add(language);
        comp.Understood.Add(language);
        Dirty(ent, comp);

        var net_ent = GetNetEntity(ent);
        SyncLanguages(net_ent, comp);
    }

    public void OnAddInit(EntityUid ent, AddLanguagesComponent component, ref ComponentInit args)
    {
        var langs = component.Languages;
        if (langs.Count == 0 || !TryComp<LanguagesComponent>(ent, out var out_comp))
        {
            RemComp<AddLanguagesComponent>(ent);
            return;
        }

        foreach (ProtoId<LanguagePrototype> protoid in langs)
        {
            var proto = GetLanguagePrototype(protoid);
            if (proto != null)
            {
                if (component.ToSpeaking)
                    out_comp.Speaking.Add(protoid);

                if (component.ToUnderstood)
                    out_comp.Understood.Add(protoid);
            }
        }
        RemComp<AddLanguagesComponent>(ent);

        Dirty(ent, out_comp);

        var net_ent = GetNetEntity(ent);
        SyncLanguages(net_ent, out_comp);
    }

    public void OnComponentInit(EntityUid ent, LanguagesComponent component, ref ComponentInit args)
    {
        var langs = component.Speaking;
        if (langs.Count == 0)
            return;
        foreach (ProtoId<LanguagePrototype> protoid in langs)
        {
            var proto = GetLanguagePrototype(protoid);
            if (proto != null)
            {
                if (TryChangeLanguage(_ent.GetNetEntity(ent), protoid))
                    return;
            }
        }
    }

    public void OnLanguagesSync(LanguagesSyncEvent msg, EntitySessionEventArgs args)
    {
        var entity = _ent.GetEntity(msg.Entity);
        if (!TryComp<LanguagesComponent>(entity, out var component))
            return;

        component.Speaking = msg.Speaking;
        component.Understood = msg.Understood;

        Dirty(entity, component);
    }

    public void OnGlobalLanguageChange(LanguageChangeEvent msg, EntitySessionEventArgs args)
    {
        var entity = _ent.GetEntity(msg.Entity);
        if (!TryComp<LanguagesComponent>(entity, out var component))
            return;
        OnLanguageChange(entity, (string)msg.Language);
    }

    public string ObfuscateMessageFromSource(string message, EntityUid source)
    {
        LanguagePrototype? proto = null;
        var innerMsg = message;

        if (TryProcessLanguageMessage(source, message, out var new_message))
        {
            proto = GetLanguagePrototype(source, message);
            innerMsg = new_message;
        }
        else if (TryComp<LanguagesComponent>(source, out var comp))
            proto = GetLanguagePrototype(comp.CurrentLanguage);

        if (proto == null)
            return innerMsg;

        return ObfuscateMessage(source, innerMsg, proto.ID);
    }

    public bool CanUnderstand(EntityUid source, EntityUid listener, string? message = null, ProtoId<LanguagePrototype>? overrideLang = null)
    {
        if (source == listener)
            return true;

        if (!TryComp<LanguagesComponent>(source, out var source_lang))
        {
            return true;
        }

        if (!TryComp<LanguagesComponent>(listener, out var listen_lang))
        {
            return true;
        }

        var message_language = GetLanguagePrototype(source, message) ?? GetLanguagePrototype(overrideLang) ?? source_lang.CurrentLanguage;

        return
            listen_lang.IsUnderstanding &&
            source_lang.IsSpeaking &&
            message_language != null &&
            listen_lang.Understood.Contains(message_language.Value);
    }

    public bool NeedTTS(EntityUid source)
    {
        if (!TryComp<LanguagesComponent>(source, out var source_lang))
            return false;
        else
        {
            var message_language = source_lang.CurrentLanguage;
            var proto = GetLanguagePrototype(message_language);
            if (proto == null)
                return false;
            else
            {
                return proto.NeedTTS;
            }
        }
    }

    public bool IsObfusEmoting(EntityUid source)
    {
        if (!TryComp<LanguagesComponent>(source, out var source_lang))
            return false;
        else
        {
            var message_language = source_lang.CurrentLanguage;
            var proto = GetLanguagePrototype(message_language);
            if (proto == null)
                return false;
            else
            {
                return proto.Obfuscation.IsEmoting();
            }
        }
    }

    public bool IsObfusEmoting(EntityUid source, string message)
    {
        var proto = GetLanguagePrototype(source, message);
        if (proto != null)
            return proto.Obfuscation.IsEmoting();

        return IsObfusEmoting(source);
    }

    /* Функция не используется нигде в коде, но может быть полезна. Закоментированно.
    public string GetObfusWrappedMessage(string message, EntityUid source, string name, SpeechVerbPrototype? speech = null)
    {
        var obfusMessage = ObfuscateMessageFromSource(message, source);
        var wrappedMessage = GetWrappedMessage(obfusMessage, source, name, speech);
        return wrappedMessage;
    }
    */

    public string GetRadioWrappedMessageFor(
        string msg,
        EntityUid source,
        EntityUid listener,
        string name,
        SpeechVerbPrototype speech,
        RadioChannelPrototype channel)
    {
        var isSelf = listener == source;
        var canUnderstand = CanUnderstand(source, listener, msg);

        var color = GetColor(msg, source, channel.Color);

        string message;
        if (isSelf || canUnderstand)
        {
            if (TryProcessLanguageMessage(source, msg, out var parsed))
                message = parsed;
            else message = msg;
        }
        else
            message = ObfuscateMessageFromSource(msg, source);

        var locId = speech.Bold
            ? "chat-radio-message-wrap-bold-lang"
            : "chat-radio-message-wrap-lang";

        if (!isSelf && !canUnderstand && IsObfusEmoting(source, msg))
            locId = "chat-radio-message-wrap-emote-lang";

        return Loc.GetString(locId,
            ("color", channel.Color),
            ("fontType", speech.FontId),
            ("fontSize", speech.FontSize),
            ("verb", Loc.GetString(_random.Pick(speech.SpeechVerbStrings))),
            ("channel", $"\\[{channel.LocalizedName}\\]"),
            ("name", name),
            ("message", message),
            ("langColor", color));
    }

    public Color GetColor(string message, EntityUid source, Color? fallback = null)
    {
        var language = GetLanguagePrototype(source, message);

        if (TryComp<LanguagesComponent>(source, out var comp))
            language ??= GetLanguagePrototype(comp.CurrentLanguage);

        if (language == null || language.Color == DefaultChatTextColor)
            return fallback ?? DefaultChatTextColor;

        return language.Color;
    }

    public string GetWhisperWrappedMessage(string message, EntityUid source, string name)
    {
        if (string.IsNullOrEmpty(message))
            return string.Empty;

        TryProcessLanguageMessage(source, message, out string new_message);

        var color = GetColor(message, source);

        var escapedMessage = FormattedMessage.EscapeText(new_message);

        return Loc.GetString("chat-manager-entity-whisper-wrap-message-lang",
            ("entityName", name),
            ("message", escapedMessage),
            ("langColor", color));
    }

    public string GetEmoteWrappedMessage(string message, EntityUid source, string name)
    {
        var ent = Identity.Entity(source, EntityManager);

        var wrappedMessage = Loc.GetString("chat-manager-entity-me-wrap-message",
            ("entityName", name),
            ("entity", ent),
            ("message", FormattedMessage.RemoveMarkupOrThrow(message))
        );

        return wrappedMessage;
    }

    public string GetWrappedMessage(string message, EntityUid source, string name, SpeechVerbPrototype speech)
    {
        if (string.IsNullOrEmpty(message))
            return string.Empty;

        TryProcessLanguageMessage(source, message, out string new_message);

        var color = GetColor(message, source);

        var locId = speech.Bold ? "chat-manager-entity-say-bold-wrap-message-lang" : "chat-manager-entity-say-wrap-message-lang";

        return Loc.GetString(locId,
            ("entityName", name),
            ("verb", Loc.GetString(_random.Pick(speech.SpeechVerbStrings))),
            ("fontType", speech.FontId),
            ("fontSize", speech.FontSize),
            ("message", FormattedMessage.EscapeText(new_message)),
            ("langColor", color));
    }
}
