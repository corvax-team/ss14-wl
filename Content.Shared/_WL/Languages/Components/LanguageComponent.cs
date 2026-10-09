using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Content.Shared._WL.Languages.Components;

namespace Content.Shared._WL.Languages.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LanguagesComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<LanguagePrototype>, LanguageLevel> Languages = new();

    [DataField, AutoNetworkedField]
    public ProtoId<LanguagePrototype>? CurrentLanguage = null;

    [DataField, AutoNetworkedField]
    public bool CanSpeak = true;

    [DataField, AutoNetworkedField]
    public bool CanUnderstand = true;

    [DataField, AutoNetworkedField]
    public TimeSpan LastPopup;

    [DataField, AutoNetworkedField]
    public TimeSpan PopupCooldown = TimeSpan.FromSeconds(1);
}

[Serializable, NetSerializable]
public enum LanguageLevel : byte
{
    None = 0,
    Low = 1,
    Medium = 2,
    Good = 3,
    Full = 4
}
