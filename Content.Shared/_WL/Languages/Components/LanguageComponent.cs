using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Shared._WL.Languages.Components;

[RegisterComponent, NetworkedComponent]
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
    public string CurrentLanguage = "";

    [Serializable, NetSerializable]
    public sealed class State : ComponentState
    {
        public bool IsUnderstanding = default!;
        public bool IsSpeaking = default!;
        public string CurrentLanguage = default!;
        public List<ProtoId<LanguagePrototype>> Speaking= default!;
        public List<ProtoId<LanguagePrototype>> Understood = default!;
    }
}
