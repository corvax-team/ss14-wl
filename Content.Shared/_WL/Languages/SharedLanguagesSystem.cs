using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Linq;
using Content.Shared._WL.Languages.Components;
using Content.Shared.Chat;
using Content.Shared.GameTicking;
using Content.Shared.Popups;
using Content.Shared.Speech.Muting;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._WL.Languages;

public abstract partial class SharedLanguagesSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private GameTicker _ticker = default!;
    [Dependency] private IEntityManager _ent = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedChatSystem _chat = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private StatusEffectsSystem _statusEffects = default!;

    private Dictionary<LanguageLevel, float> ObfuscationLevels = new()
    {
        {LanguageLevel.None, 1f},
        {LanguageLevel.Low, 0.9f},
        {LanguageLevel.Medium, 0.6f},
        {LanguageLevel.Good, 0.3f},
        {LanguageLevel.Full, 0f}
    };

    private FrozenDictionary<char, LanguagePrototype> _keylan = default!;

    const char LanguagePrefix = '+';

    public const int LanguageLevelNone = 0;
    public const int LanguageLevelBasic = 1;
    public const int LanguageLevelPartial = 2;
    public const int LanguageLevelFull = 4;

    public const LanguageLevel LanguageLevelSpeak = LanguageLevel.Good;
    public static ProtoId<LanguagePrototype> DefaultLanguage = "Translate";
    public static ProtoId<LanguagePrototype> UnknownLanguage = "Unknown";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LanguagesComponent, RadioLanguageCheckEvent>(OnRadioLanguageCheck);

        CacheLanguages();

    }
    private void CacheLanguages()
    {
        _keylan = _prototype.EnumeratePrototypes<LanguagePrototype>()
            .ToFrozenDictionary(x => x.KeyLanguage);
    }

    [return: NotNullIfNotNull(nameof(id))]
    public LanguagePrototype? GetLanguagePrototype(ProtoId<LanguagePrototype>? id)
    {
        _prototype.TryIndex(id, out var proto);
        return proto;
    }

    public bool TryGetLanguagePrototype(ProtoId<LanguagePrototype>? id, [NotNullWhen(true)] out LanguagePrototype? prototype)
    {
        return _prototype.TryIndex(id, out prototype);
    }

    public void OnRadioLanguageCheck(EntityUid source, LanguagesComponent comp, ref RadioLanguageCheckEvent args)
    {
        var passability = CheckRadioPass(source, args.Language);

        if (passability == 0)
        {
            args.Cancelled = true;

            var time = _timing.CurTime;
            if (time > comp.LastPopup + comp.PopupCooldown)
            {
                comp.LastPopup = time;
                var message = Loc.GetString("languages-radio-block");

                _popup.PopupEntity(message, source, source);
            }

        }

        else if (passability < 1)
        {
            args.Message = ObfuscateMessageReadability(args.Message, 1.0f - passability);

            var time = _timing.CurTime;
            if (time > comp.LastPopup + comp.PopupCooldown)
            {
                comp.LastPopup = time;
                var message = Loc.GetString("languages-radio-part-pass");

                _popup.PopupEntity(message, source, source);
            }
        }
    }

    public string ObfuscateMessage(string message, ProtoId<LanguagePrototype> language)
    {
        if (!TryGetLanguagePrototype(language, out var prototype))
            return message;

        var obfuscated = prototype.Obfuscation.Obfuscate(message, _ticker.RoundId);

        return SanitizeMessage(obfuscated);
    }

    public string ObfuscateMessageForLevel(
        string message,
        ProtoId<LanguagePrototype> language,
        LanguageLevel languageLevel)
    {
        if (!ProtoMan.TryIndex(language, out var prototype))
            return message;

        if (languageLevel == LanguageLevel.Full)
            return message;

        if (languageLevel == LanguageLevel.None)
            return ObfuscateFully(message, prototype);

        return ObfuscatePartially(message, prototype, languageLevel);
    }

    private string ObfuscateFully(
        string message,
        LanguagePrototype prototype)
    {
        var obfuscated = prototype.Obfuscation.Obfuscate(
            message,
            _ticker.RoundId);

        return SanitizeMessage(obfuscated);
    }

    private string ObfuscatePartially(
        string message,
        LanguagePrototype prototype,
        LanguageLevel level)
    {
        var chance = GetLanguageObfuscationChance(level);

        var words = message.Split(' ');
        var result = new StringBuilder();

        foreach (var word in words)
        {
            if (word.Length == 0)
                continue;

            if (_random.Prob(chance))
            {
                result.Append(
                    prototype.Obfuscation.Obfuscate(
                        word,
                        _ticker.RoundId));
            }
            else
            {
                result.Append(word);
            }

            result.Append(' ');
        }

        return SanitizeMessage(result.ToString());
    }

    public string ObfuscateMessageForListener(
        string message,
        ProtoId<LanguagePrototype> language,
        EntityUid listener)
    {
        var level = GetLanguageLevel(listener, language);

        return ObfuscateMessageForLevel(message, language, level);
    }

    public bool TrySetLanguage(EntityUid uid, ProtoId<LanguagePrototype> protoId)
    {
        if (!TryComp<LanguagesComponent>(uid, out var comp))
            return false;

        if (!comp.Languages.TryGetValue(protoId, out var level) ||
                level < LanguageLevelSpeak)
            return false;

        comp.CurrentLanguage = protoId;
        Dirty(uid, comp);
        UpdateLanguagesWindow(uid);

        return true;
    }

    protected virtual void UpdateLanguagesWindow(EntityUid uid) { }

    /// <summary>
    /// На основе префикса
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public LanguagePrototype? GetLanguagePrototype(EntityUid uid, string? message = null)
    {
        if (!TryComp<LanguagesComponent>(uid, out var comp))
            return null;

        if (string.IsNullOrEmpty(message) || message.Length < 2 || !message.StartsWith(LanguagePrefix))
        {
            return GetLanguagePrototype(comp.CurrentLanguage);
        }

        var prefix = char.ToLower(message[1]);

        return _keylan.TryGetValue(prefix, out var language)
            ? language : null;
    }

    public bool TryProcessLanguageMessage(
        EntityUid source,
        string message,
        out string newMessage,
        [NotNullWhen(true)] out ProtoId<LanguagePrototype>? languageId)
    {
        newMessage = message.Trim();
        languageId = null;

        if (string.IsNullOrWhiteSpace(message))
            return false;

        if (!TryComp<LanguagesComponent>(source, out var comp))
        {
            languageId = DefaultLanguage;
            return true;
        }

        if (!comp.CanSpeak)
        {
            languageId = UnknownLanguage;
            return true;
        }

        if (message.StartsWith(LanguagePrefix))
        {
            if (message.Length <= 2 || char.IsWhiteSpace(message[1]))
            {
                _popup.PopupEntity(
                    Loc.GetString("chat-manager-no-language-key"),
                    source,
                    source);

                return false;
            }

            var prefix = char.ToLower(message[1]);

            if (!_keylan.TryGetValue(prefix, out var language))
            {
                _popup.PopupEntity(
                    Loc.GetString(
                        "chat-manager-no-such-language",
                        ("key", prefix)),
                    source,
                    source);

                return false;
            }

            languageId = language.ID;

            newMessage = SanitizeMessage(message[2..].TrimStart());
        }
        else
            languageId = comp.CurrentLanguage;

        if (languageId is not {} protoId ||
                !comp.Languages.TryGetValue(protoId, out var level) ||
                level < LanguageLevelSpeak)
        {
            _popup.PopupEntity(
                Loc.GetString("languages-cannot-speak"),
                source,
                source);

            return false;
        }

        return true;
    }

    private float CheckRadioPass(EntityUid source, ProtoId<LanguagePrototype> langId)
    {
        if (!ProtoMan.TryIndex(langId, out var language))
            return 1.0f;

        if (_statusEffects.HasEffectComp<MutedStatusEffectComponent>(source))
            return 0f;

        return language.RadioPass;
    }

    public string ObfuscateMessageReadability(string message, float chance)
    {
        var modifiedMessage = new StringBuilder(message);

        for (var i = 0; i < message.Length; i++)
        {
            if (char.IsWhiteSpace(modifiedMessage[i]))
                continue;

            if (_random.Prob(1 - chance))
                modifiedMessage[i] = '~';
        }

        return modifiedMessage.ToString();
    }

    public string SanitizeMessage(string message, bool capitalize = true)
    {
        var newMessage = message.Trim();

        if (capitalize)
            newMessage = _chat.SanitizeMessageCapital(newMessage);

        return newMessage ?? "";
    }

    public LanguageLevel GetLanguageLevel(EntityUid entity, ProtoId<LanguagePrototype> language)
    {
        if (!TryComp<LanguagesComponent>(entity, out var comp))
            return language == DefaultLanguage ? LanguageLevel.Full : LanguageLevel.None;

        if (!comp.Languages.TryGetValue(language, out var level))
            return LanguageLevel.None;

        return level;
    }

    public float GetLanguageObfuscationChance(LanguageLevel level)
    {
        if (!ObfuscationLevels.TryGetValue(level, out var chance))
            return 1f;

        return chance;
    }
}
