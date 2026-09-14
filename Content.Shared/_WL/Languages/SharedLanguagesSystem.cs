using Content.Shared._WL.Languages.Components;
using Content.Shared.Chat;
using Content.Shared.GameTicking;
using Content.Shared.Popups;
using Content.Shared.Speech.Muting;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Linq;
using Content.Shared._WL.Languages.Components.List;

namespace Content.Shared._WL.Languages;

public abstract partial class SharedLanguagesSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private SharedGameTicker _ticker = default!;
    [Dependency] private IEntityManager _ent = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedChatSystem _chat = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;

    private FrozenDictionary<char, LanguagePrototype> _keylan = default!;

    const char LanguagePrefix = '+';

    public const int LanguageLevelNone = 0;
    public const int LanguageLevelBasic = 1;
    public const int LanguageLevelPartial = 2;
    public const int LanguageLevelSpeak = 3;
    public const int LanguageLevelFull = 4;

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
        var passability = CheckRadioPass(source, args.Message);

        if (passability == 0)
        {
            args.Cancelled = true;

            var time = _timing.CurTime;
            if (time > comp.LastPopup + comp.PopupCooldown)
            {
                comp.LastPopup = time;
                var message = Loc.GetString("languages-radio-block");

                _popup.PopupEntity(message, source);
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

                _popup.PopupEntity(message, source);
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
        int languageLevel)
    {
        if (!TryGetLanguagePrototype(language, out var prototype))
            return message;

        if (languageLevel >= LanguageLevelFull)
            return message;

        if (languageLevel <= LanguageLevelNone)
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
        int languageLevel)
    {
        var chance = GetLanguageObfuscationChance(languageLevel);

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

    public bool TryChangeLanguage(NetEntity netEnt, ProtoId<LanguagePrototype> protoId)
    {
        if (!_ent.TryGetEntity(netEnt, out var ent))
            return false;

        if (!TryComp<LanguagesComponent>(ent, out var comp))
            return false;

        var entry = comp.List.FirstOrDefault(x => x.Language == protoId);

        if (entry == null || entry.LanguageLevel < LanguageLevelSpeak)
            return false;

        comp.CurrentLanguage = protoId;
        Dirty(ent.Value, comp);

        var ev = new LanguageChangeEvent(netEnt, protoId);
        RaiseNetworkEvent(ev);
        RaiseLocalEvent(ent.Value, ev);

        var ev2 = new LanguagesInfoEvent(netEnt, (string)protoId, comp.List);
        RaiseNetworkEvent(ev2);

        return true;
    }

    public void SyncLanguages(NetEntity netEnt, LanguagesComponent comp)
    {
        var ev = new LanguagesSyncEvent(netEnt, comp.List);
        RaiseNetworkEvent(ev);
    }

    public void OnLanguageChange(EntityUid entity, string language)
    {
        if (!TryComp<LanguagesComponent>(entity, out var component))
            return;

        component.CurrentLanguage = language;
        Dirty(entity, component);

        var netEntity = GetNetEntity(entity);
        var ev = new LanguagesInfoEvent(netEntity, language, component.List);
        RaiseNetworkEvent(ev);
    }

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
        out string newMessage)
    {
        newMessage = _chat.SanitizeMessageCapital(message.Trim());

        if (string.IsNullOrWhiteSpace(message))
            return false;

        if (!TryComp<LanguagesComponent>(source, out var comp))
            return false;

        ProtoId<LanguagePrototype>? languageId;

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

            newMessage = _chat.SanitizeMessageCapital(
                message[2..].TrimStart());
        }
        else
        {
            languageId = comp.CurrentLanguage;

            if (languageId == null)
                return false;
        }

        var languageData = comp.List.FirstOrDefault(x => x.Language == languageId);

        if (languageData == null)
            return false;

        if (languageData.LanguageLevel < LanguageLevelSpeak)
        {
            _popup.PopupEntity(
                Loc.GetString("languages-cannot-speak"),
                source,
                source);

            return false;
        }
        return true;
    }

    private float CheckRadioPass(EntityUid source, string msg)
    {
        var language = GetLanguagePrototype(source, msg);

        if (HasComp<MutedComponent>(source))
            return 0f;

        if (language == null)
            return 1.0f;

        return language.RadioPass;
    }

    public string ObfuscateMessageReadability(string message, float chance)
    {
        var modifiedMessage = new StringBuilder(message);

        for (var i = 0; i < message.Length; i++)
        {
            if (char.IsWhiteSpace(modifiedMessage[i]))
            {
                continue;
            }

            if (_random.Prob(1 - chance))
            {
                modifiedMessage[i] = '~';
            }
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

    public int GetLanguageLevel(EntityUid entity, ProtoId<LanguagePrototype> language)
    {
        if (!TryComp<LanguagesComponent>(entity, out var comp))
            return 0;

        var entry = comp.List.FirstOrDefault(x => x.Language == language);

        return entry?.LanguageLevel ?? 0;
    }

    public float GetLanguageObfuscationChance(int level)
    {
        return Math.Clamp(level, LanguageLevelNone, LanguageLevelFull) switch
        {
            LanguageLevelNone => 1.00f,
            LanguageLevelBasic => 0.90f,
            LanguageLevelPartial => 0.60f,
            LanguageLevelSpeak => 0.30f,
            LanguageLevelFull => 0.00f,
            _ => 1.00f
        };
    }

    [Serializable, NetSerializable]
    public sealed class LanguagesInfoEvent : EntityEventArgs
    {
        public readonly NetEntity NetEntity;
        public readonly string CurrentLanguage;
        public readonly List<LanguagesList> List;

        public LanguagesInfoEvent(NetEntity netEntity, string current, List<LanguagesList> list)
        {
            NetEntity = netEntity;
            CurrentLanguage = current;
            List = list;
        }
    }
}
