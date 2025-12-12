#nullable enable
using Content.Server.Antag;
using Content.Server.Antag.Components;
using Content.Server.Construction.Conditions;
using Content.Server.GameTicking;
using Content.Shared.GameTicking;
using Robust.Shared.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Random;
using System.Collections.Generic;
using System.Linq;

namespace Content.IntegrationTests.Tests.GameRules;

// Once upon a time, players in the lobby weren't ever considered eligible for antag roles.
// Lets not let that happen again.
[TestFixture]
public sealed class AntagPreferenceTest
{
    [Test]
    public async Task TestLobbyPlayersValid()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            DummyTicker = false,
            Connected = true,
            InLobby = true
        });

        var server = pair.Server;
        var client = pair.Client;
        var ticker = server.System<GameTicker>();
        var sys = server.System<AntagSelectionSystem>();

        // Initially in the lobby
        // WL-Changes-start
        Assert.Multiple(() =>
        {
            Assert.That(ticker.RunLevel, Is.EqualTo(GameRunLevel.PreRoundLobby));
            Assert.That(client.AttachedEntity, Is.Null);
            Assert.That(ticker.PlayerGameStatuses[client.User!.Value], Is.EqualTo(PlayerGameStatus.NotReadyToPlay));
        });
        // WL-Changes-end

        EntityUid uid = default;
        await server.WaitPost(() => uid = server.EntMan.Spawn("Traitor"));
        var rule = new Entity<AntagSelectionComponent>(uid, server.EntMan.GetComponent<AntagSelectionComponent>(uid));
        var def = rule.Comp.Definitions.Single();

        // IsSessionValid & IsEntityValid are preference agnostic and should always be true for players in the lobby.
        // Though maybe that will change in the future, but then GetPlayerPool() needs to be updated to reflect that.
        // WL-Changes-start
        await server.WaitPost(() =>
        {
            Assert.Multiple(() =>
            {
                Assert.That(sys.IsSessionValid(rule, pair.Player, def), Is.True);
                Assert.That(sys.IsEntityValid(client.AttachedEntity, def), Is.True);
            });
        });
        // WL-Changes-end

        // By default, traitor/antag preferences are disabled, so the pool should be empty.
        var sessions = new List<ICommonSession> { pair.Player! };

        // WL-Changes-start
        AntagSelectionPlayerPool? pool = null;
        await server.WaitPost(() =>
        {
            pool = sys.GetPlayerPool(rule, sessions, def);
            Assert.That(pool.Count, Is.EqualTo(0));
        });
        // WL-Changes-end

        // Opt into the traitor role.
        await pair.SetAntagPreference("Traitor", true);

        // WL-Changes-start
        await server.WaitPost(() =>
        {
            Assert.Multiple(() =>
            {
                Assert.That(sys.IsSessionValid(rule, pair.Player, def), Is.True);
                Assert.That(sys.IsEntityValid(client.AttachedEntity, def), Is.True);
            });

            pool = sys.GetPlayerPool(rule, sessions, def);
            Assert.That(pool.Count, Is.EqualTo(1));

            pool.TryPickAndTake(server.ResolveDependency<IRobustRandom>(), out var picked);

            Assert.Multiple(() =>
            {
                Assert.That(picked, Is.EqualTo(pair.Player));
                Assert.That(sessions, Has.Count.EqualTo(1));
            });
        });
        // WL-Changes-end

        // opt back out
        await pair.SetAntagPreference("Traitor", false);

        // WL-Changes-start
        await server.WaitPost(() =>
        {
            Assert.Multiple(() =>
            {
                Assert.That(sys.IsSessionValid(rule, pair.Player, def), Is.True);
                Assert.That(sys.IsEntityValid(client.AttachedEntity, def), Is.True);
            });

            pool = sys.GetPlayerPool(rule, sessions, def);
            Assert.That(pool.Count, Is.EqualTo(0));
        });
        // WL-Changes-end

        await server.WaitPost(() => server.EntMan.DeleteEntity(uid));
        await pair.CleanReturnAsync();
    }
}
