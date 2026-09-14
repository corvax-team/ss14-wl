using System.Linq;
using Content.Shared._WL.Languages;
using Content.Shared._WL.Languages.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.Radio;
using Content.Shared.Speech;
using Content.Shared.Speech.Muting;
using Content.Server.Atmos.EntitySystems;
using Content.Shared._WL.Languages.Components.List;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Robust.Shared.Timing;

namespace Content.Server._WL.Languages;

public sealed partial class LanguagesSystem : SharedLanguagesSystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IEntityManager _ent = default!;
    [Dependency] private AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    private static readonly Color DefaultChatTextColor = Color.LightGray;

    private static readonly string DefaultChatTextFontId = "Default";
    private static readonly int DefaultChatTextFontSize = 12;
    private static readonly float FullTalkPressure = 50f;
    private static readonly float MinTalkPressure = 5f;
    private static readonly float ForceWhisperPass = .3f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LanguagesComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<LanguagesComponent, PressureLanguageCheckEvent>(OnPressureLanguageCheck);
        SubscribeLocalEvent<ModifyLanguagesComponent, ComponentInit>(OnModifyInit);

        SubscribeNetworkEvent<LanguageChangeEvent>(OnGlobalLanguageChange);
        SubscribeNetworkEvent<LanguagesSyncEvent>(OnLanguagesSync);
        SubscribeNetworkEvent<LanguageSyncRequestEvent>(OnLanguageSyncRequest);
    }

    public void AddLanguage(EntityUid ent, string language, int level)
    {
        if (!TryComp<LanguagesComponent>(ent, out var comp))
            return;
        var protoId = new ProtoId<LanguagePrototype>(language);
        var existing = comp.List.FirstOrDefault(x => x.Language == protoId);
        if (existing != null)
        {
            existing.LanguageLevel = level;
        }
        else
        {
            comp.List.Add(new LanguagesList
            {
                Language = protoId,
                LanguageLevel = level
            });
        }
        var netEnt = GetNetEntity(ent);
        SyncLanguages(netEnt, comp);
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
                    var existing = out_comp.List.FirstOrDefault(x => x.Language == protoid);
                    if (existing != null)
                    {
                        existing.LanguageLevel = component.LanguageLevel;
                    }
                    else
                    {
                        out_comp.List.Add(new LanguagesList
                        {
                            Language = protoid,
                            LanguageLevel = component.LanguageLevel
                        });
                    }
                }
            }
        }
        else
        {
            var specieProtoId = out_comp.SpecieLanguage;

            if (specieProtoId != null)
            {
                var proto = GetLanguagePrototype(specieProtoId.Value);
                if (proto != null)
                {
                    out_comp.List.RemoveAll(x => x.Language == specieProtoId.Value);

                    if (out_comp.CurrentLanguage == specieProtoId.Value)
                    {
                        var firstAvailable = out_comp.List.FirstOrDefault();
                        if (firstAvailable != null)
                        {
                            TryChangeLanguage(GetNetEntity(ent), firstAvailable.Language);
                        }
                        else
                        {
                            out_comp.CurrentLanguage = null;
                        }
                    }
                    out_comp.SpecieLanguage = null;
                }
            }
        }

        RemComp<ModifyLanguagesComponent>(ent);

        Dirty(ent, out_comp);

        var net_ent = GetNetEntity(ent);
        SyncLanguages(net_ent, out_comp);
    }

    public void OnComponentInit(EntityUid ent, LanguagesComponent component, ref ComponentInit args)
    {
        var langs = component.List;
        if (langs.Count == 0)
            return;

        foreach (var lang in langs)
        {
            var protoid = lang.Language;
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

        component.List = msg.List;

        Dirty(entity, component);
    }

    public void OnLanguageSyncRequest(LanguageSyncRequestEvent msg, EntitySessionEventArgs args)
    {
        var entity = _ent.GetEntity(msg.Entity);
        if (!TryComp<LanguagesComponent>(entity, out var component))
            return;

        if (component.List != msg.List)
            SyncLanguages(msg.Entity, component);
    }

    public void OnGlobalLanguageChange(LanguageChangeEvent msg, EntitySessionEventArgs args)
    {
        var entity = _ent.GetEntity(msg.Entity);
        if (!HasComp<LanguagesComponent>(entity))
            return;
        OnLanguageChange(entity, (string)msg.Language);
    }

    public void OnPressureLanguageCheck(EntityUid source, LanguagesComponent comp, ref PressureLanguageCheckEvent args)
    {
        var passability = CheckVocalizationPass(source, args.Message);
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

            if (passability < ForceWhisperPass)
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

    public string ObfuscateMessageFromSource(
        string message,
        EntityUid source,
        EntityUid listener)
    {
        var innerMsg = message.Trim();

        LanguagePrototype? proto = null;

        if (TryProcessLanguageMessage(source, message, out var parsed))
        {
            proto = GetLanguagePrototype(source, message);
            innerMsg = parsed;
        }
        else if (TryComp<LanguagesComponent>(source, out var comp))
        {
            proto = GetLanguagePrototype(comp.CurrentLanguage);
        }

        if (source == listener)
            return innerMsg;

        if (proto == null)
            return innerMsg;

        return ObfuscateMessageForListener(innerMsg, proto.ID, listener);
    }

    public string ObfuscateMessageFromSource(string message, EntityUid source)
    {
        LanguagePrototype? proto = null;
        var innerMsg = message.Trim();

        if (TryProcessLanguageMessage(source, message, out var new_message))
        {
            proto = GetLanguagePrototype(source, message);
            innerMsg = new_message;
        }
        else if (TryComp<LanguagesComponent>(source, out var comp))
        {
            proto = GetLanguagePrototype(comp.CurrentLanguage);
        }

        if (proto == null)
            return innerMsg;

        var level = GetLanguageLevel(source, proto.ID);

        return ObfuscateMessageForLevel(innerMsg, proto.ID, level);
    }

    public bool CanUnderstand(
        EntityUid source,
        EntityUid listener,
        string? message = null,
        ProtoId<LanguagePrototype>? overrideLang = null)
    {
        if (source == listener)
            return true;

        if (!TryComp<LanguagesComponent>(source, out var source_lang))
            return true;

        if (!TryComp<LanguagesComponent>(listener, out var listen_lang))
            return true;

        var languageProto =
            GetLanguagePrototype(source, message)
            ?? GetLanguagePrototype(overrideLang)
            ?? GetLanguagePrototype(source_lang.CurrentLanguage);

        if (languageProto == null)
            return true;

        return
            listen_lang.IsUnderstanding &&
            source_lang.IsSpeaking &&
            GetLanguageLevel(listener, languageProto.ID) >= LanguageLevelFull;
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
        RadioChannelPrototype channel,
        bool colorize = true)
    {
        var canUnderstand = CanUnderstand(source, listener, msg);
        var language = GetLanguagePrototype(source, msg);

        var color = GetColor(language, colorize && canUnderstand, channel.Color);
        var (fontSize, fontId) = GetFontParams(language, speech.FontSize, speech.FontId);

        string message;

        if (canUnderstand)
        {
            if (TryProcessLanguageMessage(source, msg, out var parsed))
                message = parsed;
            else
                message = msg;
        }
        else
        {
            message = ObfuscateMessageFromSource(msg, source, listener);
        }

        var locId = speech.Bold
            ? "chat-radio-message-wrap-bold-lang"
            : "chat-radio-message-wrap-lang";

        if (!canUnderstand && IsObfusEmoting(source, msg))
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

        return wrappedMessage;
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

    public Color GetColor(LanguagePrototype? language, bool useColor = true, Color? fallback = null)
    {
        if (language == null || language.Color == DefaultChatTextColor || !useColor)
            return fallback ?? DefaultChatTextColor;

        return language.Color;
    }

    public string GetWhisperWrappedMessage(string message, EntityUid source, string name, bool colorize = true)
    {
        if (string.IsNullOrEmpty(message))
            return string.Empty;
        if (!TryProcessLanguageMessage(source, message, out string new_message) || string.IsNullOrEmpty(new_message))
            return string.Empty;

        var language = GetLanguagePrototype(source, message);
        var color = GetColor(language, colorize);
        var escapedMessage = FormattedMessage.EscapeText(new_message);

        var wrappedMessage = Loc.GetString("chat-manager-entity-whisper-wrap-message-lang",
            ("entityName", name),
            ("message", escapedMessage),
            ("langColor", color));

        return wrappedMessage;
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

    public string GetWrappedMessage(
        string message,
        EntityUid source,
        string name,
        SpeechVerbPrototype speech,
        bool colorize = true)
    {
        if (string.IsNullOrEmpty(message))
            return string.Empty;

        if (!TryProcessLanguageMessage(source, message, out var newMessage))
            return string.Empty;

        var language = GetLanguagePrototype(source, message);

        var color = GetColor(language, colorize);

        var (fontSize, fontId) = GetFontParams(
            language,
            speech.FontSize,
            speech.FontId);

        var locId = speech.Bold
            ? "chat-manager-entity-say-bold-wrap-message-lang"
            : "chat-manager-entity-say-wrap-message-lang";

        return Loc.GetString(
            locId,
            ("entityName", name),
            ("verb", Loc.GetString(_random.Pick(speech.SpeechVerbStrings))),
            ("fontType", fontId),
            ("fontSize", fontSize),
            ("message", FormattedMessage.EscapeText(newMessage)),
            ("langColor", color));
    }

    private float CheckVocalizationPass(EntityUid source, string msg)
    {
        var language = GetLanguagePrototype(source, msg);

        if (language == null)
            return 1f;

        if (_atmosphereSystem.GetContainingMixture(source) is { } mixture)
        {
            var fixed_pressure = MathF.Max(mixture.Pressure - MinTalkPressure, 0f);

            var pressure_prob = MathF.Min(fixed_pressure / (FullTalkPressure - MinTalkPressure), 1f);

            if (HasComp<MutedComponent>(source))
                pressure_prob = 0f;

            var full_prob = MathF.Min(pressure_prob + language.PressurePass, 1f);

            return full_prob;
        }
        else
            return 1f;
    }

}
