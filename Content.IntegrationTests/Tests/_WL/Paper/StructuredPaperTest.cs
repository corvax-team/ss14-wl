using System.Collections.Generic;
using System.Linq;
using Content.Server._WL.Documents;
using Content.IntegrationTests.Tests.Interaction;
using Content.Shared._WL.Paper;
using Content.Shared.Paper;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Preferences;
using Content.Shared.Traits;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using static Content.Shared.Paper.PaperComponent;

namespace Content.IntegrationTests.Tests._WL.Paper;

[TestFixture]
public sealed class StructuredPaperTest : InteractionTest
{
    [Test]
    public async Task HandwritingTraitsAreMutuallyExclusiveAndProvideTheirStyle()
    {
        var expected = new Dictionary<ProtoId<TraitPrototype>, PaperHandwritingStyle>
        {
            ["HandwritingDefault"] = PaperHandwritingStyle.Default,
            ["HandwritingNeat"] = PaperHandwritingStyle.Neat,
            ["HandwritingQuick"] = PaperHandwritingStyle.Quick,
            ["HandwritingFormal"] = PaperHandwritingStyle.Formal,
            ["HandwritingHeavy"] = PaperHandwritingStyle.Heavy,
            ["HandwritingMessy"] = PaperHandwritingStyle.Messy,
        };

        await Server.WaitAssertion(() =>
        {
            foreach (var (traitId, style) in expected)
            {
                var trait = ProtoMan.Index(traitId);
                Assert.That(trait.Components.TryGetComponent(Factory, out PaperHandwritingComponent component),
                    Is.True);
                Assert.That(component!.Style, Is.EqualTo(style));
            }

            var profile = new HumanoidCharacterProfile()
                .WithTraitPreference("HandwritingNeat", ProtoMan)
                .WithTraitPreference("HandwritingMessy", ProtoMan);

            Assert.That(profile.TraitPreferences, Does.Not.Contain((ProtoId<TraitPrototype>) "HandwritingNeat"));
            Assert.That(profile.TraitPreferences, Does.Contain((ProtoId<TraitPrototype>) "HandwritingMessy"));
            Assert.That(profile.TraitPreferences.Count(trait => expected.ContainsKey(trait)), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task HandwrittenTextCanOnlyBeAppendedWithPen()
    {
        await SpawnTarget("PrintedDocumentApplicationEmployment");
        var paper = STarget!.Value;
        SEntMan.GetComponent<PrintedDocumentFormatComponent>(paper).Taken = true;
        var structured = SEntMan.GetComponent<StructuredPaperComponent>(paper);
        var originalCount = structured.Elements.Count;
        structured.Elements[^1].NewLineAfter = false;

        await Activate();
        await SendBui(PaperUiKey.Key, new PaperAppendTextMessage("An unauthorized note."));
        Assert.That(structured.Elements, Has.Count.EqualTo(originalCount));

        await PlaceInHands("Pen");
        await SendBui(PaperUiKey.Key, new PaperAppendTextMessage("First line\nSecond line"));

        Assert.That(structured.Elements, Has.Count.EqualTo(originalCount + 1));
        var appended = structured.Elements[^1];
        Assert.That(appended.Id, Does.StartWith("note-"));
        Assert.That(appended.Type, Is.EqualTo(StructuredPaperElementType.HandwrittenText));
        Assert.That(appended.Text, Is.EqualTo("First line\nSecond line"));
        Assert.That(structured.Elements[^2].NewLineAfter, Is.True,
            "Appended handwriting must begin on a physical new line.");
        Assert.That(SEntMan.GetComponent<PaperComponent>(paper).Content, Does.EndWith("First line\nSecond line\n"));
    }

    [Test]
    public async Task FieldSubmissionStartsAuthorizedSessionWithoutRoundTrip()
    {
        await SpawnTarget("PrintedDocumentApplicationEmployment");
        var paper = STarget!.Value;
        SEntMan.GetComponent<PrintedDocumentFormatComponent>(paper).Taken = true;

        await Activate();
        Assert.That(IsUiOpen(PaperUiKey.Key), Is.True);

        await SendBui(PaperUiKey.Key, new PaperInputFieldMessage("employment-department", "Denied"));
        var structured = SEntMan.GetComponent<StructuredPaperComponent>(paper);
        Assert.That(structured.Elements.Single(element => element.Id == "employment-department").Text,
            Is.Empty);

        await PlaceInHands("Pen");
        await SendBui(PaperUiKey.Key, new PaperInputFieldMessage("employment-department", "Engineering"));
        Assert.That(structured.Elements.Single(element => element.Id == "employment-department").Text,
            Is.EqualTo("Engineering"));
    }

    [Test]
    public async Task FieldCorrectionsArePhysicalAndBounded()
    {
        await SpawnTarget("PrintedDocumentApplicationEmployment");
        var paper = STarget!.Value;
        SEntMan.GetComponent<PrintedDocumentFormatComponent>(paper).Taken = true;
        var field = SEntMan.GetComponent<StructuredPaperComponent>(paper).Elements
            .Single(element => element.Id == "employment-department");

        await InteractUsing("Pen");
        await SendBui(PaperUiKey.Key, new PaperInputFieldMessage(field.Id, "   "));
        Assert.That(field.Text, Is.Empty);
        Assert.That(field.Revisions, Is.Empty);

        foreach (var value in new[] { "One", "Two", "Three", "Four", "Five" })
            await SendBui(PaperUiKey.Key, new PaperInputFieldMessage(field.Id, value));

        Assert.That(field.Text, Is.EqualTo("Four"));
        Assert.That(field.Revisions.Select(revision => revision.Text),
            Is.EqualTo(new[] { "One", "Two", "Three" }));
    }

    [Test]
    public async Task LegacyPaperSupportsFreeTextAndAdvancedStructureConversion()
    {
        await SpawnTarget("Paper");
        var paper = STarget!.Value;

        Assert.That(SEntMan.HasComponent<StructuredPaperComponent>(paper), Is.False);

        await InteractUsing("Pen");
        await SendBui(PaperUiKey.Key, new PaperInputTextMessage("A legacy free-form note."));

        Assert.That(SEntMan.GetComponent<PaperComponent>(paper).Content,
            Is.EqualTo("A legacy free-form note."));
        Assert.That(SEntMan.HasComponent<StructuredPaperComponent>(paper), Is.False);

        await InteractUsing("Pen");
        await SendBui(PaperUiKey.Key, new PaperInputTextMessage("A second physical line."));
        Assert.That(SEntMan.GetComponent<PaperComponent>(paper).Content,
            Is.EqualTo("A legacy free-form note.\nA second physical line."));

        await CloseBui(PaperUiKey.Key);
        await InteractUsing("PenCentcom");
        await SendBui(PaperUiKey.Key, new PaperInputTextMessage("Rewritten with full access."));
        Assert.That(SEntMan.GetComponent<PaperComponent>(paper).Content,
            Is.EqualTo("Rewritten with full access."));

        await InteractUsing("PenCentcom");

        var structure = new List<StructuredPaperElement>
        {
            new("converted-static", StructuredPaperElementType.StaticText, "Converted document: ",
                newLineAfter: false),
            new("converted-field", StructuredPaperElementType.SingleLineField, "approved"),
        };

        await SendBui(PaperUiKey.Key, new PaperInputStructureMessage(structure));

        var structured = SEntMan.GetComponent<StructuredPaperComponent>(paper);
        Assert.That(structured.Elements, Has.Count.EqualTo(2));
        Assert.That(structured.Elements.Select(element => element.Id),
            Is.EqualTo(new[] { "converted-static", "converted-field" }));
        Assert.That(SEntMan.GetComponent<PaperComponent>(paper).Content,
            Is.EqualTo("Converted document: approved\n"));
    }

    [Test]
    public async Task OrdinaryPenCanAppendStructuredElementsWithoutReplacingExistingText()
    {
        await SpawnTarget("Paper");
        var paper = STarget!.Value;
        SEntMan.GetComponent<PaperComponent>(paper).Content = "Existing physical text.";
        SEntMan.System<MetaDataSystem>().SetEntityName(SPlayer, "Alex Writer");

        await InteractUsing("Pen");
        var draft = new List<StructuredPaperElement>
        {
            new("client-text", StructuredPaperElementType.HandwrittenText, "A formatted paragraph."),
            new("client-field", StructuredPaperElementType.SingleLineField, "spoofed value"),
            new("client-signature", StructuredPaperElementType.Signature, "spoofed signature"),
            new("trailing-editor", StructuredPaperElementType.HandwrittenText, string.Empty),
        };

        await SendBui(PaperUiKey.Key, new PaperAppendElementsMessage(draft));

        var structured = SEntMan.GetComponent<StructuredPaperComponent>(paper);
        Assert.That(structured.Elements, Has.Count.EqualTo(4));
        Assert.That(structured.Elements[0].Text, Is.EqualTo("Existing physical text."));
        Assert.That(structured.Elements[1].Type, Is.EqualTo(StructuredPaperElementType.HandwrittenText));
        Assert.That(structured.Elements[1].Text, Is.EqualTo("A formatted paragraph."));
        Assert.That(structured.Elements[2].Type, Is.EqualTo(StructuredPaperElementType.SingleLineField));
        Assert.That(structured.Elements[2].Text, Is.Empty);
        Assert.That(structured.Elements[3].Type, Is.EqualTo(StructuredPaperElementType.Signature));
        Assert.That(structured.Elements[3].Text, Is.Empty);
        Assert.That(structured.Elements.Select(element => element.Id), Is.Unique);
        Assert.That(structured.Elements.Select(element => element.Id),
            Does.Not.Contain("client-field"), "Client element IDs must not become authoritative.");
        Assert.That(SEntMan.GetComponent<PaperComponent>(paper).Content,
            Does.StartWith("Existing physical text.\nA formatted paragraph."));
    }

    [Test]
    public async Task SignatureEditorTokenBecomesInlineInteractiveSignature()
    {
        await SpawnTarget("Paper");
        var paper = STarget!.Value;
        SEntMan.System<MetaDataSystem>().SetEntityName(SPlayer, "Alex Writer");

        await InteractUsing("Pen");
        await SendBui(PaperUiKey.Key, new PaperAppendElementsMessage(
        [
            new StructuredPaperElement(
                "client-text",
                StructuredPaperElementType.HandwrittenText,
                "Approved by [sign] without a line break."),
        ]));

        var structured = SEntMan.GetComponent<StructuredPaperComponent>(paper);
        Assert.That(structured.Elements.Select(element => element.Type), Is.EqualTo(new[]
        {
            StructuredPaperElementType.HandwrittenText,
            StructuredPaperElementType.Signature,
            StructuredPaperElementType.HandwrittenText,
        }));
        Assert.That(structured.Elements[0].Text, Is.EqualTo("Approved by "));
        Assert.That(structured.Elements[1].Text, Is.Empty);
        Assert.That(structured.Elements[2].Text, Is.EqualTo(" without a line break."));
        Assert.That(structured.Elements.Take(2).All(element => !element.NewLineAfter), Is.True);
        Assert.That(SEntMan.GetComponent<PaperComponent>(paper).Content, Does.Not.Contain("[sign]"));

        await SendBui(PaperUiKey.Key, new PaperSignFieldMessage(structured.Elements[1].Id));
        Assert.That(structured.Elements[1].Text, Does.Contain("Alex Writer"));
        Assert.That(SEntMan.GetComponent<PaperComponent>(paper).Content,
            Does.Contain("Approved by Alex Writer without a line break."));
    }

    [Test]
    public async Task PaperEditAccessIsValidatedByServer()
    {
        await SpawnTarget("PrintedDocumentApplicationEmployment");
        var paper = STarget!.Value;

        var structured = SEntMan.GetComponent<StructuredPaperComponent>(paper);
        Assert.That(structured.Elements.Any(element => element.Id == "employment-department"));
        Assert.That(structured.Elements.Single(element => element.Id == "employment-department").NewLineAfter, Is.True);
        Assert.That(structured.Elements.All(element => element.LocId == null), Is.True);
        Assert.That(SEntMan.GetComponent<PaperComponent>(paper).Content, Does.Not.Contain(":ДАТА:"));
        Assert.That(SEntMan.GetComponent<PaperComponent>(paper).Content, Does.Not.Contain(":DATE:"));

        var ui = SEntMan.GetComponent<UserInterfaceComponent>(paper);
        var initialState = (PaperBoundUserInterfaceState) ui.States[PaperUiKey.Key];
        Assert.That(initialState.Elements, Is.Not.Null);
        Assert.That(initialState.Elements!.Where(element => element.Type == StructuredPaperElementType.StaticText)
            .All(element => !string.IsNullOrEmpty(element.Text)), Is.True);

        // InteractionTest's synthetic player has no valid server-side mind. Map-init formatting is covered above;
        // skip the unrelated pickup formatter so this test can focus on paper BUI authorization and serialization.
        SEntMan.GetComponent<PrintedDocumentFormatComponent>(paper).Taken = true;

        await InteractUsing("Pen");

        var forgedStructure = new List<StructuredPaperElement>
        {
            new("forged", StructuredPaperElementType.StaticText, "FORGED DOCUMENT"),
        };

        await SendBui(PaperUiKey.Key, new PaperInputStructureMessage(forgedStructure));

        Assert.That(structured.Elements[0].Text, Is.Not.EqualTo("FORGED DOCUMENT"));

        await SendBui(PaperUiKey.Key, new PaperInputFieldMessage("employment-department", "Engineering"));

        Assert.That(structured.Elements.Single(element => element.Id == "employment-department").Text,
            Is.EqualTo("Engineering"));
        Assert.That(SEntMan.GetComponent<PaperComponent>(paper).Content, Does.Contain("Engineering"));

        await SendBui(PaperUiKey.Key, new PaperInputFieldMessage("employment-department", "Science"));
        var corrected = structured.Elements.Single(element => element.Id == "employment-department");
        Assert.That(corrected.Text, Is.EqualTo("Science"));
        Assert.That(corrected.PreviousText, Is.Empty);
        Assert.That(corrected.Revisions.Select(revision => revision.Text),
            Is.EqualTo(new[] { "Engineering" }));

        var hands = SEntMan.System<SharedHandsSystem>();
        Assert.That(hands.TryGetActiveItem(SPlayer, out var activePen), Is.True);
        Assert.That(activePen, Is.Not.Null);
        SEntMan.System<MetaDataSystem>().SetEntityName(SPlayer, "Alex Writer");
        Assert.That(SEntMan.GetComponent<MetaDataComponent>(SPlayer).EntityName, Is.EqualTo("Alex Writer"));
        Assert.That(IsUiOpen(PaperUiKey.Key), Is.True);
        Assert.That(structured.Elements.Sum(element => element.Text.Length + element.PreviousText.Length +
                element.Revisions.Sum(revision => revision.Text.Length)),
            Is.LessThan(SEntMan.GetComponent<PaperComponent>(paper).ContentSize));

        await SendBui(PaperUiKey.Key, new PaperInputFieldMessage("author-signature", "Alex Writer"));
        var signature = structured.Elements.Single(element => element.Id == "author-signature");
        Assert.That(signature.Type, Is.EqualTo(StructuredPaperElementType.Signature));
        Assert.That(signature.Text, Is.EqualTo("Alex Writer"));
        Assert.That(signature.Text, Does.Not.Contain('\n'));

        await SendBui(PaperUiKey.Key, new PaperInputFieldMessage("employment-department", "Invalid\nname"));
        Assert.That(structured.Elements.Single(element => element.Id == "employment-department").Text,
            Is.EqualTo("Science"));

        await SendBui(PaperUiKey.Key, new PaperInputFieldMessage("employment-department", new string('x',
            StructuredPaperElement.DefaultSingleLineMaxLength + 1)));
        Assert.That(structured.Elements.Single(element => element.Id == "employment-department").Text,
            Is.EqualTo("Science"));

        await CloseBui(PaperUiKey.Key);
        await InteractUsing("RubberStampCaptain");
        Assert.That(SEntMan.GetComponent<PaperComponent>(paper).StampedBy, Has.Count.EqualTo(1));

        await InteractUsing("Pen");
        Assert.That(IsUiOpen(PaperUiKey.Key), Is.False);

        await InteractUsing("PenCentcom");
        var invalidStructure = new List<StructuredPaperElement>
        {
            new("invalid-style", StructuredPaperElementType.SingleLineField, "Invalid")
            {
                HandwritingStyle = (PaperHandwritingStyle) byte.MaxValue,
            },
        };
        await SendBui(PaperUiKey.Key, new PaperInputStructureMessage(invalidStructure));
        Assert.That(structured.Elements.Any(element => element.Id == "employment-department"), Is.True);

        await SendBui(PaperUiKey.Key, new PaperInputStructureMessage(forgedStructure));

        Assert.That(structured.Elements, Has.Count.EqualTo(1));
        Assert.That(structured.Elements[0].Text, Is.EqualTo("FORGED DOCUMENT"));

        var newSignature = new List<StructuredPaperElement>
        {
            new("signature", StructuredPaperElementType.Signature, string.Empty),
        };
        await InteractUsing("PenCentcom");
        await SendBui(PaperUiKey.Key, new PaperInputStructureMessage(newSignature));
        Assert.That(structured.Elements.Single().Type, Is.EqualTo(StructuredPaperElementType.Signature));
        Assert.That(structured.Elements.Single().Text, Is.Empty);

        var editedSignature = new List<StructuredPaperElement>
        {
            new("signature", StructuredPaperElementType.Signature, "Edited signature"),
        };
        await InteractUsing("PenCentcom");
        await SendBui(PaperUiKey.Key, new PaperInputStructureMessage(editedSignature));
        Assert.That(structured.Elements.Single().Type, Is.EqualTo(StructuredPaperElementType.Signature));
        Assert.That(structured.Elements.Single().Text, Is.EqualTo("Edited signature"));

        await SpawnTarget("PrintedDocumentApplicationAccess");
        var accessPaper = STarget!.Value;
        var accessStructured = SEntMan.GetComponent<StructuredPaperComponent>(accessPaper);
        SEntMan.GetComponent<PrintedDocumentFormatComponent>(accessPaper).Taken = true;

        await InteractUsing("Pen");
        await SendBui(PaperUiKey.Key, new PaperInputFieldMessage("requested-accesses", "Engineering\nCommand"));

        Assert.That(accessStructured.Elements.Single(element => element.Id == "requested-accesses").Text,
            Is.EqualTo("Engineering\nCommand"));
    }
}
