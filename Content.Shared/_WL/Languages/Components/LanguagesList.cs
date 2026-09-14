using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Languages.Components.List;

[DataDefinition, Serializable, NetSerializable]
public sealed partial class LanguagesList
{
    /// <summary>
    /// UwU
    /// </summary>
    [DataField(required: true)]
    public ProtoId<LanguagePrototype> Language; // ссылка на прототип языка

    /// <summary>
    /// 0 — вообще не понимает
    /// 1 — плохо понимает
    /// 2 — понимает с ошибками
    /// 3 — хорошо понимает и говорит
    /// 4 — свободно говорит и понимает полностью
    /// </summary>
    [DataField]
    public int LanguageLevel = 0;

}
