using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Enums;
using Content.Shared.Dataset;
using Content.Shared.Random;
using System.Text;
using Content.Shared.Random.Helpers;

namespace Content.Shared.Humanoid
{
    /// <summary>
    /// Figure out how to name a humanoid with these extensions.
    /// </summary>
    public sealed class NamingSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        //WL-Changes-start
        public string GetName(string species, Gender gender = Gender.Male)
        {
            // if they have an old species or whatever just fall back to human I guess?
            // Some downstream is probably gonna have this eventually but then they can deal with fallbacks.
            if (!_prototypeManager.TryIndex(species, out SpeciesPrototype? speciesProto))
            {
                speciesProto = _prototypeManager.Index<SpeciesPrototype>("Human");
                Log.Warning($"Unable to find species {species} for name, falling back to Human");
            }

            if (speciesProto.Naming.TryGetValue(gender, out var list))
                return GetName(list);
            else
                return GetName(speciesProto.Naming[Gender.Male]);
        }

        public string GetName(List<string> values)
        {
            var content = new StringBuilder();

            foreach (var value in values)
            {
                var dataset = _prototypeManager.TryIndex<DatasetPrototype>(value, out var datasetProto);
                var weighted = _prototypeManager.TryIndex<WeightedRandomPrototype>(value, out var weightedProto);

                if (dataset && weighted)
                {
                    Log.Error($"{nameof(NamingSystem)}: При выборе имени, ID '{value}' было найдено как в {nameof(DatasetPrototype)}, так и {nameof(WeightedRandomPrototype)}.");
                    return "error";
                }

                if (datasetProto != null)
                    content.Append(_random.Pick(datasetProto));
                else if (weightedProto != null)
                    content.Append(weightedProto.Pick(_random));
                else content.Append(value);
            }

            return content.ToString();
        }
        //WL-Changes-end
    }
}
