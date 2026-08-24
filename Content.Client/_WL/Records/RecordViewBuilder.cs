using Content.Shared._WL.Records;
using Robust.Shared.Prototypes;

namespace Content.Client._WL.Records;

public static class RecordViewBuilder
{
    public static RecordViewData Medical(RecordIdentityData identity, string storage, string species, Func<string, string> loc,
        bool inGame = false)
    {
        var record = StructuredCharacterRecords.ReadMedical(storage);
        var organic = RecordSpeciesClassification.IsOrganic(species);
        var manufactured = RecordSpeciesClassification.IsManufactured(species);
        var sections = new List<RecordViewSection>
        {
            IdentitySection(identity, loc, inGame),
            new(loc("records-view-medical-directions"),
            [
                Author("records-weight", record.Weight, "records-value-no-data", loc),
                Author("records-postmortem", record.PostmortemInstructions, "records-value-not-applicable", loc, true,
                    !string.IsNullOrWhiteSpace(record.PostmortemInstructions)),
                AuthorValue("records-do-not-resuscitate", loc(record.DoNotResuscitate ? "records-value-yes" : "records-value-no"), loc,
                    record.DoNotResuscitate),
                Author("records-refused-treatment", record.RefusedTreatment, "records-value-not-applicable", loc, true,
                    !string.IsNullOrWhiteSpace(record.RefusedTreatment)),
                Author("records-emergency-contact", record.EmergencyContact, "records-value-not-applicable", loc),
            ]),
        };

        if (organic || manufactured)
            sections.Add(LongSection(manufactured ? "records-repair-records" : "records-surgeries",
                record.Surgeries,
                "records-value-no-data",
                loc));

        if (organic)
            sections.Add(LongSection("records-medication", record.Medication, "records-value-no-data", loc));

        sections.Add(LongSection("records-physiological-notes", record.PhysiologicalNotes, "records-value-no-data", loc));
        sections.Add(LongSection("records-psychological-notes", record.PsychologicalNotes, "records-value-no-data", loc));
        sections.Add(LongSection("records-notes", record.Notes, "records-value-no-data", loc));
        sections.Add(ServiceSection(record.LastUpdated, loc));
        return new RecordViewData(RecordViewKind.Medical, identity, sections);
    }

    public static RecordViewData Security(RecordIdentityData identity, string storage, Func<string, string> loc,
        bool inGame = false)
    {
        var record = StructuredCharacterRecords.ReadSecurity(storage);
        var sections = new List<RecordViewSection>
        {
            IdentitySection(identity, loc, inGame),
            new(loc("records-view-personal-details"),
            [
                Author("records-residence-registration", ResidenceValue(record.Residence, loc), "records-value-no-data", loc, true),
                Author("records-identifying-features", record.IdentifyingFeatures, "records-value-no-data", loc, true),
                AuthorValue("records-marital-status", loc($"records-marital-{record.MaritalStatus.ToString().ToLowerInvariant()}"), loc),
                Author("records-close-relatives", record.CloseRelatives, "records-value-no-data", loc),
                Author("records-emergency-contact", record.EmergencyContact, "records-value-no-data", loc),
            ]),
            new(loc("records-view-security-status"),
            [
                Author("records-permits", record.Permits, "records-value-no-data", loc, true),
                AuthorValue("records-security-supervision-short",
                    loc(record.UnderSecuritySupervision ? "records-value-yes" : "records-value-no"), loc,
                    record.UnderSecuritySupervision),
            ]),
            LongSection("records-arrest-history", record.ArrestHistory, "records-value-no-data", loc,
                !string.IsNullOrWhiteSpace(record.ArrestHistory)),
            LongSection("records-imprisonment-history", record.ImprisonmentHistory, "records-value-no-data", loc,
                !string.IsNullOrWhiteSpace(record.ImprisonmentHistory)),
            LongSection("records-notes", record.Notes, "records-value-no-data", loc),
            ServiceSection(record.LastUpdated, loc),
        };
        return new RecordViewData(RecordViewKind.Security, identity, sections);
    }

