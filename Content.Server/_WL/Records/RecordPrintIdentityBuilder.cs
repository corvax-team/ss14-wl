using System.Linq;
using Content.Shared._WL.Records;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.StationRecords;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Records;

public static class RecordPrintIdentityBuilder
{
    public static RecordPrintIdentity FromStationRecord(
        GeneralStationRecord record,
        IPrototypeManager prototypes,
        Func<string, string> loc)
    {
        var noData = loc("records-value-no-data");
        var languages = string.Join(", ", record.Languages.Select(language =>
            prototypes.TryIndex(language, out Content.Shared._WL.Languages.LanguagePrototype? prototype) && prototype != null
                ? loc(prototype.Name)
                : language.Id));
        var confederation = prototypes.TryIndex(record.Confederation, out ConfederationRecordsPrototype? confederationPrototype)
            ? loc(confederationPrototype.Name)
            : noData;
        var species = prototypes.TryIndex<SpeciesPrototype>(record.Species, out var speciesPrototype)
            ? loc(speciesPrototype.Name)
            : noData;

        return new RecordPrintIdentity(
            string.IsNullOrWhiteSpace(record.Fullname) ? record.Name : record.Fullname,
            loc(record.Species is "Ipc" or "Android"
                ? "records-date-of-manufacture-edit"
                : "records-date-of-birth-edit"),
            string.IsNullOrWhiteSpace(record.DateOfBirth) ? noData : record.DateOfBirth,
            loc($"humanoid-profile-editor-sex-{record.Sex.ToString().ToLowerInvariant()}-text"),
            species,
            $"{record.Height} {loc("records-height-unit-centimeters")}",
            record.Fingerprint ?? noData,
            record.DNA ?? noData,
            string.IsNullOrWhiteSpace(languages) ? noData : languages,
            confederation,
            string.IsNullOrWhiteSpace(record.Country) ? noData : record.Country);
    }
}
