using Content.Shared._WL.Languages.Components;
using Content.Shared.GameTicking;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Languages;

public abstract class SharedLanguagesSystem : EntitySystem
{
    [Dependency] protected readonly IPrototypeManager _prototype = default!;
    [Dependency] protected readonly SharedGameTicker _ticker = default!;
    [Dependency] private readonly IEntityManager _ent = default!;

    public LanguagePrototype? GetLanguagePrototype(ProtoId<LanguagePrototype> id)
    {
        _prototype.TryIndex(id, out var proto);
        return proto;
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
