using System.Collections.Generic;
using Content.Shared._WL.Languages.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._WL.Languages
{
    /// <summary>
    /// Helper for aggregating multiple ModifyLanguagesComponent instances into one.
    /// Exposed in Shared so tests can validate aggregation logic.
    /// </summary>
    public static class ModifyLanguagesAggregator
    {
        public static ModifyLanguagesComponent Aggregate(IEnumerable<ModifyLanguagesComponent> components)
        {
            var aggLanguages = new HashSet<ProtoId<LanguagePrototype>>();
            var aggToRemove = false;
            var aggToUnderstood = false;
            var aggToSpeaking = false;
            var aggSpecieLanguage = false;

            foreach (var comp in components)
            {
                if (comp == null)
                    continue;

                aggToRemove |= comp.ToRemove;
                aggToUnderstood |= comp.ToUnderstood;
                aggToSpeaking |= comp.ToSpeaking;
                aggSpecieLanguage |= comp.SpecieLanguage;

                foreach (var l in comp.Languages)
                    aggLanguages.Add(l);
            }

            var result = new ModifyLanguagesComponent();
            result.ToRemove = aggToRemove;
            result.ToUnderstood = aggToUnderstood;
            result.ToSpeaking = aggToSpeaking;
            result.SpecieLanguage = aggSpecieLanguage;

            foreach (var l in aggLanguages)
                result.Languages.Add(l);

            return result;
        }
    }
}
