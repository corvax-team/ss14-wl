using System.Collections.Generic;
using Content.IntegrationTests.Fixtures;
using Content.IntegrationTests.Fixtures.Attributes;
using Content.IntegrationTests.Utility;
using Content.Shared._WL.Research;
using Content.Shared._WL.Research.Systems;
using Content.Shared._WL.Research.Components;
using Content.Shared._WL.Research.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests._WL.Tests;

public sealed class WLResearchChecks : GameTest
{
    [SidedDependency(Side.Server)] private SharedResearchNewSystem _research = default!;

    /// <summary>
    /// Check research prototypes don't make tree cycled.
    /// </summary>
    [Test]
    public async Task ResearchPrototypesTreeValidTest()
    {
        var pair = Pair;
        var server = pair.Server;

        var protoMan = server.ProtoMan;

        foreach (var research in protoMan.EnumeratePrototypes<ResearchPrototype>())
        {
            var tree = new Dictionary<ProtoId<ResearchPrototype>, ResearchState>();
            tree.Add(research, new ResearchState());

            Assert.That(_research.TryGenerateTechTree(research, ref tree), Is.True, $"Research prototype make technology tree cycled! Invalid proto: {research}");
        }
    }
}
