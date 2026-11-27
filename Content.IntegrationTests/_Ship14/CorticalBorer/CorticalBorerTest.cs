using System.Linq;
using System.Numerics;
using Content.Client.Actions;
using Content.Client.StatusIcon;
using Content.IntegrationTests.Fixtures;
using Content.Server._Ship14.CorticalBorer;
using Content.Server.Mind;
using Content.Shared._Ship14.CorticalBorer;
using Content.Shared._Ship14.CorticalBorer.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage.Components;
using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Content.Shared.Roles;
using Content.Shared.StatusIcon;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests._Ship14.CorticalBorer;

[TestFixture]
public sealed class CorticalBorerTest : GameTest
{
    private static readonly EntProtoId BorerPrototype = "MobCorticalBorer";
    private static readonly EntProtoId HostPrototype = "MobHuman";
    private static readonly ProtoId<ReagentPrototype> Sugar = "Sugar";
    private static readonly EntProtoId MedicalVisor = "ClothingEyesHudMedical";
    private static readonly EntProtoId AdvancedMedicalVisor = "ClothingEyesHudMedicalAdvanced";
    private static readonly EntProtoId BorerRole = "MindRoleCorticalBorer";

    [Test]
    public async Task BorerRoleAssignsObjectiveWithoutGameRule()
    {
        var map = await Pair.CreateTestMap();
        var borer = await SpawnAtPosition(BorerPrototype, map.GridCoords);
        await Server.WaitAssertion(() =>
        {
            var minds = Server.System<MindSystem>();
            var roles = Server.System<SharedRoleSystem>();
            var mind = minds.CreateMind(null);
            minds.TransferTo(mind, borer);
            roles.MindAddRole(mind, BorerRole, mind.Comp);

            Assert.That(minds.TryGetObjectiveComp<CorticalBorerObjectiveComponent>(mind, out var objective, mind.Comp), Is.True);
            Assert.That(objective!.RequiredInfected, Is.EqualTo(2));
        });
    }

    [TestCase("MobIpc")]
    [TestCase("MobAndroid")]
    public async Task InfestingSyntheticSpeciesIsBlocked(string hostPrototype)
    {
        var map = await Pair.CreateTestMap();
        var borer = await SpawnAtPosition(BorerPrototype, map.GridCoords);
        var host = await SpawnAtPosition(hostPrototype, map.GridCoords);

        await Server.WaitAssertion(() =>
        {
            var infest = new CorticalBorerInfestActionEvent { Target = host };
            SEntMan.EventBus.RaiseLocalEvent(borer, ref infest);
            Assert.That(infest.Handled, Is.False);
        });
    }

