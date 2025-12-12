using Content.Shared._WL.Languages;
using Content.Shared._WL.Languages.Components;
using Robust.Shared.Prototypes;

namespace Content.Client._WL.Languages;

public sealed partial class ClientLanguagesSystem : SharedLanguagesSystem
{
    [Dependency] private readonly IEntityManager _ent = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<LanguagesInfoEvent>(OnLanguagesInfoEvent);
        SubscribeNetworkEvent<LanguageChangeEvent>(OnGlobalLanguageChange);
        SubscribeNetworkEvent<LanguagesSyncEvent>(OnLanguagesSync);

        SubscribeLocalEvent<LanguagesComponent, LanguageChangeEvent>(OnLocalLanguageChange);
    }

    public event Action<LanguagesData>? OnLanguagesUpdate;

    public List<LanguagePrototype>? GetSpeakingLanguages(EntityUid entity)
    {
        if (!TryComp<LanguagesComponent>(entity, out var comp))
            return null;

        var prototypes = new List<LanguagePrototype>();
        foreach (ProtoId<LanguagePrototype>protoid in comp.Speaking)
        {
            var proto = GetLanguagePrototype(protoid);
            if (proto == null)
                continue;
            prototypes.Add(proto);
        }

        if (prototypes.Count == 0)
            return null;
        return prototypes;
    }

    public void OnLocalLanguageChange(EntityUid entity, LanguagesComponent comp, ref LanguageChangeEvent args)
    {
        OnLanguageChange(entity, (string)args.Language);
    }

    public void OnGlobalLanguageChange(LanguageChangeEvent msg, EntitySessionEventArgs args)
    {
        var entity = GetEntity(msg.Entity);
        OnLanguageChange(entity, (string)msg.Language);
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

    private void OnLanguagesInfoEvent(LanguagesInfoEvent msg, EntitySessionEventArgs args)
    {
        var entity = GetEntity(msg.NetEntity);
        var data = new LanguagesData(entity, msg.CurrentLanguage, msg.Speaking, msg.Understood);

        OnLanguagesUpdate?.Invoke(data);
    }
}

public readonly record struct LanguagesData(
    EntityUid Entity,
    string? CurrentLanguage,
    List<ProtoId<LanguagePrototype>> Speaking,
    List<ProtoId<LanguagePrototype>> Understood
);
