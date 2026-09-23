using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Languages.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState] // ← Добавили автогенерацию
public sealed partial class ModifyLanguagesComponent : Component
{
    /// <summary>
    /// Если true, то компонент будет удалять указанные языки, а не добавлять их.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ToRemove = false;

    /// <summary>
    /// Уровень владения языками, который даст этот модификатор.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int LanguageLevel = 0;

    /// <summary>
    /// Если указан, то этот язык будет установлен как родной.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool SpecieLanguage = false;

    /// <summary>
    /// Список языков, на которые влияет этот модификатор.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<ProtoId<LanguagePrototype>> Languages = [];
}