    [Test]
    public async Task OnlyAdvancedVisorShowsInfestation()
    {
        var map = await Pair.CreateTestMap();
        var borer = await SpawnAtPosition(BorerPrototype, map.GridCoords);
        var host = await SpawnAtPosition(HostPrototype, map.GridCoords);
        var viewer = await SpawnAtPosition(HostPrototype, map.GridCoords);
        var medical = await SpawnAtPosition(MedicalVisor, map.GridCoords);
        var advanced = await SpawnAtPosition(AdvancedMedicalVisor, map.GridCoords);
        var netHost = SEntMan.GetNetEntity(host);
        var inventory = Server.System<InventorySystem>();

        await Server.WaitAssertion(() =>
        {
            var minds = Server.System<MindSystem>();
            var mind = minds.CreateMind(ServerSession!.UserId);
            minds.TransferTo(mind, viewer);
            Assert.That(inventory.TryEquip(viewer, medical, "eyes", silent: true), Is.True);
            var infest = new CorticalBorerInfestActionEvent { Target = host };
            SEntMan.EventBus.RaiseLocalEvent(borer, ref infest);
            Assert.That(infest.Handled, Is.True);
        });

        await RunSeconds(5.5f);
        await RunUntilSynced();
        await AssertInfestationIcon(false);

        await Server.WaitAssertion(() =>
        {
            Assert.That(inventory.TryUnequip(viewer, "eyes", silent: true), Is.True);
            Assert.That(inventory.TryEquip(viewer, advanced, "eyes", silent: true), Is.True);
        });
        await RunUntilSynced();
        await AssertInfestationIcon(true);

        await Server.WaitAssertion(() =>
        {
            var leave = new CorticalBorerLeaveActionEvent();
            SEntMan.EventBus.RaiseLocalEvent(borer, ref leave);
            Assert.That(leave.Handled, Is.True);
        });
        await RunSeconds(5.5f);
        await RunUntilSynced();
        await AssertInfestationIcon(false);

        Task AssertInfestationIcon(bool expected) => Client.WaitAssertion(() =>
        {
            var icons = Client.System<StatusIconSystem>().GetStatusIcons(CEntMan.GetEntity(netHost));
            Assert.That(icons.OfType<StatusIconPrototype>().Any(icon => icon.ID == "HealthIconCorticalBorer"), Is.EqualTo(expected));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task InfestingAndLeavingSwitchesActions(bool interruptWithSugar)
    {
        var map = await Pair.CreateTestMap();
        var borer = await SpawnAtPosition(BorerPrototype, map.GridCoords);
        var host = await SpawnAtPosition(HostPrototype, map.GridCoords);

        await Server.WaitAssertion(() =>
        {
            Assert.That(Server.System<ActionBlockerSystem>().CanSpeak(borer), Is.True);
            var component = SEntMan.GetComponent<CorticalBorerComponent>(borer);
            Assert.That(component.InfestActionEntity, Is.Not.Null);
            var infest = new CorticalBorerInfestActionEvent { Target = host };
            SEntMan.EventBus.RaiseLocalEvent(borer, ref infest);
            Assert.That(infest.Handled, Is.True);
        });

        await RunSeconds(5.5f);
        await Server.WaitAssertion(() =>
        {
            var component = SEntMan.GetComponent<CorticalBorerComponent>(borer);
            Assert.That(component.Host, Is.EqualTo(host));
            Assert.That(component.InfestActionEntity, Is.Null);
            Assert.That(component.LeaveActionEntity, Is.Not.Null);
            var blocker = Server.System<ActionBlockerSystem>();
            Assert.That(blocker.CanSpeak(borer), Is.False);
            Assert.That(blocker.CanEmote(borer), Is.False);
            Assert.That(blocker.CanSpeak(host), Is.True);

            var leave = new CorticalBorerLeaveActionEvent();
            SEntMan.EventBus.RaiseLocalEvent(borer, ref leave);
            Assert.That(leave.Handled, Is.True);

            if (interruptWithSugar)
                AddSugar(host);
        });

        await RunSeconds(5.5f);
        await Server.WaitAssertion(() =>
        {
            var component = SEntMan.GetComponent<CorticalBorerComponent>(borer);
            if (interruptWithSugar)
            {
                Assert.That(component.Host, Is.EqualTo(host));
                Assert.That(component.SugarSuppressed, Is.True);
                return;
            }

            Assert.That(component.Host, Is.Null);
            Assert.That(component.InfestActionEntity, Is.Not.Null);
            Assert.That(component.LeaveActionEntity, Is.Null);
            Assert.That(Server.System<ActionBlockerSystem>().CanSpeak(borer), Is.True);
        });
    }

    [TestCase(true, false, false, false)]
    [TestCase(false, false, false, false)]
    [TestCase(true, true, false, false)]
    [TestCase(true, true, true, false)]
    [TestCase(true, false, false, true)]
    public async Task TakeoverAndReleaseRestoreMindOwnership(bool occupiedHost, bool useSugar, bool detachBorer, bool resist)
    {
        var map = await Pair.CreateTestMap();
        var borer = await SpawnAtPosition(BorerPrototype, map.GridCoords);
        var host = await SpawnAtPosition(HostPrototype, map.GridCoords);
        var minds = Server.System<MindSystem>();
        var actions = Server.System<SharedActionsSystem>();
        EntityUid borerMind = default;
        EntityUid? hostMind = null;
        EntityUid? screechAction = null;
        NetEntity netTrapped = default;

        await Server.WaitAssertion(() =>
        {
            SEntMan.EnsureComponent<GodmodeComponent>(host);
            SEntMan.EnsureComponent<GodmodeComponent>(borer);
            borerMind = minds.CreateMind(null).Owner;
            minds.TransferTo(borerMind, borer);
            if (occupiedHost)
            {
                hostMind = minds.CreateMind(resist ? ServerSession!.UserId : null).Owner;
                minds.TransferTo(hostMind.Value, host);
            }

            var infest = new CorticalBorerInfestActionEvent { Target = host };
            SEntMan.EventBus.RaiseLocalEvent(borer, ref infest);
            Assert.That(infest.Handled, Is.True);
        });

        await RunSeconds(5.5f);
        await Server.WaitAssertion(() =>
        {
            var component = SEntMan.GetComponent<CorticalBorerComponent>(borer);
            Assert.That(component.Host, Is.EqualTo(host));
            screechAction = component.ScreechActionEntity;
            Assert.That(screechAction, Is.Not.Null);
            actions.SetCooldown(screechAction, TimeSpan.FromSeconds(60));
            actions.PerformAction(borer, actions.GetAction(component.ControlActionEntity)!.Value);
        });

        await RunSeconds(15.5f);
        await Server.WaitAssertion(() =>
        {
            var component = SEntMan.GetComponent<CorticalBorerComponent>(borer);
            var hostComponent = SEntMan.GetComponent<CorticalBorerHostComponent>(host);
            Assert.That(component.ControllingHost, Is.True);
            Assert.That(minds.GetMind(host), Is.EqualTo(borerMind));
            if (occupiedHost)
            {
                Assert.That(hostComponent.TrappedMind, Is.Not.Null);
                var trapped = hostComponent.TrappedMind!.Value;
                netTrapped = SEntMan.GetNetEntity(trapped);
                Assert.That(minds.GetMind(trapped), Is.EqualTo(hostMind));

                var transforms = Server.System<SharedTransformSystem>();
                var originalPosition = transforms.GetWorldPosition(trapped);
                transforms.SetCoordinates(host, map.GridCoords.Offset(new Vector2(0.25f, 0f)));
                Assert.That(SEntMan.GetComponent<TransformComponent>(trapped).ParentUid, Is.EqualTo(host));
                var currentPosition = transforms.GetWorldPosition(trapped);
                Assert.That(currentPosition, Is.Not.EqualTo(originalPosition));
                Assert.That(currentPosition, Is.EqualTo(transforms.GetWorldPosition(host)));
            }
            else
                Assert.That(hostComponent.TrappedMind, Is.Null);

            if (useSugar)
            {
                if (detachBorer)
                    minds.TransferTo(borerMind, null, createGhost: false);

                AddSugar(host);
            }
            else if (!resist)
                actions.PerformAction(host, actions.GetAction(component.ReleaseActionEntity)!.Value);
        });

        if (useSugar)
            await RunTicksSync(2);

        if (resist)
            await ResistFromClient(netTrapped);

        await Server.WaitAssertion(() =>
        {
            var component = SEntMan.GetComponent<CorticalBorerComponent>(borer);
            Assert.That(component.ControllingHost, Is.False);
            Assert.That(component.Host, Is.EqualTo(host));
            Assert.That(component.SugarSuppressed, Is.EqualTo(useSugar));
            Assert.That(minds.GetMind(borer), Is.EqualTo(detachBorer ? (EntityUid?) null : borerMind));
            Assert.That(minds.GetMind(host), Is.EqualTo(hostMind));
            Assert.That(SEntMan.GetComponent<CorticalBorerHostComponent>(host).TrappedMind, Is.Null);
            Assert.That(component.ScreechActionEntity, Is.EqualTo(screechAction));
            Assert.That(actions.IsCooldownActive(actions.GetAction(screechAction)!.Value.Comp), Is.EqualTo(!resist));
            if (useSugar)
                Assert.That(actions.GetAction(component.ControlActionEntity)!.Value.Comp.Enabled, Is.False);
        });
    }

    private async Task ResistFromClient(NetEntity netTrapped)
    {
        await RunUntilSynced();
        await Client.WaitAssertion(() =>
        {
            var trapped = CEntMan.GetEntity(netTrapped);
            Assert.That(Client.ResolveDependency<IPlayerManager>().LocalEntity, Is.EqualTo(trapped));

            var victim = CEntMan.GetComponent<CorticalBorerVictimComponent>(trapped);
            var actions = Client.System<ActionsSystem>();
            actions.TriggerAction(actions.GetAction(victim.ResistActionEntity)!.Value);
        });

        await RunTicksSync(3);
        await RunUntilSynced();
        await Client.WaitAssertion(() =>
        {
            var trapped = CEntMan.GetEntity(netTrapped);
            Assert.That(CEntMan.HasComponent<ActiveDoAfterComponent>(trapped), Is.True);
            Assert.That(CEntMan.GetComponent<DoAfterComponent>(trapped).DoAfters.Values.Any(doAfter =>
                doAfter.Args.Event is CorticalBorerResistDoAfterEvent && !doAfter.Cancelled), Is.True);
            Assert.That(CEntMan.GetComponent<SpriteComponent>(trapped).Visible, Is.False);
        });

        await RunSeconds(75.5f);
    }

    private void AddSugar(EntityUid host)
    {
        var bloodstream = SEntMan.GetComponent<BloodstreamComponent>(host);
        var solutions = Server.System<SharedSolutionContainerSystem>();
        Assert.That(solutions.ResolveSolution(host, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution), Is.True);
        Assert.That(solutions.TryAddReagent(bloodstream.BloodSolution!.Value, Sugar, 5), Is.True);
    }
}
