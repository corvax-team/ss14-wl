namespace Content.Client._WL.Records;

public enum RecordViewKind
{
    Medical,
    Security,
    Employment,
}

public enum RecordValueSource
{
    Automatic,
    Author,
    Placeholder,
}

public sealed record RecordViewField(
    string Label,
    string Value,
    RecordValueSource Source,
    bool LongValue = false,
    bool Warning = false);

public sealed record RecordViewSection(string Title, IReadOnlyList<RecordViewField> Fields);

public sealed record RecordIdentityData(
    string Name,
    string Job,
    string Fingerprint,
    string Dna,
    string FullName,
    string DateLabel,
    string Date,
    string Sex,
    string Species,
    string Height,
    string Languages,
    string Confederation,
    string Country);

public sealed record RecordViewData(
    RecordViewKind Kind,
    RecordIdentityData Identity,
    IReadOnlyList<RecordViewSection> Sections);
