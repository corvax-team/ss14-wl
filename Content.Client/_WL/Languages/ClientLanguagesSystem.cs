using System.Linq;
using Content.Shared._WL.Languages;
using Content.Shared._WL.Languages.Components;
using Content.Shared._WL.Languages.Components.List;

namespace Content.Client._WL.Languages;

public sealed partial class ClientLanguagesSystem : SharedLanguagesSystem
{
    [Dependency] private IEntityManager _ent = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<LanguagesInfoEvent>(OnLanguagesInfoEvent);
        SubscribeNetworkEvent<LanguageChangeEvent>(OnGlobalLanguageChange);
        SubscribeNetworkEvent<LanguagesSyncEvent>(OnLanguagesSync);

        SubscribeLocalEvent<LanguagesComponent, ComponentInit>(OnLanguageCommponentSyns);
        SubscribeLocalEvent<LanguagesComponent, LanguageChangeEvent>(OnLocalLanguageChange);
    }

    public event Action<LanguagesData>? OnLanguagesUpdate;

    public void OnLanguageCommponentSyns(EntityUid entity, LanguagesComponent comp, ComponentInit args)
    {
        var net_ent = GetNetEntity(entity);
        var language = comp.List.FirstOrDefault();

        if (language == null)
            return;

        var ev = new LanguageSyncRequestEvent(
            net_ent,
            language.Language,
            language.LanguageLevel,
            comp.List);

        RaiseNetworkEvent(ev);
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

        component.List = msg.List;

        Dirty(entity, component);
    }

    private void OnLanguagesInfoEvent(LanguagesInfoEvent msg, EntitySessionEventArgs args)
    {
        var entity = GetEntity(msg.NetEntity);
        var data = new LanguagesData(entity, msg.CurrentLanguage, msg.List);

        OnLanguagesUpdate?.Invoke(data);
    }
}

public readonly record struct LanguagesData(
    EntityUid Entity,
    string? CurrentLanguage,
    List<LanguagesList> List
);
