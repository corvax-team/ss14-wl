using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Languages;

/// <summary>
/// One utterance shared by local speech and radio, with one comprehension result per listener.
/// </summary>
public sealed class LanguageMessageContext(ProtoId<LanguagePrototype> language, string message)
{
    public readonly ProtoId<LanguagePrototype> Language = language;
    public readonly string Message = message;
    public readonly Dictionary<EntityUid, string> UnderstoodMessages = new();
}
