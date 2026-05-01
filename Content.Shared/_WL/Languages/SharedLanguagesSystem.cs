using Content.Shared._WL.Languages.Components;
using Content.Shared.Chat;
using Content.Shared.GameTicking;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Content.Shared._WL.Languages;

public abstract class SharedLanguagesSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedGameTicker _ticker = default!;
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private FrozenDictionary<char, LanguagePrototype> _keylan = default!;

    const char LanguagePrefix = '+';

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


    /// <summary>
    /// на основе айди прототипа
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(id))]
    public LanguagePrototype? GetLanguagePrototype(ProtoId<LanguagePrototype>? id)
    {
        _prototype.TryIndex(id, out var proto);
        return proto;
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
        var proto = GetLanguagePrototype(language);

        if (proto == null)
        {
            return message;
        }
        else
        {
            var obfus = proto.Obfuscation.Obfuscate(message, _ticker.RoundId);
            return obfus;
        }
    }

    public bool TryChangeLanguage(NetEntity netEnt, ProtoId<LanguagePrototype> protoid)
    {
        if (!_ent.TryGetEntity(netEnt, out var ent))
            return false;

        if (!TryComp<LanguagesComponent>(ent, out var comp))
            return false;
        if (!comp.Understood.Contains(protoid))
            return false;

        if (ent == null)
            return false;

        comp.CurrentLanguage = protoid;
        Dirty(ent.Value, comp);

        var ev = new LanguageChangeEvent(netEnt, protoid);
        RaiseNetworkEvent(ev);
        RaiseLocalEvent(ent.Value, ev);

        var ev2 = new LanguagesInfoEvent(netEnt, (string)protoid, comp.Speaking, comp.Understood);
        RaiseNetworkEvent(ev2);

        return true;
    }

    public void SyncLanguages(NetEntity netEnt, LanguagesComponent comp)
    {
        var ev = new LanguagesSyncEvent(netEnt, comp.Speaking, comp.Understood);
        RaiseNetworkEvent(ev);
    }

    public void OnLanguageChange(EntityUid entity, string language)
    {
        if (!TryComp<LanguagesComponent>(entity, out var component))
            return;

        component.CurrentLanguage = language;
        Dirty(entity, component);

        var netEntity = GetNetEntity(entity);
        var ev = new LanguagesInfoEvent(netEntity, language, component.Speaking, component.Understood);
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

    public bool TryProcessLanguageMessage(EntityUid source, string message, out string new_message)
    {
        new_message = message.Trim();

        if (message.Length == 0)
            return false;

        if (TryComp<LanguagesComponent>(source, out var comp))
        {
            if (!message.StartsWith(LanguagePrefix))
                return false;

            if (message.Length <= 2 || char.IsWhiteSpace(message[1]))
            {
                new_message = string.Empty;
                _popup.PopupEntity(Loc.GetString("chat-manager-no-language-key"), source, source);
                return false;
            }

            var prefix = message[1];
            prefix = char.ToLower(prefix);
            new_message = _chat.SanitizeMessageCapital(message[2..].TrimStart());

            if (!_keylan.TryGetValue(prefix, out LanguagePrototype? language))
            {
                var msg = Loc.GetString("chat-manager-no-such-language", ("key", prefix));
                new_message = string.Empty;
                _popup.PopupEntity(msg, source, source);
                return false;
            }
            if (language != null)
            {
                if (comp.Speaking.Contains(language.ID))
                    return true;

                new_message = string.Empty;
                return false;
            }
        }

        return false;
    }

    private float CheckRadioPass(EntityUid source, string msg)
    {
        var language = GetLanguagePrototype(source, msg);

        if (language == null)
            return 1.0f;

        return language.RadioPass;
    }

    public string ObfuscateMessageReadability(string message, float chance)
    {
        var modifiedMessage = new StringBuilder(message);

        for (var i = 0; i < message.Length; i++)
        {
            if (char.IsWhiteSpace((modifiedMessage[i])))
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

    public string SanitizeWrappedMessage(EntityUid source, string message, bool capitalize = true)
    {
        var newMessage = message.Trim();

        if (capitalize)
            newMessage = _chat.SanitizeMessageCapital(newMessage);

        return newMessage ?? "";
    }

    [Serializable, NetSerializable]
    public sealed class LanguagesInfoEvent : EntityEventArgs
    {
        public readonly NetEntity NetEntity;
        public readonly string CurrentLanguage;
        public readonly List<ProtoId<LanguagePrototype>> Speaking;
        public readonly List<ProtoId<LanguagePrototype>> Understood;

        public LanguagesInfoEvent(NetEntity netEntity, string current, List<ProtoId<LanguagePrototype>> speeking, List<ProtoId<LanguagePrototype>> understood)
        {
            NetEntity = netEntity;
            CurrentLanguage = current;
            Speaking = speeking;
            Understood = understood;
        }
    }
}
