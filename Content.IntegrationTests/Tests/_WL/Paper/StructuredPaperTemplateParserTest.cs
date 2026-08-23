using System.Linq;
using Content.Shared._WL.Paper;

namespace Content.IntegrationTests.Tests._WL.Paper;

[TestFixture]
public sealed class StructuredPaperTemplateParserTest
{
    [Test]
    public void ParsesInlineAndBlockFieldsWithoutChangingLineStructure()
    {
        const string template = "Employee: %%field:employee%%\nReason:\n%%multiline:reason%%\nStamp area";

        Assert.That(StructuredPaperTemplateParser.TryParse(template, out var elements, out var error),
            Is.True,
            error);

        Assert.That(elements.Where(element => element.Type == StructuredPaperElementType.SingleLineField)
            .Select(element => element.Id), Is.EqualTo(new[] { "employee" }));
        Assert.That(elements.Where(element => element.Type == StructuredPaperElementType.MultilineField)
            .Select(element => element.Id), Is.EqualTo(new[] { "reason" }));

        elements.Single(element => element.Id == "employee").Text = "Alex";
        elements.Single(element => element.Id == "reason").Text = "Testing";
        var flattened = string.Concat(elements.Select(element =>
            element.Text + (element.NewLineAfter ? "\n" : string.Empty)));

        Assert.That(flattened, Is.EqualTo("Employee: Alex\nReason:\nTesting\nStamp area"));
    }

    [Test]
    public void RejectsDuplicateFieldIds()
    {
        const string template = "%%field:name%%\n%%field:name%%";

        Assert.That(StructuredPaperTemplateParser.TryParse(template, out _, out var error), Is.False);
        Assert.That(error, Does.Contain("duplicate"));
    }

    [Test]
    public void RejectsInlineMultilineFields()
    {
        const string template = "Reason: %%multiline:reason%%";

        Assert.That(StructuredPaperTemplateParser.TryParse(template, out _, out var error), Is.False);
        Assert.That(error, Does.Contain("only content"));
    }

    [Test]
    public void RejectsFieldIdsReservedForGeneratedStaticElements()
    {
        const string template = "Label: %%field:static-1%%";

        Assert.That(StructuredPaperTemplateParser.TryParse(template, out _, out var error), Is.False);
        Assert.That(error, Does.Contain("Invalid"));
    }

    [Test]
    public void ParsesSignatureMarkerAsSignatureAndInitialFieldValue()
    {
        const string template = "Position: %%field:position=:JOB:%%\nSigned by: %%signature:author-signature%%";

        Assert.That(StructuredPaperTemplateParser.TryParse(template, out var elements, out var error),
            Is.True,
            error);

        var position = elements.Single(element => element.Id == "position");
        Assert.That(position.Type, Is.EqualTo(StructuredPaperElementType.SingleLineField));
        Assert.That(position.Text, Is.EqualTo(":JOB:"));

        var signature = elements.Single(element => element.Id == "author-signature");
        Assert.That(signature.Type, Is.EqualTo(StructuredPaperElementType.Signature));
        Assert.That(signature.Text, Is.Empty);
    }
}
