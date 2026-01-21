using Content.Shared._WL.Languages;
using Content.Shared._WL.Languages.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.Timing;
using Content.Shared.Radio;
using Content.Shared.Speech;
using Content.Shared.Trigger.Systems;
using Content.Server.Atmos.EntitySystems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Robust.Shared.Timing;

namespace Content.Server._WL.Languages;

public sealed class LanguagesSystem : SharedLanguagesSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    /// <summary>
    /// Потому что <see cref="Shared.Chat.ChatChannelExtensions.TextColor(Shared.Chat.ChatChannel)" />.
    /// </summary>
    private static readonly Color DefaultChatTextColor = Color.LightGray;

    private static readonly string DefaultChatTextFontId = "Default";
    private static readonly int DefaultChatTextFontSize = 12;
    private static readonly float FullTalkPressure = 50f;
    private static readonly float MinTalkPressure = 5f;
    private static readonly float ForceWhisperProb = .3f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LanguagesComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<LanguagesComponent, PressureLanguageCheckEvent>(OnPressureLanguageCheck);
        SubscribeLocalEvent<ModifyLanguagesComponent, ComponentInit>(OnModifyInit);

        SubscribeNetworkEvent<LanguageChangeEvent>(OnGlobalLanguageChange);
        SubscribeNetworkEvent<LanguagesSyncEvent>(OnLanguagesSync);
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

    public void OnModifyInit(EntityUid ent, ModifyLanguagesComponent component, ref ComponentInit args)
    {
        var langs = component.Languages;
        if (!TryComp<LanguagesComponent>(ent, out var out_comp))
        {
            RemComp<ModifyLanguagesComponent>(ent);
            return;
        }

        if (!component.SpecieLanguage)
        {
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
        }
        else
        {
            var protoid = out_comp.SpecieLanguage;
            var proto = GetLanguagePrototype(protoid);
            if (proto != null && protoid != null)
            {
                if (component.ToSpeaking)
                    out_comp.Speaking.Remove(protoid.Value);

                if (component.ToUnderstood)
                    out_comp.Understood.Remove(protoid.Value);
            }
        }

        RemComp<ModifyLanguagesComponent>(ent);

        Dirty(ent, out_comp);

        var net_ent = GetNetEntity(ent);
        SyncLanguages(net_ent, out_comp);
    }

    public void OnComponentInit(EntityUid ent, LanguagesComponent component, ref ComponentInit args)
    {
        var langs = component.Speaking;
        if (langs.Count == 0)
            return;

        if (component.SpecieLanguage != null)
            AddLanguage(ent, component.SpecieLanguage);

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

    public void OnPressureLanguageCheck(EntityUid source, LanguagesComponent comp, ref PressureLanguageCheckEvent args)
    {
        var passability = CheckPressurePass(source, args.Message);
        if (passability == 0)
        {
            args.Cancelled = true;

            var time = _timing.CurTime;
            if (time > comp.LastPopup + comp.PopupCooldown)
            {
                comp.LastPopup = time;
                var message = Loc.GetString("languages-vacuum-block");

                _popup.PopupEntity(message, source, source);
            }

        }

        else if (passability < 1)
        {
            args.Message = ObfuscateMessageReadability(args.Message, passability);

            if (passability < ForceWhisperProb)
                args.ForceWhisper = true;

            var time = _timing.CurTime;
            if (time > comp.LastPopup + comp.PopupCooldown)
            {
                comp.LastPopup = time;
                var message = Loc.GetString("languages-vacuum-part-pass");

                _popup.PopupEntity(message, source);
            }
        }
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
            return true;
        else
        {
            var message_language = source_lang.CurrentLanguage;
            var proto = GetLanguagePrototype(message_language);
            if (proto == null)
                return true;
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
                return proto.Emoting;
            }
        }
    }

    public bool IsObfusEmoting(EntityUid source, string message)
    {
        var proto = GetLanguagePrototype(source, message);
        if (proto != null)
            return proto.Emoting;

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

        var language = GetLanguagePrototype(source, msg);

        var color = GetColor(language, channel.Color);

        var (fontSize, fontId) = GetFontParams(language, speech.FontSize, speech.FontId);

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

        var wrappedMessage = Loc.GetString(locId,
            ("color", channel.Color),
            ("fontType", fontId),
            ("fontSize", fontSize),
            ("verb", Loc.GetString(_random.Pick(speech.SpeechVerbStrings))),
            ("channel", $"\\[{channel.LocalizedName}\\]"),
            ("name", name),
            ("message", message),
            ("langColor", color));

        return SanitizeWrappedMessage(source, wrappedMessage);
    }

    public (int, string) GetFontParams(LanguagePrototype? language, int? fallbackSize = null, string? fallbackId = null)
    {
        int size;
        string id;

        if (language == null || language.FontSize == DefaultChatTextFontSize)
            size = fallbackSize ?? DefaultChatTextFontSize;
        else
            size = language.FontSize;

        if (language == null || language.FontId == DefaultChatTextFontId)
            id = fallbackId ?? DefaultChatTextFontId;
        else
            id = language.FontId;

        return (size, id);
    }

    public Color GetColor(LanguagePrototype? language, Color? fallback = null)
    {
        if (language == null || language.Color == DefaultChatTextColor)
            return fallback ?? DefaultChatTextColor;

        return language.Color;
    }

    public string GetWhisperWrappedMessage(string message, EntityUid source, string name)
    {
        if (string.IsNullOrEmpty(message))
            return string.Empty;

        TryProcessLanguageMessage(source, message, out string new_message);

        var language = GetLanguagePrototype(source, message);

        var color = GetColor(language);

        var escapedMessage = FormattedMessage.EscapeText(new_message);

        var wrappedMessage = Loc.GetString("chat-manager-entity-whisper-wrap-message-lang",
            ("entityName", name),
            ("message", escapedMessage),
            ("langColor", color));

        return SanitizeWrappedMessage(source, wrappedMessage);
    }

    public string GetEmoteWrappedMessage(string message, EntityUid source, string name)
    {
        var ent = Identity.Entity(source, EntityManager);

        var wrappedMessage = Loc.GetString("chat-manager-entity-me-wrap-message",
            ("entityName", name),
            ("entity", ent),
            ("message", FormattedMessage.RemoveMarkupOrThrow(message))
        );

        return SanitizeWrappedMessage(source, wrappedMessage);
    }

    public string GetWrappedMessage(string message, EntityUid source, string name, SpeechVerbPrototype speech)
    {
        if (string.IsNullOrEmpty(message))
            return string.Empty;

        TryProcessLanguageMessage(source, message, out string new_message);

        var language = GetLanguagePrototype(source, message);

        var color = GetColor(language);

        var (fontSize, fontId) = GetFontParams(language, speech.FontSize, speech.FontId);

        var locId = speech.Bold ? "chat-manager-entity-say-bold-wrap-message-lang" : "chat-manager-entity-say-wrap-message-lang";
        Logger.Debug(fontId);

        var wrappedMessage = Loc.GetString(locId,
            ("entityName", name),
            ("verb", Loc.GetString(_random.Pick(speech.SpeechVerbStrings))),
            ("fontType", fontId), //Оно не работает, даже если захардкодить. Не понимаю почему. (tau)
            ("fontSize", fontSize),
            ("message", FormattedMessage.EscapeText(new_message)),
            ("langColor", color));

        return SanitizeWrappedMessage(source, wrappedMessage);
    }

    private float CheckPressurePass(EntityUid source, string msg)
    {
        var language = GetLanguagePrototype(source, msg);

        if (language == null)
            return 1f;

        if (_atmosphereSystem.GetContainingMixture(source) is { } mixture)
        {
            var fixed_pressure = MathF.Max(mixture.Pressure - MinTalkPressure, 0f);

            var pressure_prob = MathF.Min(fixed_pressure/(FullTalkPressure - MinTalkPressure), 1f);

            var full_prob = MathF.Min(pressure_prob + language.PressurePass, 1f);

            return full_prob;
        }
        else
            return 1f;
    }

}
