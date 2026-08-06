using System.Collections.Generic;
using Content.Shared._WL.Languages.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Languages
{
    /// <summary>
    /// Helper for aggregating multiple ModifyLanguagesComponent instances into one.
    /// Exposed in Shared so tests can validate aggregation logic and to apply aggregated changes to LanguagesComponent.
    /// </summary>
    public static class ModifyLanguagesAggregator
    {
        public static ModifyLanguagesComponent Aggregate(IEnumerable<ModifyLanguagesComponent> components)
        {
            var understood = new Dictionary<ProtoId<LanguagePrototype>, bool>();
            var speaking = new Dictionary<ProtoId<LanguagePrototype>, bool>();
            var aggToRemove = false;
            var aggSpecieLanguage = false;
            var aggToSpeakingForSpecie = false;
            var aggToUnderstoodForSpecie = false;

            foreach (var comp in components)
            {
                if (comp == null)
                    continue;

                aggToRemove |= comp.ToRemove;
                aggSpecieLanguage |= comp.SpecieLanguage;

                // If component explicitly lists per-language permissions, use them.
                if (comp.SpeakingLanguages?.Count > 0)
                {
                    foreach (var l in comp.SpeakingLanguages)
                        speaking[l] = speaking.GetValueOrDefault(l) || true;
                }

                if (comp.UnderstoodLanguages?.Count > 0)
                {
                    foreach (var l in comp.UnderstoodLanguages)
                        understood[l] = understood.GetValueOrDefault(l) || true;
                }

                // Fallback to old-style component fields: Languages + ToSpeaking/ToUnderstood
                if (comp.Languages?.Count > 0)
                {
                    foreach (var l in comp.Languages)
                    {
                        if (comp.ToSpeaking)
                            speaking[l] = speaking.GetValueOrDefault(l) || true;

                        if (comp.ToUnderstood)
                            understood[l] = understood.GetValueOrDefault(l) || true;
                    }
                }

                // Preserve ToSpeaking/ToUnderstood flags for specie-language handling
                if (comp.SpecieLanguage)
                {
                    aggToSpeakingForSpecie |= comp.ToSpeaking;
                    aggToUnderstoodForSpecie |= comp.ToUnderstood;
                }
            }

            var result = new ModifyLanguagesComponent();
            result.ToRemove = aggToRemove;
            result.SpecieLanguage = aggSpecieLanguage;
            // Use specie-specific aggregated flags for backwards-compatible specie removal behavior
            result.ToSpeaking = aggToSpeakingForSpecie;
            result.ToUnderstood = aggToUnderstoodForSpecie;

            // Populate per-language lists
            if (speaking.Count > 0)
                foreach (var kv in speaking)
                    result.SpeakingLanguages.Add(kv.Key);

            if (understood.Count > 0)
                foreach (var kv in understood)
                    result.UnderstoodLanguages.Add(kv.Key);

            // Also populate Languages as union for compatibility
            var union = new HashSet<ProtoId<LanguagePrototype>>(speaking.Keys);
            union.UnionWith(understood.Keys);
            foreach (var l in union)
                result.Languages.Add(l);

            return result;
        }

        // Apply aggregated ModifyLanguagesComponent to a LanguagesComponent instance.
        public static void ApplyTo(Content.Shared._WL.Languages.Components.LanguagesComponent outComp, ModifyLanguagesComponent component)
        {
            if (outComp == null || component == null)
                return;

            // If ToRemove is set, remove listed languages entirely.
            if (component.ToRemove)
            {
                var toRemove = new HashSet<ProtoId<LanguagePrototype>>();
                if (component.SpeakingLanguages?.Count > 0)
                    toRemove.UnionWith(component.SpeakingLanguages);
                if (component.UnderstoodLanguages?.Count > 0)
                    toRemove.UnionWith(component.UnderstoodLanguages);
                if (component.Languages?.Count > 0)
                    toRemove.UnionWith(component.Languages);

                foreach (var l in toRemove)
                {
                    outComp.Speaking.Remove(l);
                    outComp.Understood.Remove(l);
                }
                return;
            }

            // Add per-language permissions
            if (component.SpeakingLanguages?.Count > 0)
            {
                foreach (var l in component.SpeakingLanguages)
                {
                    if (!outComp.Speaking.Contains(l))
                        outComp.Speaking.Add(l);
                }
            }

            if (component.UnderstoodLanguages?.Count > 0)
            {
                foreach (var l in component.UnderstoodLanguages)
                {
                    if (!outComp.Understood.Contains(l))
                        outComp.Understood.Add(l);
                }
            }

            // Backwards compatibility: if only Languages list present with global flags
            if ((component.SpeakingLanguages == null || component.SpeakingLanguages.Count == 0) &&
                (component.UnderstoodLanguages == null || component.UnderstoodLanguages.Count == 0) &&
                component.Languages?.Count > 0)
            {
                foreach (var l in component.Languages)
                {
                    if (component.ToSpeaking)
                    {
                        if (!outComp.Speaking.Contains(l))
                            outComp.Speaking.Add(l);
                    }

                    if (component.ToUnderstood)
                    {
                        if (!outComp.Understood.Contains(l))
                            outComp.Understood.Add(l);
                    }
                }
            }

            // Handle specie language removal (backwards compatible)
            if (component.SpecieLanguage && outComp.SpecieLanguage != null)
            {
                var protoid = outComp.SpecieLanguage;
                if (protoid != null)
                {
                    if (component.ToSpeaking)
                        outComp.Speaking.Remove(protoid.Value);

                    if (component.ToUnderstood)
                        outComp.Understood.Remove(protoid.Value);
                }
            }
        }
    }
}
