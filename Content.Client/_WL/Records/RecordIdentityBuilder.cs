using System.Linq;
using Content.Shared._WL.Languages;
using Content.Shared._WL.Records;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.StationRecords;
using Robust.Shared.Prototypes;

namespace Content.Client._WL.Records;

public static class RecordIdentityBuilder
{
    public static RecordIdentityData FromStationRecord(
        GeneralStationRecord record,
        IPrototypeManager prototypes,
        Func<string, string> loc)
    {
        var noData = loc("records-value-no-data");
        var species = prototypes.TryIndex<SpeciesPrototype>(record.Species, out var speciesPrototype)
            ? loc(speciesPrototype.Name)
            : noData;
        var confederation = prototypes.TryIndex<ConfederationRecordsPrototype>(record.Confederation, out var confederationPrototype)
            ? loc(confederationPrototype.Name)
            : noData;
        var languages = record.Languages
            .Select(id => prototypes.TryIndex(id, out var language) ? loc(language.Name) : id.Id)
            .ToList();

        return new RecordIdentityData(
            record.Name,
            record.JobTitle,
            record.Fingerprint ?? noData,
            record.DNA ?? noData,
            string.IsNullOrWhiteSpace(record.Fullname) ? record.Name : record.Fullname,
            loc(record.Species is "Ipc" or "Android"
                ? "records-date-of-manufacture-edit"
                : "records-date-of-birth-edit"),
            string.IsNullOrWhiteSpace(record.DateOfBirth) ? noData : record.DateOfBirth,
            loc($"humanoid-profile-editor-sex-{record.Sex.ToString().ToLowerInvariant()}-text"),
            species,
            $"{record.Height} {loc("records-height-unit-centimeters")}",
            languages.Count == 0 ? noData : string.Join(", ", languages),
            confederation,
            string.IsNullOrWhiteSpace(record.Country) ? noData : record.Country);
    }
}
