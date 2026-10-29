using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Languages;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BorgLanguageModuleComponent : Component
{
    public const int MaxLanguages = 5;

    [DataField, AutoNetworkedField]
    public bool Configured;

    [DataField, AutoNetworkedField]
    public List<ProtoId<LanguagePrototype>> Languages = new();
}

[Serializable, NetSerializable]
public enum BorgLanguageModuleUiKey : byte { Key }

[Serializable, NetSerializable]
public sealed class BorgLanguageModuleState(List<ProtoId<LanguagePrototype>> available,
    List<ProtoId<LanguagePrototype>> selected, bool configured) : BoundUserInterfaceState
{
    public readonly List<ProtoId<LanguagePrototype>> Available = available;
    public readonly List<ProtoId<LanguagePrototype>> Selected = selected;
    public readonly bool Configured = configured;
}

[Serializable, NetSerializable]
public sealed class BorgLanguageModuleConfirmMessage(List<ProtoId<LanguagePrototype>> languages) : BoundUserInterfaceMessage
{
    public readonly List<ProtoId<LanguagePrototype>> Languages = languages;
}
