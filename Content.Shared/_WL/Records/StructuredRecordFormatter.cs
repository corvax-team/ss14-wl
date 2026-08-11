using System.Linq;
using System.Text;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Records;

public sealed record RecordPrintIdentity(
    string FullName,
    string DateLabel,
    string DateOfBirth,
    string Sex,
    string Species,
    string Height,
    string Fingerprint,
    string DNA,
    string Languages,
    string Confederation,
    string Country);

/// <summary>
/// Builds safe SS14 paper markup for structured character records.
/// </summary>
public static class StructuredRecordFormatter
{
    private const string AutomaticColor = "#252529";
    private const string AuthorColor = "#55555D";
    private const string PlaceholderColor = "#85858C";

    public static string FormatDocument(
        string title,
        string accent,
        RecordPrintIdentity identity,
        string body,
        Func<string, string> loc,
        bool includeForensics = false)
    {
        var builder = new StringBuilder();
        builder.Append("[head=2][color=").Append(accent).Append(']')
            .Append(Escape(title)).AppendLine("[/color][/head]");
        builder.Append("[color=").Append(accent).Append("][bold]")
            .Append(Escape(loc("records-view-basic-details-employee")))
            .AppendLine("[/bold][/color]");
        AppendAutomatic(builder, loc("records-full-name-edit"), identity.FullName, loc("records-value-no-data"));
        AppendAutomatic(builder, identity.DateLabel, identity.DateOfBirth, loc("records-value-no-data"));
        AppendAutomatic(builder, loc("records-sex"), identity.Sex, loc("records-value-no-data"));
        AppendAutomatic(builder, loc("records-species"), identity.Species, loc("records-value-no-data"));
        AppendAutomatic(builder, loc("records-height-label"), identity.Height, loc("records-value-no-data"));
        if (includeForensics)
        {
            AppendAutomatic(builder, loc("records-view-fingerprint"), identity.Fingerprint, loc("records-value-no-data"));
            AppendAutomatic(builder, loc("records-view-dna"), identity.DNA, loc("records-value-no-data"));
        }
        AppendAutomatic(builder, loc("records-language"), identity.Languages, loc("records-value-no-data"));
        AppendAutomatic(builder, loc("records-confederation-edit"), identity.Confederation, loc("records-value-no-data"));
        AppendAutomatic(builder, loc("records-country-edit"), identity.Country, loc("records-value-no-data"));
        builder.AppendLine("[color=#77777D]────────────────────────────────────────[/color]");
        builder.Append(body.Trim());
        return builder.ToString().TrimEnd();
    }

    public static string FormatMedical(string storage, Func<string, string> loc, bool organic)
    {
        var record = StructuredCharacterRecords.ReadMedical(storage);
        var builder = new StringBuilder();
        AppendHeading(builder, loc("records-view-medical-directions"), "#36678A");
        AppendAuthor(builder, loc("records-weight"), record.Weight, loc("records-value-no-data"));
        AppendAuthor(builder, loc("records-postmortem"), record.PostmortemInstructions, loc("records-value-not-applicable"));
        AppendAuthor(builder, loc("records-do-not-resuscitate"), YesNo(record.DoNotResuscitate, loc));
        AppendAuthor(builder, loc("records-refused-treatment"), record.RefusedTreatment, loc("records-value-not-applicable"));
        AppendAuthor(builder, loc("records-emergency-contact"), record.EmergencyContact, loc("records-value-not-applicable"));
        if (organic)
        {
            AppendSection(builder, loc("records-surgeries"), record.Surgeries, loc("records-value-no-data"), "#36678A");
            AppendSection(builder, loc("records-medication"), record.Medication, loc("records-value-no-data"), "#36678A");
        }
        AppendSection(builder, loc("records-physiological-notes"), record.PhysiologicalNotes, loc("records-value-no-data"), "#36678A");
        AppendSection(builder, loc("records-psychological-notes"), record.PsychologicalNotes, loc("records-value-no-data"), "#36678A");
        AppendSection(builder, loc("records-notes"), record.Notes, loc("records-value-no-data"), "#36678A");
        AppendAuthor(builder, loc("records-last-updated"), record.LastUpdated, loc("records-value-no-data"));
        return builder.ToString().TrimEnd();
    }

    public static string FormatSecurity(string storage, Func<string, string> loc)
    {
        var record = StructuredCharacterRecords.ReadSecurity(storage);
        var residence = StructuredCharacterRecords.ReadResidence(record.Residence);
        var builder = new StringBuilder();
        AppendHeading(builder, loc("records-residence-registration"), "#8A3F42");
        AppendAuthor(builder, loc("records-residence-confederation"), residence.Confederation, loc("records-value-no-data"));
        AppendAuthor(builder, loc("records-residence-planet"), residence.Planet, loc("records-value-no-data"));
        AppendAuthor(builder, loc("records-residence-street"), residence.Street, loc("records-value-no-data"));
        AppendAuthor(builder, loc("records-residence-unit"), residence.Unit, loc("records-value-no-data"));
        AppendAuthor(builder, loc("records-residence-details"), residence.Details, loc("records-value-no-data"));
        AppendSection(builder, loc("records-identifying-features"), record.IdentifyingFeatures, loc("records-value-no-data"), "#8A3F42");
        AppendAuthor(builder, loc("records-marital-status"), loc($"records-marital-{record.MaritalStatus.ToString().ToLowerInvariant()}"));
        AppendAuthor(builder, loc("records-close-relatives"), record.CloseRelatives, loc("records-value-no-data"));
        AppendAuthor(builder, loc("records-emergency-contact"), record.EmergencyContact, loc("records-value-no-data"));
        AppendSection(builder, loc("records-permits"), record.Permits, loc("records-value-no-data"), "#8A3F42");
        AppendAuthor(builder, loc("records-security-supervision-short"), YesNo(record.UnderSecuritySupervision, loc));
        AppendSection(builder, loc("records-arrest-history"), record.ArrestHistory, loc("records-value-no-data"), "#8A3F42");
        AppendSection(builder, loc("records-imprisonment-history"), record.ImprisonmentHistory, loc("records-value-no-data"), "#8A3F42");
        AppendSection(builder, loc("records-notes"), record.Notes, loc("records-value-no-data"), "#8A3F42");
        AppendAuthor(builder, loc("records-last-updated"), record.LastUpdated, loc("records-value-no-data"));
        return builder.ToString().TrimEnd();
    }

