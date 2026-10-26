using System.Linq;
using Content.Shared._WL.Languages;
using Content.Shared._WL.Languages.Components;
using Robust.Shared.Prototypes;

namespace Content.Client._WL.Languages;

public sealed partial class ClientLanguagesSystem : SharedLanguagesSystem
{
    [Dependency] private IEntityManager _entMan = default!;

    public event Action? OnLanguagesUpdate;

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
