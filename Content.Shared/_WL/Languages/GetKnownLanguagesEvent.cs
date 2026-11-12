using Content.Shared._WL.Languages.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Languages;

[ByRefEvent]
public readonly record struct GetKnownLanguagesEvent(Dictionary<ProtoId<LanguagePrototype>, LanguageLevel> Languages)
{
    public void AddLanguage(ProtoId<LanguagePrototype> language, LanguageLevel level)
    {
        if (level > Languages.GetValueOrDefault(language))
            Languages[language] = level;
    }
}
