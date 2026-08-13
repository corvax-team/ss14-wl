using Content.Shared._WL.Forensics;
using NUnit.Framework;

namespace Content.Tests.Shared._WL.Forensics;

[TestFixture]
[TestOf(typeof(ForensicReportFormatter))]
public sealed class ForensicReportFormatterTest
{
    [Test]
    public void ReportEscapesValuesAndRemovesDuplicateDna()
    {
        var result = ForensicReportFormatter.Format(
            "[bold]locker[/bold]",
            ["fingerprint"],
            ["fiber"],
            ["shared-dna"],
            [" shared-dna ", "solution-dna"],
            ["residue"],
            key => key);

        Assert.Multiple(() =>
        {
            Assert.That(result, Does.Contain("\\[bold]locker\\[/bold]"));
            Assert.That(result.Split("shared-dna"), Has.Length.EqualTo(2));
            Assert.That(result, Does.Contain("solution-dna"));
        });
    }

    [Test]
    public void EmptySectionsUseLocalizedPlaceholder()
    {
        var result = ForensicReportFormatter.Format(
            "locker",
            [],
            [],
            [],
            [],
            [],
            key => key);

        Assert.That(result.Split("forensic-scanner-printout-no-traces"), Has.Length.EqualTo(5));
    }
}
