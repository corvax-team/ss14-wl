#nullable enable

using System.Collections.Generic;
using System.Linq;
using Content.IntegrationTests.Fixtures;
using Content.IntegrationTests.Fixtures.Attributes;
using Content.Server._WL.Documents;
using Content.Shared._WL.Paper;
using Content.Shared.CCVar;
using Content.Shared.Paper;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.IntegrationTests.Tests._WL.Paper;

[TestFixture]
public sealed class StructuredPaperCatalogTest : GameTest
{
    private const string PrintedDocumentPrototype = "PrintedDocument";
    private const string LegacyDocumentPrototype = "PrintedDocumentErrorLoadingFormHeader";
    private const string RoundTripDocumentPrototype = "PrintedDocumentApplicationEmployment";
    private const string RoundTripField = "employment-department";
    private const string RoundTripValue = "Engineering";
    private const string StampName = "stamp-component-stamped-name-captain";
    private const string StampState = "paper_stamp-captain";

    private static readonly string[] MapInitAutoTokens =
    [
        ":STATION:",
        ":DATE:",
        ":СТАНЦИЯ:",
        ":ДАТА:",
    ];

    public override PoolSettings PoolSettings => new()
    {
        Connected = false,
    };

    [Test]
    public async Task PrintedDocumentCatalogInitializesResolvedStructuredForms()
    {
        await Server.WaitAssertion(() =>
        {
            var documents = SProtoMan.EnumeratePrototypes<EntityPrototype>()
                .Where(prototype =>
                    !prototype.Abstract &&
                    SProtoMan.EnumerateAllParents<EntityPrototype>(prototype.ID)
                        .Any(parent => parent.id == PrintedDocumentPrototype))
                .OrderBy(prototype => prototype.ID)
                .ToList();

            Assert.That(documents, Has.Count.EqualTo(64),
                $"Expected the complete printed-document catalog, got: {string.Join(", ", documents.Select(prototype => prototype.ID))}");
            Assert.That(documents.Select(prototype => prototype.ID), Does.Contain(LegacyDocumentPrototype));

            foreach (var prototype in documents)
            {
                var document = SSpawn(prototype.ID);
                var paper = SEntMan.GetComponent<PaperComponent>(document);

                if (prototype.ID == LegacyDocumentPrototype)
                {
                    Assert.That(SEntMan.HasComponent<StructuredPaperComponent>(document), Is.False,
                        $"{LegacyDocumentPrototype} must remain a legacy free-form document.");
                    Assert.That(paper.Content, Is.Not.Empty);
                    continue;
                }

                Assert.That(SEntMan.TryGetComponent(document, out StructuredPaperComponent? structured), Is.True,
                    $"{prototype.ID} did not initialize as structured paper.");
                Assert.That(structured, Is.Not.Null);

                var elements = structured!.Elements;
                var ids = elements.Select(element => element.Id).ToList();
                var flattened = Flatten(elements);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(structured.TemplateLocId, Is.Null,
                        $"{prototype.ID} retained its prototype template localization ID after map init.");
                    Assert.That(elements, Is.Not.Empty, $"{prototype.ID} resolved to an empty form.");
                    Assert.That(elements.Count, Is.LessThanOrEqualTo(128),
                        $"{prototype.ID} exceeds the server structure limit.");
                    Assert.That(ids.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(ids.Count),
                        $"{prototype.ID} contains duplicate element IDs.");
                    Assert.That(ids.All(IsValidElementId), Is.True,
                        $"{prototype.ID} contains an invalid element ID.");
                    Assert.That(elements.All(element => element.LocId == null), Is.True,
                        $"{prototype.ID} retained an element localization ID after map init.");
                    Assert.That(paper.Content, Does.Not.Contain("%%field:"),
                        $"{prototype.ID} leaked a single-line template marker into Paper.Content. " +
                        $"Element IDs: {string.Join(", ", ids)}");
                    Assert.That(paper.Content, Does.Not.Contain("%%multiline:"),
                        $"{prototype.ID} leaked a multiline template marker into Paper.Content. " +
                        $"Element IDs: {string.Join(", ", ids)}");
                    Assert.That(paper.Content, Does.Not.Contain("%%signature:"),
                        $"{prototype.ID} leaked a signature template marker into Paper.Content. " +
                        $"Element IDs: {string.Join(", ", ids)}");
                    Assert.That(elements.Count(element =>
                            element.Type == StructuredPaperElementType.Signature &&
                            element.Id == "author-signature"),
                        Is.EqualTo(1),
                        $"{prototype.ID} must have exactly one author-signature element.");
                    Assert.That(paper.Content, Is.EqualTo(flattened),
                        $"{prototype.ID} Paper.Content differs from its flattened structured elements.");

                    foreach (var token in MapInitAutoTokens)
                    {
                        Assert.That(paper.Content, Does.Not.Contain(token),
                            $"{prototype.ID} retained the automatic token {token} after map init.");
                    }
                }
            }
        });
    }

    [Test]
    [EnsureCVar(Side.Server, typeof(CCVars), nameof(CCVars.GridFill), false)]
    public async Task FilledStampedStructuredDocumentSurvivesMapRoundTrip()
    {
        var mapPath = new ResPath("/Maps/Test/StructuredPaperRoundTrip.yml");
        var mapData = await Pair.CreateTestMap();
        var mapLoader = SEntMan.System<MapLoaderSystem>();
        var mapSystem = SEntMan.System<SharedMapSystem>();
        var paperSystem = SEntMan.System<PaperSystem>();
        var resourceManager = Server.ResolveDependency<IResourceManager>();

        var expectedStamp = new StampDisplayInfo
        {
            StampedName = StampName,
            StampedColor = Color.FromHex("#1b487e"),
        };
        (string Id, StructuredPaperElementType Type, string Text, string Revisions,
            PaperHandwritingStyle HandwritingStyle, PaperHandwritingStyle PreviousHandwritingStyle,
            bool NewLineAfter, int MaxLength)[] expectedElements = [];
        var expectedContent = string.Empty;

        await Server.WaitAssertion(() =>
        {
            resourceManager.UserData.CreateDir(mapPath.Directory);

            var document = SEntMan.SpawnEntity(RoundTripDocumentPrototype, mapData.GridCoords);
            var paper = SEntMan.GetComponent<PaperComponent>(document);
            var structured = SEntMan.GetComponent<StructuredPaperComponent>(document);
            var format = SEntMan.GetComponent<PrintedDocumentFormatComponent>(document);

            var filledField = structured.Elements.Single(element => element.Id == RoundTripField);
            filledField.Text = RoundTripValue;
            filledField.Revisions.Add(new PaperFieldRevision("Previous value", PaperHandwritingStyle.Messy));
            filledField.HandwritingStyle = PaperHandwritingStyle.Formal;
            paperSystem.RefreshStructuredContent((document, paper), structured);
            Assert.That(paperSystem.TryStamp((document, paper), expectedStamp, StampState), Is.True);
            format.Taken = true;

            expectedElements = structured.Elements
                .Select(element => (element.Id, element.Type, element.Text,
                    string.Join('|', element.Revisions.Select(revision => $"{revision.Text}:{revision.HandwritingStyle}")),
                    element.HandwritingStyle, element.PreviousHandwritingStyle, element.NewLineAfter, element.MaxLength))
                .ToArray();
            expectedContent = paper.Content;

            Assert.That(paper.StampState, Is.EqualTo(StampState));
            Assert.That(mapLoader.TrySaveMap(mapData.MapId, mapPath), Is.True);
            mapSystem.DeleteMap(mapData.MapId);
        });

        await Server.WaitIdleAsync();

        MapId loadedMap = default;
        await Server.WaitAssertion(() =>
        {
            Assert.That(mapLoader.TryLoadMap(mapPath, out var map, out _), Is.True);
            loadedMap = map!.Value.Comp.MapId;
        });

        await Server.WaitIdleAsync();

        await Server.WaitAssertion(() =>
        {
            var matches = new List<EntityUid>();
            var query = SEntMan.AllEntityQueryEnumerator<MetaDataComponent, TransformComponent>();
            while (query.MoveNext(out var uid, out var metadata, out var transform))
            {
                if (transform.MapID == loadedMap && metadata.EntityPrototype?.ID == RoundTripDocumentPrototype)
                    matches.Add(uid);
            }

            Assert.That(matches, Has.Count.EqualTo(1));
            var document = matches.Single();
            var paper = SEntMan.GetComponent<PaperComponent>(document);
            var structured = SEntMan.GetComponent<StructuredPaperComponent>(document);
            var format = SEntMan.GetComponent<PrintedDocumentFormatComponent>(document);
            var actualElements = structured.Elements
                .Select(element => (element.Id, element.Type, element.Text,
                    string.Join('|', element.Revisions.Select(revision => $"{revision.Text}:{revision.HandwritingStyle}")),
                    element.HandwritingStyle, element.PreviousHandwritingStyle, element.NewLineAfter, element.MaxLength))
                .ToArray();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(structured.TemplateLocId, Is.Null);
                Assert.That(actualElements, Is.EqualTo(expectedElements));
                Assert.That(structured.Elements.Single(element => element.Id == RoundTripField).Text,
                    Is.EqualTo(RoundTripValue));
                Assert.That(paper.Content, Is.EqualTo(expectedContent));
                Assert.That(paper.Content, Is.EqualTo(Flatten(structured.Elements)));
                Assert.That(paper.StampedBy, Has.Count.EqualTo(1));
                Assert.That(paper.StampedBy[0].StampedName, Is.EqualTo(expectedStamp.StampedName));
                Assert.That(paper.StampedBy[0].StampedColor, Is.EqualTo(expectedStamp.StampedColor));
                Assert.That(paper.StampState, Is.EqualTo(StampState));
                Assert.That(format.Taken, Is.True);
            }

            mapSystem.DeleteMap(loadedMap);
        });
    }

    private static string Flatten(IEnumerable<StructuredPaperElement> elements)
    {
        return string.Concat(elements.Select(element =>
            element.Text + (element.NewLineAfter ? "\n" : string.Empty)));
    }

    private static bool IsValidElementId(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || id.Length > 64)
            return false;

        return id.All(character =>
            char.IsAsciiLetterOrDigit(character) || character is '-' or '_');
    }
}