    public static string FormatEmployment(string storage, Func<string, string> loc)
    {
        var record = StructuredCharacterRecords.ReadEmployment(storage);
        var builder = new StringBuilder();
        if (record.Education.Count == 0)
        {
            AppendSection(builder, loc("records-education"), string.Empty, loc("records-value-no-data"), "#356A49");
        }
        else
        {
            for (var i = 0; i < record.Education.Count; i++)
            {
                var education = record.Education[i];
                AppendHeading(builder, $"{loc("records-education")} {i + 1}", "#356A49");
                AppendAuthor(builder, loc("records-specialty"), education.Specialty, loc("records-value-no-data"));
                var specialtyGroup = StructuredCharacterRecords.SpecialtyGroups.Contains(education.SpecialtyGroup)
                    ? loc($"records-specialty-group-value-{education.SpecialtyGroup}")
                    : education.SpecialtyGroup;
                var specialtySubgroup = SpecialtyGroupCatalog.ContainsSubgroup(education.SpecialtySubgroup)
                    ? loc(SpecialtyGroupCatalog.GetSubgroupLocalizationKey(education.SpecialtySubgroup))
                    : education.SpecialtySubgroup;
                AppendAuthor(builder, loc("records-specialty-group"), specialtyGroup, loc("records-value-no-data"));
                AppendAuthor(builder,
                    loc("records-specialty-subgroup"),
                    specialtySubgroup,
                    loc("records-value-no-data"));
                AppendAuthor(builder, loc("records-degree"), loc($"records-degree-{education.Degree.ToString().ToLowerInvariant()}"));
                AppendAuthor(builder, loc("records-institution"), education.Institution, loc("records-value-no-data"));
                AppendAuthor(builder, loc("records-diploma-date"), education.DiplomaDate, loc("records-value-no-data"));
            }
        }

        AppendHeading(builder, loc("records-academic-title-section"), "#356A49");
        AppendAuthor(builder, loc("records-academic-title-short"), loc($"records-academic-title-value-{record.AcademicTitle.ToString().ToLowerInvariant()}"));
        if (record.AcademicTitle != RecordAcademicTitle.NotApplicable)
        {
            AppendAuthor(builder, loc("records-academic-title-field"), record.AcademicTitleField, loc("records-value-not-applicable"));
            AppendAuthor(builder, loc("records-academic-title-date"), record.AcademicTitleDate, loc("records-value-no-data"));
        }
        AppendSection(builder, loc("records-licenses"), record.Licenses, loc("records-value-no-data"), "#356A49");
        AppendSection(builder, loc("records-employment-history"), record.EmploymentHistory, loc("records-value-no-data"), "#356A49");
        AppendSection(builder, loc("records-notes"), record.Notes, loc("records-value-no-data"), "#356A49");
        AppendAuthor(builder, loc("records-last-updated"), record.LastUpdated, loc("records-value-no-data"));
        return builder.ToString().TrimEnd();
    }

    private static string YesNo(bool value, Func<string, string> loc) =>
        loc(value ? "records-value-yes" : "records-value-no");

    private static void AppendAutomatic(StringBuilder builder, string label, string value, string fallback)
    {
        builder.Append("[bold]").Append(Escape(label)).Append("[/bold] ");
        AppendValue(builder, value, fallback, AutomaticColor);
    }

    private static void AppendAuthor(StringBuilder builder, string label, string value, string? fallback = null)
    {
        builder.Append("[bold]").Append(Escape(label)).Append("[/bold] ");
        AppendValue(builder, value, fallback ?? string.Empty, AuthorColor);
    }

    private static void AppendSection(StringBuilder builder, string label, string value, string fallback, string accent)
    {
        builder.AppendLine();
        AppendHeading(builder, label, accent);
        AppendValue(builder, value, fallback, AuthorColor);
    }

    private static void AppendHeading(StringBuilder builder, string title, string accent)
    {
        builder.Append("[color=").Append(accent).Append("][bold]› ")
            .Append(Escape(title)).AppendLine("[/bold][/color]");
    }

    private static void AppendValue(StringBuilder builder, string value, string fallback, string color)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            builder.Append("[color=").Append(PlaceholderColor).Append("][italic]")
                .Append(Escape(fallback)).AppendLine("[/italic][/color]");
            return;
        }

        builder.Append("[color=").Append(color).Append(']')
            .Append(Escape(value.Trim())).AppendLine("[/color]");
    }

    private static string Escape(string value) => FormattedMessage.EscapeText(value);
}
