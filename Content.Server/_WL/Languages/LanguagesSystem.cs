using Content.Shared._WL.Languages;
using Content.Shared._WL.Languages.Components;
using Content.Shared.Chat;
using Content.Shared.Radio;
using Content.Shared.Speech;
using Content.Shared.IdentityManagement;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Languages;

public sealed class LanguagesSystem : SharedLanguagesSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IEntityManager _ent = default!;

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
        if (!TryComp<LanguagesComponent>(source, out var source_lang))
            return message;
        else
        {
            var message_language = source_lang.CurrentLanguage;
            var obfus = ObfuscateMessage(message, message_language);
            return obfus;
        }
    }

    public bool CanUnderstand(EntityUid source, EntityUid listener)
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

        var message_language = source_lang.CurrentLanguage;
        return listen_lang.IsUnderstanding && source_lang.IsSpeaking && listen_lang.Understood.Contains(message_language);
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

    public string GetObfusWrappedMessage(string message, EntityUid source, string name, SpeechVerbPrototype? speech = null)
    {
        var obfusMessage = ObfuscateMessageFromSource(message, source);
        if (IsObfusEmoting(source))
        {
            var ent = Identity.Entity(source, EntityManager);
            var wrappedMessage = Loc.GetString("chat-manager-entity-me-wrap-message",
                ("entityName", name),
                ("entity", ent),
                ("message", FormattedMessage.RemoveMarkupOrThrow(obfusMessage))
            );
            return wrappedMessage;
        }
        else
        {
            if (speech == null)
            {
                var wrappedobfuscatedMessage = Loc.GetString("chat-manager-entity-whisper-wrap-message",("entityName", name), ("message", FormattedMessage.EscapeText(obfusMessage)));
                return wrappedobfuscatedMessage;
            }
            else
            {
                var wrappedMessage = Loc.GetString(speech.Bold ? "chat-manager-entity-say-bold-wrap-message" : "chat-manager-entity-say-wrap-message",
                        ("entityName", name),
                        ("verb", Loc.GetString(_random.Pick(speech.SpeechVerbStrings))),
                        ("fontType", speech.FontId),
                        ("fontSize", speech.FontSize),
                        ("message", FormattedMessage.EscapeText(obfusMessage)
                    )
                );
                return wrappedMessage;
            }
        }
    }

    public string GetRadioObfusWrappedMessage(string message, EntityUid source, string name, SpeechVerbPrototype speech, RadioChannelPrototype channel)
    {
        var obfusMessage = ObfuscateMessageFromSource(message, source);
        var wrappedMessage = Loc.GetString(speech.Bold ? "chat-radio-message-wrap-bold" : "chat-radio-message-wrap",
            ("color", channel.Color),
            ("fontType", speech.FontId),
            ("fontSize", speech.FontSize),
            ("verb", Loc.GetString(_random.Pick(speech.SpeechVerbStrings))),
            ("channel", $"\\[{channel.LocalizedName}\\]"),
            ("name", name),
            ("message", FormattedMessage.EscapeText(obfusMessage)));
        return wrappedMessage;
    }
}
