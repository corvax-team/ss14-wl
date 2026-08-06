using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Languages.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ModifyLanguagesComponent : Component
{
    [DataField]
    public bool ToRemove = false;

    [DataField]
    public bool ToUnderstood = true;

    [DataField]
    public bool ToSpeaking = true;

    [DataField]
    public bool SpecieLanguage = false;

    // Per-language permissions: lists allow preserving speaking/understood per language.
    [DataField]
    public List<ProtoId<LanguagePrototype>> SpeakingLanguages = [];

    [DataField]
    public List<ProtoId<LanguagePrototype>> UnderstoodLanguages = [];

    [DataField]
    public List<ProtoId<LanguagePrototype>> Languages = [];

    // When ToRemove is set, RemoveLanguages holds the specific languages to remove (aggregated per-component).
    [DataField]
    public List<ProtoId<LanguagePrototype>> RemoveLanguages = [];

    [Serializable, NetSerializable]
    public sealed class State : ComponentState
    {
        public bool ToRemove = default!;
        public bool ToUnderstood = default!;
        public bool ToSpeaking = default!;
        public List<ProtoId<LanguagePrototype>> Languages = default!;
        public List<ProtoId<LanguagePrototype>> RemoveLanguages = default!;
    }
}
