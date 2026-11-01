using Content.Shared._WL.Languages;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Languages;

/// <summary>
/// Final chat text for each listener, shared with speech synthesis without rerolling comprehension.
/// </summary>
public sealed class LanguageSpeechEvent(
    ProtoId<LanguagePrototype> language,
    Dictionary<EntityUid, string> messages,
    bool whisper = false) : EntityEventArgs
{
    public readonly ProtoId<LanguagePrototype> Language = language;
    public readonly Dictionary<EntityUid, string> Messages = messages;
    public readonly bool Whisper = whisper;
}
