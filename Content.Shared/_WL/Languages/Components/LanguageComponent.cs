using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Languages.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LanguagesComponent : Component
{
    [DataField]
    public bool IsUnderstanding = true;

    [DataField]
    public bool IsSpeaking = true;

    [DataField]
    public List<ProtoId<LanguagePrototype>> Speaking = [];

    [DataField]
    public List<ProtoId<LanguagePrototype>> Understood = [];

    [DataField]
    public ProtoId<LanguagePrototype>? CurrentLanguage = null;

    [DataField]
    public ProtoId<LanguagePrototype>? SpecieLanguage = null;

    [DataField, AutoNetworkedField]
    public TimeSpan LastPopup;

    [DataField, AutoNetworkedField]
    public TimeSpan PopupCooldown = TimeSpan.FromSeconds(1);

    [Serializable, NetSerializable]
    public sealed class State : ComponentState
    {
        public bool IsUnderstanding = default!;
        public bool IsSpeaking = default!;
        public ProtoId<LanguagePrototype>? CurrentLanguage = null;
        public List<ProtoId<LanguagePrototype>> Speaking = default!;
        public List<ProtoId<LanguagePrototype>> Understood = default!;
    }
}
