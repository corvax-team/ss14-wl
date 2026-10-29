using System.Linq;
using Content.Shared._WL.Languages;
using Content.Shared._WL.Languages.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Client.Player;

namespace Content.Client._WL.Languages;

public sealed partial class ClientLanguagesSystem : SharedLanguagesSystem
{
    [Dependency] private IEntityManager _entMan = default!;
    [Dependency] private IPlayerManager _players = default!;

    public event Action? OnLanguagesUpdate;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LanguagesComponent, AfterAutoHandleStateEvent>(OnLanguagesState);
    }

    private void OnLanguagesState(Entity<LanguagesComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (_players.LocalEntity == ent.Owner)
            UpdateLanguagesWindow(ent);
    }

    protected override void UpdateLanguagesWindow(EntityUid uid)
    {
        OnLanguagesUpdate?.Invoke();
    }

    public void SetLanguage(EntityUid uid, ProtoId<LanguagePrototype> language)
    {
        if (!TrySetLanguage(uid, language))
            return;

        if (!_entMan.TryGetNetEntity(uid, out var netEntity))
            return;

        if (netEntity is {} netEnt)
            RaiseNetworkEvent(new SetLanguageEvent(netEnt, language));
    }
}
