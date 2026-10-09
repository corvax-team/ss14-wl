using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Languages.Components;

[RegisterComponent]
public sealed partial class ModifyLanguagesComponent : Component
{
    /// <summary>
    /// Список языков, на которые влияет этот модификатор.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<LanguagePrototype>, LanguageLevel> Languages = [];
}
