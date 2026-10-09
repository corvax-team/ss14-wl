using System.Linq;
using Content.Shared._WL.Languages;
using Content.Shared._WL.Languages.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.Radio;
using Content.Shared.Speech;
using Content.Shared.Speech.Muting;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Robust.Shared.Timing;

namespace Content.Server._WL.Languages;

public sealed partial class LanguagesSystem : SharedLanguagesSystem
{
    [Dependency] private IEntityManager _entMan = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private ISharedPlayerManager _player = null!;
    [Dependency] private AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private StatusEffectsSystem _statusEffects = default!;

    private static readonly Color DefaultChatTextColor = Color.LightGray;

    private static readonly string DefaultChatTextFontId = "Default";
    private static readonly int DefaultChatTextFontSize = 12;
    private static readonly float FullTalkPressure = 50f;
    private static readonly float MinTalkPressure = 5f;
    private static readonly float ForceWhisperPass = .3f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<SetLanguageEvent>(SetLanguage);

        SubscribeLocalEvent<LanguagesComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<LanguagesComponent, PressureLanguageCheckEvent>(OnPressureLanguageCheck);
        SubscribeLocalEvent<ModifyLanguagesComponent, ComponentInit>(OnModifyInit);
    }

    public void ChangeLanguage(EntityUid ent, ProtoId<LanguagePrototype> language, LanguageLevel level)
    {
        if (!TryComp<LanguagesComponent>(ent, out var comp))
            return;

        if (level == LanguageLevel.None)
            comp.Languages.Remove(language);
        else if (!comp.Languages.TryAdd(language, level))
            comp.Languages[language] = level;

        Dirty(ent, comp);
        UpdateLanguagesWindow(ent);
    }

    public void OnModifyInit(EntityUid ent, ModifyLanguagesComponent component, ref ComponentInit args)
    {
        if (TryComp<LanguagesComponent>(ent, out var langComp))
        {
            foreach (var (protoId, level) in component.Languages)
            {
                ChangeLanguage(ent, protoId, level);
            }
        }

        RemComp<ModifyLanguagesComponent>(ent);
    }

    private void OnComponentInit(EntityUid ent, LanguagesComponent component, ref ComponentInit args)
    {
        foreach (var (protoId, level) in component.Languages)
        {
            if (level == LanguageLevel.None)
                ChangeLanguage(ent, protoId, level);
            else if (TrySetLanguage(ent, protoId))
                return;
        }
    }

    private void SetLanguage(SetLanguageEvent ev, EntitySessionEventArgs args)
    {
        if (!_entMan.TryGetEntity(ev.Entity, out var ent) ||
                args.SenderSession.AttachedEntity is not {} userEnt ||
                ent is {} entity ||
                entity == userEnt)
            return;

        TrySetLanguage(entity, ev.Language);
    }

    protected override void UpdateLanguagesWindow(EntityUid uid)
    {
        if (!_player.TryGetSessionByEntity(uid, out var session))
            return;

        var ev = new LanguagesChangedEvent();
        RaiseNetworkEvent(ev, session.Channel);
    }

    public void OnPressureLanguageCheck(EntityUid source, LanguagesComponent comp, ref PressureLanguageCheckEvent args)
    {
        var passability = CheckVocalizationPass(source, args.Language);

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

    public bool CanUnderstand(
        ProtoId<LanguagePrototype> langId,
        EntityUid listener,
        LanguageLevel requiredLevel = LanguageLevel.Full)
    {
        if (!TryComp<LanguagesComponent>(listener, out var listen_lang))
            return true;

        return GetLanguageLevel(listener, langId) >= (requiredLevel);
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

    public string GetRadioWrappedMessageFor(
        string msg,
        ProtoId<LanguagePrototype> langId,
        EntityUid listener,
        string name,
        SpeechVerbPrototype speech,
        RadioChannelPrototype channel,
        bool colorize = true)
    {
        if (!ProtoMan.TryIndex(langId, out var language))
            return string.Empty;

        var canColor = CanUnderstand(langId, listener, LanguageLevel.Low);
        var canUnderstand = CanUnderstand(langId, listener);

        var color = GetColor(language, colorize && canColor, channel.Color);
        var (fontSize, fontId) = GetFontParams(language, speech.FontSize, speech.FontId);

        if (!canUnderstand)
            msg = ObfuscateMessageForListener(msg, langId, listener);

        var locId = speech.Bold
            ? "chat-radio-message-wrap-bold-lang"
            : "chat-radio-message-wrap-lang";

        if (!canUnderstand && language.Emoting)
            locId = "chat-radio-message-wrap-emote-lang";

        var wrappedMessage = Loc.GetString(locId,
            ("color", channel.Color),
            ("fontType", fontId),
            ("fontSize", fontSize),
            ("verb", Loc.GetString(_random.Pick(speech.SpeechVerbStrings))),
            ("channel", $"\\[{channel.LocalizedName}\\]"),
            ("name", name),
            ("message", msg),
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
        if (language is null || language.Color == DefaultChatTextColor || !useColor)
            return fallback ?? DefaultChatTextColor;

        return language.Color;
    }

    public string GetWhisperWrappedMessage(string message, ProtoId<LanguagePrototype> langId, string name, bool colorize = true)
    {
        if (string.IsNullOrEmpty(message))
            return string.Empty;

        if (!ProtoMan.TryIndex(langId, out var language))
            return string.Empty;

        var color = GetColor(language, colorize);
        var escapedMessage = FormattedMessage.EscapeText(message);

        var locId = color != DefaultChatTextColor ?
            "chat-manager-entity-whisper-wrap-message-lang" :
            "chat-manager-entity-whisper-wrap-message";

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
        ProtoId<LanguagePrototype> langId,
        string name,
        SpeechVerbPrototype speech,
        bool colorize = true,
        EntityUid? listener = null)
    {
        if (string.IsNullOrEmpty(message))
            return string.Empty;

        if (!ProtoMan.TryIndex(langId, out var language))
            return string.Empty;

        var canColor = listener is {} listen && CanUnderstand(langId, listen, LanguageLevel.Low);

        var color = GetColor(language, colorize && canColor);

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
            ("message", FormattedMessage.EscapeText(message)),
            ("langColor", color));
    }

    private float CheckVocalizationPass(EntityUid source, ProtoId<LanguagePrototype> langId)
    {
        if (!ProtoMan.TryIndex(langId, out var language))
            return 1f;

        if (_atmosphereSystem.GetContainingMixture(source) is { } mixture)
        {
            var fixed_pressure = MathF.Max(mixture.Pressure - MinTalkPressure, 0f);

            var pressure_prob = MathF.Min(fixed_pressure / (FullTalkPressure - MinTalkPressure), 1f);

            if (_statusEffects.HasEffectComp<MutedStatusEffectComponent>(source))
                pressure_prob = 0f;

            var full_prob = MathF.Min(pressure_prob + language.PressurePass, 1f);

            return full_prob;
        }
        else
            return 1f;
    }
}
