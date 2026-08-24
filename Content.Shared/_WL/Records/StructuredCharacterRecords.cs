using System.Text;
using Robust.Shared.Utility;

namespace Content.Shared._WL.Records;

public enum RecordMaritalStatus
{
    NotApplicable,
    Single,
    Married,
    Widowed,
}

public enum RecordAcademicDegree
{
    NotApplicable,
    Qualificate,
    Bachelor,
    Master,
    Candidate,
    Doctor,
}

public enum RecordAcademicTitle
{
    NotApplicable,
    Assistant,
    AssociateProfessor,
    Professor,
}

public sealed class MedicalRecordData
{
    public string Weight { get; set; } = string.Empty;
    public string PostmortemInstructions { get; set; } = string.Empty;
    public bool DoNotResuscitate { get; set; }
    public string RefusedTreatment { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public string Surgeries { get; set; } = string.Empty;
    public string Medication { get; set; } = string.Empty;
    public string PhysiologicalNotes { get; set; } = string.Empty;
    public string PsychologicalNotes { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string LastUpdated { get; set; } = string.Empty;
}

public sealed class SecurityRecordData
{
    public string Residence { get; set; } = string.Empty;
    public string IdentifyingFeatures { get; set; } = string.Empty;
    public RecordMaritalStatus MaritalStatus { get; set; }
    public string CloseRelatives { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public string Permits { get; set; } = string.Empty;
    public bool UnderSecuritySupervision { get; set; }
    public string ArrestHistory { get; set; } = string.Empty;
    public string ImprisonmentHistory { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string LastUpdated { get; set; } = string.Empty;
}

public sealed class ResidenceRecordData
{
    public string Confederation { get; set; } = string.Empty;
    public string Planet { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}

public sealed class EducationRecordData
{
    public string Specialty { get; set; } = string.Empty;
    public string SpecialtyGroup { get; set; } = string.Empty;
    public string SpecialtySubgroup { get; set; } = string.Empty;
    public RecordAcademicDegree Degree { get; set; }
    public string Institution { get; set; } = string.Empty;
    public string DiplomaDate { get; set; } = string.Empty;
}

public sealed class EmploymentRecordData
{
    public List<EducationRecordData> Education { get; set; } = new();
    public RecordAcademicTitle AcademicTitle { get; set; }
    public string AcademicTitleField { get; set; } = string.Empty;
    public string AcademicTitleDate { get; set; } = string.Empty;
    public string Licenses { get; set; } = string.Empty;
    public string EmploymentHistory { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string LastUpdated { get; set; } = string.Empty;
}

/// <summary>
/// Stores fixed record forms in the existing profile text columns.
/// Legacy free-form records are preserved in the corresponding notes field.
/// </summary>
public static class StructuredCharacterRecords
{
    public const int MaxEducationEntries = 8;
    public const int MaxShortTextLength = 256;
    public const int MaxLongTextLength = 2048;
    public const int MaxEmploymentHistoryLength = 4096;
    public const int MaxNotesTextLength = 4096;

    private const string MedicalPrefix = "WL_MEDICAL_V1:";
    private const string SecurityPrefix = "WL_SECURITY_V1:";
    private const string EmploymentV1Prefix = "WL_EMPLOYMENT_V1:";
    private const string EmploymentV2Prefix = "WL_EMPLOYMENT_V2:";
    private const string EmploymentV3Prefix = "WL_EMPLOYMENT_V3:";
    private const string ResidencePrefix = "WL_ADDRESS_V1:";

    public static MedicalRecordData ReadMedical(string? storage)
    {
        if (!TryReadFields(storage, MedicalPrefix, 11, out var fields))
            return new MedicalRecordData { Notes = ClampNotes(storage) };

        return new MedicalRecordData
        {
            Weight = ClampShort(fields[0]),
            PostmortemInstructions = ClampShort(fields[1]),
            DoNotResuscitate = ReadBool(fields[2]),
            RefusedTreatment = ClampShort(fields[3]),
            EmergencyContact = ClampShort(fields[4]),
            Surgeries = ClampLong(fields[5]),
            Medication = ClampLong(fields[6]),
            PhysiologicalNotes = ClampLong(fields[7]),
            PsychologicalNotes = ClampLong(fields[8]),
            Notes = ClampNotes(fields[9]),
            LastUpdated = ClampShort(fields[10]),
        };
    }

    public static string WriteMedical(MedicalRecordData record)
    {
        return WriteFields(MedicalPrefix, new[]
        {
            ClampShort(record.Weight),
            ClampShort(record.PostmortemInstructions),
            WriteBool(record.DoNotResuscitate),
            ClampShort(record.RefusedTreatment),
            ClampShort(record.EmergencyContact),
            ClampLong(record.Surgeries),
            ClampLong(record.Medication),
            ClampLong(record.PhysiologicalNotes),
            ClampLong(record.PsychologicalNotes),
            ClampNotes(record.Notes),
            ClampShort(record.LastUpdated),
        });
    }

    public static SecurityRecordData ReadSecurity(string? storage)
    {
        if (!TryReadFields(storage, SecurityPrefix, 11, out var fields))
            return new SecurityRecordData { Notes = ClampNotes(storage) };

        return new SecurityRecordData
        {
            Residence = ClampSerialized(fields[0], MaxLongTextLength),
            IdentifyingFeatures = ClampShort(fields[1]),
            MaritalStatus = ReadEnum<RecordMaritalStatus>(fields[2]),
            CloseRelatives = ClampShort(fields[3]),
            EmergencyContact = ClampShort(fields[4]),
            Permits = ClampLong(fields[5]),
            UnderSecuritySupervision = ReadBool(fields[6]),
            ArrestHistory = ClampLong(fields[7]),
            ImprisonmentHistory = ClampLong(fields[8]),
            Notes = ClampNotes(fields[9]),
            LastUpdated = ClampShort(fields[10]),
        };
    }

    public static string WriteSecurity(SecurityRecordData record)
    {
        return WriteFields(SecurityPrefix, new[]
        {
            ClampSerialized(record.Residence, MaxLongTextLength),
            ClampShort(record.IdentifyingFeatures),
            record.MaritalStatus.ToString(),
            ClampShort(record.CloseRelatives),
            ClampShort(record.EmergencyContact),
            ClampLong(record.Permits),
            WriteBool(record.UnderSecuritySupervision),
            ClampLong(record.ArrestHistory),
            ClampLong(record.ImprisonmentHistory),
            ClampNotes(record.Notes),
            ClampShort(record.LastUpdated),
        });
    }

    public static EmploymentRecordData ReadEmployment(string? storage)
    {
        var version = 3;
        if (!TryReadFields(storage, EmploymentV3Prefix, null, out var fields))
        {
            version = 2;
            if (!TryReadFields(storage, EmploymentV2Prefix, null, out fields))
            {
                version = 1;
                if (!TryReadFields(storage, EmploymentV1Prefix, null, out fields))
                    return new EmploymentRecordData { Notes = ClampNotes(storage) };
            }
        }

        var baseFieldCount = version >= 2 ? 8 : 7;
        var educationFieldCount = version >= 3 ? 6 : 5;
        if (fields.Count < baseFieldCount ||
            !int.TryParse(fields[0], out var educationCount) ||
            educationCount is < 0 or > MaxEducationEntries ||
            fields.Count != baseFieldCount + educationCount * educationFieldCount)
        {
            return new EmploymentRecordData { Notes = ClampNotes(storage) };
        }

        var record = new EmploymentRecordData
        {
            AcademicTitle = ReadEnum<RecordAcademicTitle>(fields[1]),
            AcademicTitleField = version >= 2 ? ClampShort(fields[2]) : string.Empty,
            AcademicTitleDate = ClampShort(fields[version >= 2 ? 3 : 2]),
            Licenses = ClampLong(fields[version >= 2 ? 4 : 3]),
            EmploymentHistory = ClampEmploymentHistory(fields[version >= 2 ? 5 : 4]),
            Notes = ClampNotes(fields[version >= 2 ? 6 : 5]),
            LastUpdated = ClampShort(fields[version >= 2 ? 7 : 6]),
        };

        for (var i = 0; i < educationCount; i++)
        {
            var offset = baseFieldCount + i * educationFieldCount;
            record.Education.Add(new EducationRecordData
            {
                Specialty = ClampShort(fields[offset]),
                SpecialtyGroup = ClampShort(fields[offset + 1]),
                SpecialtySubgroup = version >= 3 ? ClampShort(fields[offset + 2]) : string.Empty,
                Degree = ReadEnum<RecordAcademicDegree>(fields[offset + (version >= 3 ? 3 : 2)]),
                Institution = ClampShort(fields[offset + (version >= 3 ? 4 : 3)]),
                DiplomaDate = ClampShort(fields[offset + (version >= 3 ? 5 : 4)]),
            });
        }

        return record;
    }

    public static string WriteEmployment(EmploymentRecordData record)
    {
        var educationCount = Math.Min(record.Education.Count, MaxEducationEntries);
        var fields = new List<string>(8 + educationCount * 6)
        {
            educationCount.ToString(),
            record.AcademicTitle.ToString(),
            ClampShort(record.AcademicTitleField),
            ClampShort(record.AcademicTitleDate),
            ClampLong(record.Licenses),
            ClampEmploymentHistory(record.EmploymentHistory),
            ClampNotes(record.Notes),
            ClampShort(record.LastUpdated),
        };

        for (var i = 0; i < educationCount; i++)
        {
            var education = record.Education[i];
            fields.Add(ClampShort(education.Specialty));
            fields.Add(ClampShort(education.SpecialtyGroup));
            fields.Add(ClampShort(education.SpecialtySubgroup));
            fields.Add(education.Degree.ToString());
            fields.Add(ClampShort(education.Institution));
            fields.Add(ClampShort(education.DiplomaDate));
        }

        return WriteFields(EmploymentV3Prefix, fields);
    }

    public static ResidenceRecordData ReadResidence(string? storage)
    {
        if (!TryReadFields(storage, ResidencePrefix, 5, out var fields))
            return new ResidenceRecordData { Details = ClampShort(storage) };

        return new ResidenceRecordData
        {
            Confederation = ClampShort(fields[0]),
            Planet = ClampShort(fields[1]),
            Street = ClampShort(fields[2]),
            Unit = ClampShort(fields[3]),
            Details = ClampShort(fields[4]),
        };
    }

    public static string WriteResidence(ResidenceRecordData residence)
    {
        if (string.IsNullOrWhiteSpace(residence.Confederation) &&
            string.IsNullOrWhiteSpace(residence.Planet) &&
            string.IsNullOrWhiteSpace(residence.Street) &&
            string.IsNullOrWhiteSpace(residence.Unit) &&
            string.IsNullOrWhiteSpace(residence.Details))
        {
            return string.Empty;
        }

        return WriteFields(ResidencePrefix, new[]
        {
            ClampShort(residence.Confederation),
            ClampShort(residence.Planet),
            ClampShort(residence.Street),
            ClampShort(residence.Unit),
            ClampShort(residence.Details),
        });
    }

    public static string NormalizeMedical(string? storage) => WriteMedical(ReadMedical(storage));
    public static string NormalizeSecurity(string? storage) => WriteSecurity(ReadSecurity(storage));
    public static string NormalizeEmployment(string? storage) => WriteEmployment(ReadEmployment(storage));

    private static string WriteFields(string prefix, IReadOnlyList<string> fields)
    {
        var builder = new StringBuilder(prefix);
        builder.Append($"{fields.Count};");
        foreach (var field in fields)
            builder.Append($"{field.Length}:{field}");

        return builder.ToString();
    }

    private static bool TryReadFields(string? storage, string prefix, int? expectedCount, out List<string> fields)
    {
        fields = new List<string>();
        if (string.IsNullOrEmpty(storage) || !storage.StartsWith(prefix, StringComparison.Ordinal))
            return false;

        var position = prefix.Length;
        if (!TryReadNumber(storage, ref position, ';', out var count) ||
            count < 0 ||
            expectedCount != null && count != expectedCount)
        {
            return false;
        }

        for (var i = 0; i < count; i++)
        {
            if (!TryReadNumber(storage, ref position, ':', out var length) ||
                length < 0 ||
                length > storage.Length - position)
            {
                return false;
            }

            fields.Add(storage.Substring(position, length));
            position += length;
        }

        return position == storage.Length;
    }

    private static bool TryReadNumber(string value, ref int position, char delimiter, out int number)
    {
        number = 0;
        var hasDigit = false;
        while (position < value.Length && value[position] != delimiter)
        {
            var character = value[position++];
            if (character is < '0' or > '9')
                return false;

            var digit = character - '0';
            if (number > (int.MaxValue - digit) / 10)
                return false;

            number = number * 10 + digit;
            hasDigit = true;
        }

        if (!hasDigit || position >= value.Length)
            return false;

        position++;
        return true;
    }

    private static T ReadEnum<T>(string value) where T : struct, Enum
    {
        return Enum.TryParse<T>(value, out var result) && Enum.IsDefined(result) ? result : default;
    }

    private static bool ReadBool(string value) => value == "1";
    private static string WriteBool(bool value) => value ? "1" : "0";
    public static string NormalizeShortText(string? value) => Clamp(value, MaxShortTextLength);

    private static string ClampShort(string? value) => NormalizeShortText(value);
    private static string ClampLong(string? value) => Clamp(value, MaxLongTextLength);
    private static string ClampEmploymentHistory(string? value) => Clamp(value, MaxEmploymentHistoryLength);
    private static string ClampNotes(string? value) => Clamp(value, MaxNotesTextLength);

    private static string ClampSerialized(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static string Clamp(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var trimmed = FormattedMessage.RemoveMarkupPermissive(value).Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
