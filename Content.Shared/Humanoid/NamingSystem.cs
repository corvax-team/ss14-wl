using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Random.Helpers;
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
                case Gender.Male:
                    return _random.Pick(_prototypeManager.Index<LocalizedDatasetPrototype>(speciesProto.MaleFirstNames));
                case Gender.Female:
                    return _random.Pick(_prototypeManager.Index<LocalizedDatasetPrototype>(speciesProto.FemaleFirstNames));
                default:
                    if (_random.Prob(0.5f))
                        return _random.Pick(_prototypeManager.Index<LocalizedDatasetPrototype>(speciesProto.MaleFirstNames));
                    else
                        return _random.Pick(_prototypeManager.Index<LocalizedDatasetPrototype>(speciesProto.FemaleFirstNames));
            }

        // Corvax-LastnameGender-Start: Added custom gender split logic
        public string GetLastName(SpeciesPrototype speciesProto, Gender? gender = null)
        {
            switch (gender)
            {
                case Gender.Male:
                    return _random.Pick(_prototypeManager.Index<LocalizedDatasetPrototype>(speciesProto.MaleLastNames));
                case Gender.Female:
                    return _random.Pick(_prototypeManager.Index<LocalizedDatasetPrototype>(speciesProto.FemaleLastNames));
                default:
                    if (_random.Prob(0.5f))
                        return _random.Pick(_prototypeManager.Index<LocalizedDatasetPrototype>(speciesProto.MaleLastNames));
                    else
                        return _random.Pick(_prototypeManager.Index<LocalizedDatasetPrototype>(speciesProto.FemaleLastNames));
            }
        }
        //WL-Changes-end
    }
}