    public static RecordViewData Employment(RecordIdentityData identity, string storage, IPrototypeManager prototypeManager, Func<string, string> loc,
        bool inGame = false)
    {
        var record = StructuredCharacterRecords.ReadEmployment(storage);
        var educationSections = new List<RecordViewSection>();
        if (record.Education.Count == 0)
        {
            educationSections.Add(new RecordViewSection(loc("records-education"),
                [Placeholder("records-education", "records-value-no-data", loc)]));
        }
        else
        {
            for (var i = 0; i < record.Education.Count; i++)
            {
                var education = record.Education[i];
                var group = SpecialtyGroupCatalog.ContainsGroup(prototypeManager, education.SpecialtyGroup)
                    ? loc($"records-specialty-group-value-{education.SpecialtyGroup}")
                    : education.SpecialtyGroup;
                var subgroup = SpecialtyGroupCatalog.ContainsSubgroup(prototypeManager, education.SpecialtySubgroup)
                    ? loc(SpecialtyGroupCatalog.GetSubgroupLocalizationKey(education.SpecialtySubgroup))
                    : education.SpecialtySubgroup;
                educationSections.Add(new RecordViewSection(
                    $"{loc("records-education")} {i + 1}",
                    [
                        Author("records-specialty", education.Specialty, "records-value-no-data", loc),
                        Author("records-specialty-group", group, "records-value-no-data", loc),
                        Author("records-specialty-subgroup", subgroup, "records-value-no-data", loc),
                        AuthorValue("records-degree", loc($"records-degree-{education.Degree.ToString().ToLowerInvariant()}"), loc),
                        Author("records-institution", education.Institution, "records-value-no-data", loc),
                        Author("records-diploma-date", education.DiplomaDate, "records-value-no-data", loc),
                    ]));
            }
        }

        var sections = new List<RecordViewSection>
        {
            IdentitySection(identity, loc, inGame),
        };
        sections.AddRange(educationSections);
        var academicTitleFields = new List<RecordViewField>
        {
            AuthorValue("records-academic-title-short", loc($"records-academic-title-value-{record.AcademicTitle.ToString().ToLowerInvariant()}"), loc),
        };
        if (record.AcademicTitle != RecordAcademicTitle.NotApplicable)
        {
            academicTitleFields.Add(Author("records-academic-title-field", record.AcademicTitleField, "records-value-not-applicable", loc));
            academicTitleFields.Add(Author("records-academic-title-date", record.AcademicTitleDate, "records-value-no-data", loc));
        }

        sections.AddRange([
            new(loc("records-academic-title-section"), academicTitleFields),
            LongSection("records-licenses", record.Licenses, "records-value-no-data", loc),
            LongSection("records-employment-history", record.EmploymentHistory, "records-value-no-data", loc),
            LongSection("records-notes", record.Notes, "records-value-no-data", loc),
            ServiceSection(record.LastUpdated, loc),
        ]);
        return new RecordViewData(RecordViewKind.Employment, identity, sections);
    }

    private static RecordViewSection IdentitySection(RecordIdentityData identity, Func<string, string> loc, bool inGame)
    {
        var noData = loc("records-value-no-data");
        return new RecordViewSection(loc(inGame ? "records-view-basic-details-employee" : "records-view-basic-details"),
        [
            Automatic("records-full-name-edit", identity.FullName, noData, loc),
            new(identity.DateLabel, Value(identity.Date, noData), RecordValueSource.Automatic),
            Automatic("records-sex", identity.Sex, noData, loc),
            Automatic("records-species", identity.Species, noData, loc),
            Automatic("records-height-label", identity.Height, noData, loc),
            Automatic("records-language", identity.Languages, noData, loc, true),
            Automatic("records-confederation-edit", identity.Confederation, noData, loc),
            Automatic("records-country-edit", identity.Country, noData, loc),
        ]);
    }

    private static RecordViewSection ServiceSection(string lastUpdated, Func<string, string> loc) =>
        new(loc("records-view-service-details"),
        [Author("records-last-updated", lastUpdated, "records-value-no-data", loc)]);

    private static RecordViewSection LongSection(string key, string value, string fallback, Func<string, string> loc, bool warning = false) =>
        new(loc(key), [Author(key, value, fallback, loc, true, warning)]);

    private static RecordViewField Automatic(string key, string value, string fallback, Func<string, string> loc, bool longValue = false) =>
        new(loc(key), Value(value, fallback), RecordValueSource.Automatic, longValue);

    private static RecordViewField Author(string key, string value, string fallbackKey, Func<string, string> loc,
        bool longValue = false, bool warning = false) => string.IsNullOrWhiteSpace(value)
        ? Placeholder(key, fallbackKey, loc)
        : new RecordViewField(loc(key), value.Trim(), RecordValueSource.Author, longValue, warning);

    private static RecordViewField AuthorValue(string key, string value, Func<string, string> loc, bool warning = false) =>
        new(loc(key), value, RecordValueSource.Author, false, warning);

    private static RecordViewField Placeholder(string key, string fallbackKey, Func<string, string> loc) =>
        new(loc(key), loc(fallbackKey), RecordValueSource.Placeholder);

    private static string Value(string value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string ResidenceValue(string storage, Func<string, string> loc)
    {
        var residence = StructuredCharacterRecords.ReadResidence(storage);
        var lines = new List<string>();
        AddResidenceLine(lines, loc("records-residence-confederation"), residence.Confederation);
        AddResidenceLine(lines, loc("records-residence-planet"), residence.Planet);
        AddResidenceLine(lines, loc("records-residence-street"), residence.Street);
        AddResidenceLine(lines, loc("records-residence-unit"), residence.Unit);
        AddResidenceLine(lines, loc("records-residence-details"), residence.Details);
        return string.Join('\n', lines);
    }

    private static void AddResidenceLine(List<string> lines, string label, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            lines.Add($"{label} {value.Trim()}");
    }
}
