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
    private bool _pendingLanguagesUpdate;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LanguagesComponent, AfterAutoHandleStateEvent>(OnLanguagesState);
        SubscribeLocalEvent<BorgLanguageModuleComponent, AfterAutoHandleStateEvent>(OnModuleState);
    }

    private void OnLanguagesState(Entity<LanguagesComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (_players.LocalEntity == ent.Owner)
            _pendingLanguagesUpdate = true;
    }

    private void OnModuleState(Entity<BorgLanguageModuleComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        _pendingLanguagesUpdate = true;
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);
        if (!_pendingLanguagesUpdate)
            return;
        _pendingLanguagesUpdate = false;
        if (_players.LocalEntity is { } entity)
            UpdateLanguagesWindow(entity);
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
