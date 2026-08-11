using System.Collections.Generic;
using System.Linq;
using Content.Shared._WL.Records;
using NUnit.Framework;
using Robust.Shared.Utility;

namespace Content.Tests.Shared._WL.Records;

[TestFixture]
public sealed class StructuredCharacterRecordsTest
{
    [Test]
    public void SpecialtyCatalogContainsEveryWikiGroup()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SpecialtyGroupCatalog.Subgroups.Keys,
                Is.EquivalentTo(StructuredCharacterRecords.SpecialtyGroups));
            Assert.That(SpecialtyGroupCatalog.Subgroups.Values.Sum(group => group.Count), Is.EqualTo(104));
            Assert.That(SpecialtyGroupCatalog.Subgroups.Values, Has.All.Not.Empty);
        });
    }

    [Test]
    public void SpecialtySubgroupIdsAreUnique()
    {
        var subgroupIds = SpecialtyGroupCatalog.Subgroups.Values.SelectMany(group => group).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(subgroupIds, Is.Unique);
            Assert.That(subgroupIds.All(id => id.All(character =>
                char.IsAsciiLetterLower(character) || char.IsAsciiDigit(character) || character == '-')), Is.True);
        });
    }

    [Test]
    public void LegacyMedicalRecordIsPreservedInNotes()
    {
        const string legacy = "Legacy allergy record";

        var record = StructuredCharacterRecords.ReadMedical(legacy);

        Assert.That(record.Notes, Is.EqualTo(legacy));
    }

    [Test]
    public void MaximumLegacyRecordIsNotTruncatedDuringMigration()
    {
        var legacy = new string('x', StructuredCharacterRecords.MaxNotesTextLength);

        var restored = StructuredCharacterRecords.ReadSecurity(legacy);

        Assert.That(restored.Notes, Is.EqualTo(legacy));
    }

    [Test]
    public void MedicalRecordRoundTripsDelimiters()
    {
        var original = new MedicalRecordData
        {
            Weight = "72 kg",
            DoNotResuscitate = true,
            EmergencyContact = "Ivan: 12;34",
            Notes = "Line 1\nLine 2",
        };

        var restored = StructuredCharacterRecords.ReadMedical(
            StructuredCharacterRecords.WriteMedical(original));

        Assert.Multiple(() =>
        {
            Assert.That(restored.Weight, Is.EqualTo(original.Weight));
            Assert.That(restored.DoNotResuscitate, Is.True);
            Assert.That(restored.EmergencyContact, Is.EqualTo(original.EmergencyContact));
            Assert.That(restored.Notes, Is.EqualTo(original.Notes));
        });
    }

    [Test]
    public void EmploymentRecordRoundTripsEducationEntries()
    {
        var original = new EmploymentRecordData
        {
            AcademicTitle = RecordAcademicTitle.Professor,
            AcademicTitleField = "Navigation and traffic control",
            Licenses = "Shuttle piloting",
            Education = new List<EducationRecordData>
            {
                new()
                {
                    Specialty = "Space navigation",
                    SpecialtyGroup = "resource-use-and-transport",
                    SpecialtySubgroup = "resource-use-and-transport-2",
                    Degree = RecordAcademicDegree.Master,
                    Institution = "Inter-Species University of Orion",
                    DiplomaDate = "12.06.2868",
                },
                new()
                {
                    Specialty = "Engineering",
                    Degree = RecordAcademicDegree.Bachelor,
                },
            },
        };

        var restored = StructuredCharacterRecords.ReadEmployment(
            StructuredCharacterRecords.WriteEmployment(original));

        Assert.Multiple(() =>
        {
            Assert.That(restored.AcademicTitle, Is.EqualTo(RecordAcademicTitle.Professor));
            Assert.That(restored.AcademicTitleField, Is.EqualTo(original.AcademicTitleField));
            Assert.That(restored.Education, Has.Count.EqualTo(2));
            Assert.That(restored.Education[0].Specialty, Is.EqualTo("Space navigation"));
            Assert.That(restored.Education[0].SpecialtyGroup, Is.EqualTo(original.Education[0].SpecialtyGroup));
            Assert.That(restored.Education[0].SpecialtySubgroup, Is.EqualTo(original.Education[0].SpecialtySubgroup));
            Assert.That(restored.Education[0].Degree, Is.EqualTo(RecordAcademicDegree.Master));
            Assert.That(restored.Education[1].Degree, Is.EqualTo(RecordAcademicDegree.Bachelor));
        });
    }

    [Test]
    public void EmploymentHistoryUsesItsExpandedLimit()
    {
        var history = new string('x', StructuredCharacterRecords.MaxEmploymentHistoryLength + 1);

        var restored = StructuredCharacterRecords.ReadEmployment(
            StructuredCharacterRecords.WriteEmployment(new EmploymentRecordData
            {
                EmploymentHistory = history,
            }));

        Assert.That(restored.EmploymentHistory, Has.Length.EqualTo(
            StructuredCharacterRecords.MaxEmploymentHistoryLength));
    }

    [Test]
    public void VersionOneEmploymentRecordRemainsReadable()
    {
        const string versionOne = "WL_EMPLOYMENT_V1:7;1:09:Professor10:01.01.28700:0:0:0:";

        var restored = StructuredCharacterRecords.ReadEmployment(versionOne);

        Assert.Multiple(() =>
        {
            Assert.That(restored.AcademicTitle, Is.EqualTo(RecordAcademicTitle.Professor));
            Assert.That(restored.AcademicTitleDate, Is.EqualTo("01.01.2870"));
            Assert.That(restored.AcademicTitleField, Is.Empty);
        });
    }

    [Test]
    public void VersionTwoEmploymentRecordRemainsReadable()
    {
        // An explicit V2 payload contains five fields per education entry and no small group.
        const string payload = "WL_EMPLOYMENT_V2:13;1:118:AssociateProfessor9:Mechanics0:0:0:0:0:17:Applied mechanics25:mathematics-and-mechanics6:Master10:University10:01.02.2860";
        var restored = StructuredCharacterRecords.ReadEmployment(payload);

        Assert.Multiple(() =>
        {
            Assert.That(restored.Education, Has.Count.EqualTo(1));
            Assert.That(restored.Education[0].Specialty, Is.EqualTo("Applied mechanics"));
            Assert.That(restored.Education[0].SpecialtySubgroup, Is.Empty);
            Assert.That(restored.AcademicTitleField, Is.EqualTo("Mechanics"));
        });
    }

    [Test]
    public void DamagedStructuredRecordRemainsVisible()
    {
        const string damaged = "WL_SECURITY_V1:2;5:test";

        var record = StructuredCharacterRecords.ReadSecurity(damaged);

        Assert.That(record.Notes, Is.EqualTo(damaged));
    }

    [Test]
    public void LegacyResidenceIsPreservedAsAddressDetails()
    {
        const string legacy = "Alpha Centauri, residential block 12";

        var residence = StructuredCharacterRecords.ReadResidence(legacy);

        Assert.That(residence.Details, Is.EqualTo(legacy));
    }

    [Test]
    public void StructuredResidenceRoundTrips()
    {
        var original = new ResidenceRecordData
        {
            Confederation = "Inter-Species Alliance",
            Planet = "New Moscow",
            Street = "Block 7",
            Unit = "Apartment 42",
            Details = "Courtyard entrance",
        };

        var restored = StructuredCharacterRecords.ReadResidence(
            StructuredCharacterRecords.WriteResidence(original));

        Assert.Multiple(() =>
        {
            Assert.That(restored.Confederation, Is.EqualTo(original.Confederation));
            Assert.That(restored.Planet, Is.EqualTo(original.Planet));
            Assert.That(restored.Street, Is.EqualTo(original.Street));
            Assert.That(restored.Unit, Is.EqualTo(original.Unit));
            Assert.That(restored.Details, Is.EqualTo(original.Details));
        });
    }

    [Test]
    public void SecurityRecordDoesNotAlterNestedResidencePayload()
    {
        const string residence = "WL_ADDRESS_V1:5;0:10:[bold]Luna0:0:0:";
        var storage = StructuredCharacterRecords.WriteSecurity(new SecurityRecordData
        {
            Residence = residence,
        });

        var restored = StructuredCharacterRecords.ReadSecurity(storage);

        Assert.That(restored.Residence, Is.EqualTo(residence));
    }

    [Test]
    public void PrintedSecurityRecordUsesMarkupInsteadOfSerializedStorage()
    {
        var storage = StructuredCharacterRecords.WriteSecurity(new SecurityRecordData
        {
            Residence = StructuredCharacterRecords.WriteResidence(new ResidenceRecordData
            {
                Planet = "Luna",
            }),
            Notes = "Verified by security staff",
        });

        var body = StructuredRecordFormatter.FormatSecurity(storage, key => key);
        var printed = StructuredRecordFormatter.FormatDocument(
            "Security dossier",
            "#8A3F42",
            new RecordPrintIdentity(
                "Employee",
                "MANUFACTURE DATE:",
                "01.01.2850",
                "Male",
                "IPC",
                "180 cm",
                "No fingerprint",
                "No DNA",
                "Common",
                "None",
                "None"),
            body,
            key => key,
            includeForensics: true);

        Assert.Multiple(() =>
        {
            Assert.That(printed, Does.Not.Contain("WL_SECURITY_V1"));
            Assert.That(printed, Does.Not.Contain("WL_ADDRESS_V1"));
            Assert.That(printed, Does.Contain("MANUFACTURE DATE:"));
            Assert.That(printed, Does.Contain("180 cm"));
            Assert.That(printed, Does.Contain("records-view-fingerprint"));
            Assert.That(printed, Does.Contain("records-view-dna"));
            Assert.That(printed, Does.Contain("No fingerprint"));
            Assert.That(printed, Does.Contain("No DNA"));
            Assert.That(FormattedMessage.TryFromMarkup(printed, out _), Is.True);
        });

        var printedWithoutForensics = StructuredRecordFormatter.FormatDocument(
            "General record",
            "#252529",
            new RecordPrintIdentity(
                "Employee",
                "DATE OF BIRTH:",
                "01.01.2850",
                "Male",
                "Human",
                "180 cm",
                "Fingerprint",
                "DNA",
                "Common",
                "None",
                "None"),
            body,
            key => key);

        Assert.Multiple(() =>
        {
            Assert.That(printedWithoutForensics, Does.Not.Contain("records-view-fingerprint"));
            Assert.That(printedWithoutForensics, Does.Not.Contain("records-view-dna"));
            Assert.That(printedWithoutForensics, Does.Not.Contain("Fingerprint"));
            Assert.That(printedWithoutForensics, Does.Not.Contain("DNA"));
        });
    }

    [Test]
    public void PrintedRecordEscapesAuthorMarkup()
    {
        var storage = StructuredCharacterRecords.WriteMedical(new MedicalRecordData
        {
            Notes = "[bold]author markup[/bold]",
        });

        var printed = StructuredRecordFormatter.FormatMedical(storage, key => key, "Human");
        Assert.Multiple(() =>
        {
            Assert.That(printed, Does.Not.Contain("[bold]author markup[/bold]"));
            Assert.That(printed, Does.Contain("author markup"));
            Assert.That(FormattedMessage.TryFromMarkup(printed, out _), Is.True);
        });
    }

    [Test]
    public void ManufacturedSpeciesUseRepairRecords()
    {
        var storage = StructuredCharacterRecords.WriteMedical(new MedicalRecordData
        {
            Surgeries = "Replaced actuator",
            Medication = "Oil",
        });

        var human = StructuredRecordFormatter.FormatMedical(storage, key => key, "Human");
        var ipc = StructuredRecordFormatter.FormatMedical(storage, key => key, "Ipc");
        var android = StructuredRecordFormatter.FormatMedical(storage, key => key, "Android");
        var golem = StructuredRecordFormatter.FormatMedical(storage, key => key, "Golem");

        Assert.Multiple(() =>
        {
            Assert.That(human, Does.Contain("records-surgeries"));
            Assert.That(human, Does.Contain("records-medication"));

            Assert.That(ipc, Does.Contain("records-repair-records"));
            Assert.That(ipc, Does.Not.Contain("records-surgeries"));
            Assert.That(ipc, Does.Not.Contain("records-medication"));

            Assert.That(android, Does.Contain("records-repair-records"));
            Assert.That(android, Does.Not.Contain("records-surgeries"));
            Assert.That(android, Does.Not.Contain("records-medication"));

            Assert.That(golem, Does.Not.Contain("records-repair-records"));
            Assert.That(golem, Does.Not.Contain("records-surgeries"));
            Assert.That(golem, Does.Not.Contain("records-medication"));
        });
    }
}
