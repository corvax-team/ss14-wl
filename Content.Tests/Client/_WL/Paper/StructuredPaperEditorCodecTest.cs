using System.Collections.Generic;
using System.Linq;
using Content.Client._WL.Paper.UI;
using Content.Shared._WL.Paper;
using NUnit.Framework;

namespace Content.Tests.Client._WL.Paper;

[TestFixture]
[TestOf(typeof(StructuredPaperEditorCodec))]
public sealed class StructuredPaperEditorCodecTest
{
    [Test]
    public void FullEditorUsesAliasesBoundToStableElementIds()
    {
        var first = new StructuredPaperElement(
            "first-field",
            StructuredPaperElementType.SingleLineField,
            "Alpha",
            newLineAfter: false);
        first.Revisions.Add(new PaperFieldRevision("Old alpha", PaperHandwritingStyle.Messy));
        var second = new StructuredPaperElement(
            "second-field",
            StructuredPaperElementType.MultilineField,
            "Beta",
            newLineAfter: false);
        var codec = StructuredPaperEditorCodec.Create([first, second], false, out var source);

        Assert.That(source, Is.EqualTo("[f:1]Alpha[/f][lf:2]Beta[/lf]"));

        var reordered = "[lf:2]Beta[/lf][f:1]Alpha[/f]";
        Assert.That(codec.TryParse(reordered, false, PaperHandwritingStyle.Default, out var parsed), Is.True);
        Assert.That(parsed.Select(element => element.Id),
            Is.EqualTo(new[] { "second-field", "first-field" }));
        Assert.That(parsed[1].Revisions.Single().Text, Is.EqualTo("Old alpha"));
    }

    [Test]
    public void FullEditorRoundTripPreservesFieldsAndSourceLayout()
    {
        var elements = new List<StructuredPaperElement>
        {
            new("static", StructuredPaperElementType.StaticText, "Name: ", newLineAfter: false),
            new("name", StructuredPaperElementType.SingleLineField, "Alex", newLineAfter: true)
            {
                HandwritingStyle = PaperHandwritingStyle.Neat,
            },
            new("reason-label", StructuredPaperElementType.StaticText, "Reason:", newLineAfter: true),
            new("reason", StructuredPaperElementType.MultilineField, "Two\nlines", newLineAfter: true),
            new("signature", StructuredPaperElementType.Signature, string.Empty, newLineAfter: false),
        };
        var codec = StructuredPaperEditorCodec.Create(elements, false, out var source);

        Assert.That(codec.TryParse(source, false, PaperHandwritingStyle.Default, out var parsed), Is.True);
        Assert.That(parsed.Single(element => element.Id == "name").HandwritingStyle,
            Is.EqualTo(PaperHandwritingStyle.Neat));
        Assert.That(parsed.Single(element => element.Id == "reason").Text, Is.EqualTo("Two\nlines"));
        Assert.That(parsed.Single(element => element.Id == "signature").Type,
            Is.EqualTo(StructuredPaperElementType.Signature));

        StructuredPaperEditorCodec.Create(parsed, false, out var reparsedSource);
        Assert.That(reparsedSource, Is.EqualTo(source));
    }

    [Test]
    public void AppendEditorCreatesHandwritingAndEmptyInteractiveFields()
    {
        var codec = StructuredPaperEditorCodec.Create([], true, out var source);
        Assert.That(source, Is.Empty);

        const string draft = "A [bold]formatted[/bold] note.\n[f]\n[lf]\nSigned: [sign]";
        Assert.That(codec.TryParse(draft, true, PaperHandwritingStyle.Formal, out var parsed), Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(parsed.Where(element => element.Type == StructuredPaperElementType.HandwrittenText)
                .All(element => element.HandwritingStyle == PaperHandwritingStyle.Formal), Is.True);
            Assert.That(parsed.Count(element => element.Type == StructuredPaperElementType.SingleLineField), Is.EqualTo(1));
            Assert.That(parsed.Count(element => element.Type == StructuredPaperElementType.MultilineField), Is.EqualTo(1));
            Assert.That(parsed.Count(element => element.Type == StructuredPaperElementType.Signature), Is.EqualTo(1));
            Assert.That(parsed.Where(element => element.Type is StructuredPaperElementType.SingleLineField or
                    StructuredPaperElementType.MultilineField or StructuredPaperElementType.Signature)
                .All(element => element.Text.Length == 0), Is.True);
        });
    }

    [Test]
    public void MixedHandwritingAndEmptyFieldsFromFullEditorAreAccepted()
    {
        var elements = new List<StructuredPaperElement>
        {
            new("note-one", StructuredPaperElementType.HandwrittenText,
                "Обычное место, можете писать здесь.\nПишите: "),
            new("short", StructuredPaperElementType.SingleLineField, string.Empty),
            new("note-two", StructuredPaperElementType.HandwrittenText,
                "\nПоле побольше, для [head=3] больших [/head] нужд.\n"),
            new("long", StructuredPaperElementType.MultilineField, string.Empty),
            new("note-three", StructuredPaperElementType.HandwrittenText,
                "\n\nЗдесь, пожалуйста, [italic]распишитесь[/italic].\n"),
        };
        var codec = StructuredPaperEditorCodec.Create(elements, false, out var source);

        Assert.That(codec.TryParse(source, false, PaperHandwritingStyle.Default, out var parsed), Is.True);
        Assert.That(parsed.Where(element => !string.IsNullOrEmpty(element.Id)).Select(element => element.Id),
            Is.EqualTo(elements.Select(element => element.Id)));
    }

    [TestCase("[f:1]value")]
    [TestCase("[f:999]value[/f]")]
    [TestCase("[f:1]one[/f][f:1]two[/f]")]
    [TestCase("[f:1]two\nlines[/f]")]
    public void InvalidFullEditorTagsAreRejected(string source)
    {
        var codec = StructuredPaperEditorCodec.Create(
        [
            new StructuredPaperElement("field", StructuredPaperElementType.SingleLineField, "value"),
        ], false, out _);

        Assert.That(codec.TryParse(source, false, PaperHandwritingStyle.Default, out _), Is.False);
    }
}
