#nullable enable
using System.Collections.Generic;
using System.Linq;
using Content.IntegrationTests.Tests.Interaction;
using Content.Server._WL.Implants;
using Content.Shared._WL.Implants;
using Content.Shared.Body;
using Content.Shared.Humanoid;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared._WL.Barks;
using Content.Shared._WL.DynamicText;
using Content.Shared.Corvax.TTS;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Wagging;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Speech;
using Content.Shared.Chat.TypingIndicator;
using Content.Shared.Examine;
using Content.Shared.Zombies;
using Content.Server.Zombies;
using Robust.Shared.Utility;
using Robust.Shared.Maths;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests.Implants;

public sealed class ImitatorImplantTest : InteractionTest
{
    protected override string PlayerPrototype => "MobHuman";

    [TestCase("MobVulpkanin")]
    [TestCase("MobGolem")]
    public async Task DisguiseSurvivesExtractionAndTransfer(string victimPrototype)
    {
        EntityUid implant = default;
        EntityUid corpse = default;
        EntityUid image = default;
        EntityUid recipient = default;
        NetEntity imageNet = default;
        string originalName = "";
        string? originalVoice = "";
        string? originalSpeechSound = null;
        await Server.WaitAssertion(() =>
        {
            var coords = SEntMan.GetComponent<TransformComponent>(SPlayer).Coordinates;
            corpse = SEntMan.SpawnEntity(victimPrototype, coords);
            if (victimPrototype == "MobVulpkanin")
            {
                SEntMan.System<Content.Server.Body.VisualBodySystem>().ApplyMarkings(corpse, new()
                {
                    ["Torso"] = new() { [HumanoidVisualLayers.Tail] = [new Marking("FoxTail", [Color.Red, Color.Blue])] },
                });
            }
            SEntMan.System<MetaDataSystem>().SetEntityName(corpse, "Copied identity");
            SEntMan.EnsureComponent<DynamicTextComponent>(corpse).Text = "Copied description";
            SEntMan.EnsureComponent<SpeechBarksComponent>(corpse).Pitch = 1.25f;
            SEntMan.EnsureComponent<TTSComponent>(corpse).VoicePrototypeId = "Maiev";
            originalName = SEntMan.GetComponent<MetaDataComponent>(SPlayer).EntityName;
            originalVoice = SEntMan.GetComponent<TTSComponent>(SPlayer).VoicePrototypeId;
            originalSpeechSound = SEntMan.GetComponent<SpeechComponent>(SPlayer).SpeechSounds?.Id;
            implant = SEntMan.System<SharedSubdermalImplantSystem>().AddImplant(SPlayer, "ImitatorImplant")!.Value;

            foreach (var state in new[] { MobState.Alive, MobState.Critical })
            {
                SEntMan.System<MobStateSystem>().ChangeMobState(corpse, state);
                Assert.That(TryCopy(implant, corpse), Is.False);
            }
            SEntMan.System<MobStateSystem>().ChangeMobState(corpse, MobState.Dead);
            Assert.That(TryCopy(implant, corpse), Is.True);
            SEntMan.System<DamageableSystem>().TryChangeDamage(SPlayer,
                new DamageSpecifier { DamageDict = { ["Blunt"] = 5 } }, ignoreResistances: true);
            Assert.That(SEntMan.GetComponent<ImitatorImplantComponent>(implant).CopyDoAfter, Is.Null);
            Assert.That(SEntMan.GetComponent<ImitatorImplantComponent>(implant).Image, Is.Null);
            Assert.That(TryCopy(implant, corpse), Is.True);
            Assert.That(SEntMan.HasComponent<ImitatorDisguiseComponent>(SPlayer), Is.False);
        });
        await RunSeconds(5);
        await Server.WaitAssertion(() => Assert.That(SEntMan.HasComponent<ImitatorDisguiseComponent>(SPlayer), Is.False));
        await RunSeconds(5.2f);
        await Server.WaitAssertion(() =>
        {
            image = SEntMan.GetComponent<ImitatorDisguiseComponent>(SPlayer).Image!.Value;
            imageNet = SEntMan.GetNetEntity(image);
            Assert.That(SEntMan.GetComponent<TTSComponent>(SPlayer).VoicePrototypeId?.Id, Is.EqualTo("Maiev"));
            Assert.That(SEntMan.GetComponent<SpeechBarksComponent>(SPlayer).Pitch, Is.EqualTo(1.25f));
            Assert.That(SEntMan.GetComponent<SpeechComponent>(SPlayer).SpeechSounds,
                Is.EqualTo(SEntMan.GetComponent<SpeechComponent>(corpse).SpeechSounds));
            Assert.That(SEntMan.GetComponent<TypingIndicatorComponent>(SPlayer).TypingIndicatorPrototype,
                Is.EqualTo(SEntMan.GetComponent<TypingIndicatorComponent>(corpse).TypingIndicatorPrototype));
            Assert.That(SEntMan.GetComponent<DynamicTextComponent>(SPlayer).Text, Is.EqualTo("Copied description"));
            Assert.That(SEntMan.GetComponent<MetaDataComponent>(SPlayer).EntityName, Is.EqualTo("Copied identity"));
            Assert.That(TryCopy(implant, corpse), Is.False);
            var damage = SEntMan.System<DamageableSystem>();
            foreach (var (amount, expected) in new[] { (90, 0f), (145, 0.4f), (200, 0.8f), (0, 0f) })
            {
                damage.SetDamage(SPlayer, new DamageSpecifier { DamageDict = { ["Blunt"] = amount } });
                Assert.That(SEntMan.GetComponent<ImitatorDisguiseComponent>(SPlayer).Instability, Is.EqualTo(expected).Within(0.001f));
            }
            damage.SetDamage(SPlayer, new DamageSpecifier { DamageDict = { ["Blunt"] = 80 } });
            if (victimPrototype == "MobVulpkanin")
            {
                Assert.That(SEntMan.HasComponent<WaggingComponent>(SPlayer), Is.True);
                Assert.That(SEntMan.System<WaggingSystem>().TryToggleWagging(SPlayer, true), Is.True);
            }
        });
        await RunTicks(10);
        await Client.WaitAssertion(() =>
        {
            var sprites = CEntMan.System<SpriteSystem>();
            var clientImage = CEntMan.GetEntity(imageNet);
            var sourceHead = CEntMan.GetComponent<BodyComponent>(clientImage).Organs!.ContainedEntities
                .Select(uid => CEntMan.TryGetComponent<VisualOrganComponent>(uid, out var c) ? c : null)
                .Single(c => Equals(c?.Layer, HumanoidVisualLayers.Head))!;
            var sprite = CEntMan.GetComponent<SpriteComponent>(CPlayer);
            var head = sprite.AllLayers.ElementAt(sprites.LayerMapGet(CPlayer, HumanoidVisualLayers.Head));
            var brute = sprite.AllLayers.ElementAt(sprites.LayerMapGet(CPlayer, "ChestBrute"));
            Assert.That(brute.Visible, Is.True);
            Assert.That(brute.Color, Is.EqualTo(Color.FromHex(victimPrototype switch
            {
                "MobVulpkanin" => "#ccac1f",
                "MobGolem" => "#3b3c3d",
                _ => "#FF0000",
            })));
            Assert.That(brute.Rsi?.Path.ToString(), Is.EqualTo(victimPrototype == "MobGolem"
                ? "/Textures/_WL/Mobs/Effects/brute_damage.rsi"
                : "/Textures/Mobs/Effects/brute_damage.rsi"));
            Assert.That(head.RsiState.Name, Is.EqualTo(sourceHead.Data.State));
            Assert.That(head.Rsi?.Path.ToString(), Is.EqualTo("/Textures/" + sourceHead.Data.RsiPath));
            if (victimPrototype == "MobVulpkanin")
            {
                var visualBody = CEntMan.System<Content.Client.Body.VisualBodySystem>();
                Assert.That(visualBody.TryGatherMarkingsData(CPlayer, [HumanoidVisualLayers.Tail], out _, out _, out var markings, useDisguise: true), Is.True);
                Assert.That(markings!.ContainsKey("Torso"), Is.True);
                Assert.That(sprites.LayerMapTryGet(CPlayer, "FoxTailAnimated-tail_fox_waggin", out var tailIndex, false), Is.True);
                Assert.That(sprite.AllLayers.ElementAt(tailIndex).Color, Is.EqualTo(Color.Red));
            }
        });
        await Server.WaitAssertion(() =>
        {
            SEntMan.DeleteEntity(corpse);
            var tool = SEntMan.SpawnEntity("Implanter", SEntMan.GetComponent<TransformComponent>(SPlayer).Coordinates);
            var implanters = SEntMan.System<SharedImplanterSystem>();
            implanters.SetSelectedDeimplant(tool, "ImitatorImplant");
            implanters.Draw((tool, SEntMan.GetComponent<ImplanterComponent>(tool)), SPlayer, SPlayer);
            Assert.That(SEntMan.HasComponent<ImitatorDisguiseComponent>(SPlayer), Is.False);
            Assert.That(SEntMan.GetComponent<MetaDataComponent>(SPlayer).EntityName, Is.EqualTo(originalName));
            Assert.That(SEntMan.GetComponent<TTSComponent>(SPlayer).VoicePrototypeId?.Id, Is.EqualTo(originalVoice));
            Assert.That(SEntMan.GetComponent<SpeechComponent>(SPlayer).SpeechSounds?.Id, Is.EqualTo(originalSpeechSound));
            Assert.That(SEntMan.GetComponent<TypingIndicatorComponent>(SPlayer).TypingIndicatorPrototype.Id, Is.EqualTo("default"));
            recipient = SEntMan.SpawnEntity("MobHuman", SEntMan.GetComponent<TransformComponent>(SPlayer).Coordinates);
            implanters.Implant(SPlayer, recipient, (tool, SEntMan.GetComponent<ImplanterComponent>(tool)));
            SEntMan.DeleteEntity(tool);
        });
        await RunTicks(5);
        await Client.WaitAssertion(() =>
        {
            Assert.That(CEntMan.HasComponent<ImitatorDisguiseComponent>(CPlayer), Is.False);
            var sprites = CEntMan.System<SpriteSystem>();
            var sprite = CEntMan.GetComponent<SpriteComponent>(CPlayer);
            var head = sprite.AllLayers.ElementAt(sprites.LayerMapGet(CPlayer, HumanoidVisualLayers.Head));
            Assert.That(head.Rsi?.Path.ToString(), Is.EqualTo("/Textures/Mobs/Species/Human/parts.rsi"));
            var brute = sprite.AllLayers.ElementAt(sprites.LayerMapGet(CPlayer, "ChestBrute"));
            Assert.That(brute.Color, Is.EqualTo(Color.Red));
            Assert.That(brute.Rsi?.Path.ToString(), Is.EqualTo("/Textures/Mobs/Effects/brute_damage.rsi"));
        });
        await Server.WaitAssertion(() =>
        {
            Assert.That(SEntMan.GetComponent<ImitatorDisguiseComponent>(recipient).Image, Is.EqualTo(image));
            Assert.That(SEntMan.GetComponent<ImitatorImplantComponent>(implant).Image, Is.EqualTo(image));
            Assert.That(SEntMan.GetComponent<SubdermalImplantComponent>(implant).Action, Is.Null);
            SEntMan.DeleteEntity(implant);
            Assert.That(SEntMan.HasComponent<ImitatorDisguiseComponent>(recipient), Is.False);
            SEntMan.DeleteEntity(recipient);
        });
    }

    [Test]
    public async Task MarkingEditorDoesNotMixDisguiseAndZombieChanges()
    {
        await Server.WaitAssertion(() =>
        {
            var image = SEntMan.SpawnEntity("MobVulpkanin", SEntMan.GetComponent<TransformComponent>(SPlayer).Coordinates);
            SEntMan.EnsureComponent<ImitatorDisguiseComponent>(SPlayer).Image = image;
            var body = SEntMan.System<Content.Server.Body.VisualBodySystem>();
            var edit = new HumanoidMarkingModifierMarkingSetMessage(new()
            {
                ["Torso"] = new() { [HumanoidVisualLayers.Tail] = [new Marking("FoxTail", [Color.Red, Color.Blue])] },
            }) { UiKey = HumanoidMarkingModifierKey.Key, Actor = SPlayer };
            SEntMan.EventBus.RaiseLocalEvent(SPlayer, edit);
            Assert.That(body.TryGatherMarkingsData(image, null, out _, out _, out var markings), Is.True);
            Assert.That(markings!["Torso"][HumanoidVisualLayers.Tail].Single().MarkingId.Id, Is.EqualTo("FoxTail"));

            var zombies = SEntMan.System<ZombieSystem>();
            zombies.ZombifyEntity(SPlayer);
            var zombie = SEntMan.GetComponent<ZombieComponent>(SPlayer);
            Assert.That(zombie.BeforeZombifiedMarkings["Torso"].GetValueOrDefault(HumanoidVisualLayers.Tail), Is.Null.Or.Empty);
            Assert.That(body.TryGatherMarkingsData(image, null, out _, out _, out markings), Is.True);
            Assert.That(markings!["Torso"][HumanoidVisualLayers.Tail].Single().MarkingColors[0], Is.EqualTo(Color.Red));
            zombies.UnZombify(SPlayer, SPlayer, zombie);
            Assert.That(body.TryGatherMarkingsData(image, null, out _, out _, out markings), Is.True);
            Assert.That(markings!["Torso"][HumanoidVisualLayers.Tail].Single().MarkingColors[0], Is.EqualTo(Color.Red));
            SEntMan.RemoveComponent<ImitatorDisguiseComponent>(SPlayer);
            SEntMan.DeleteEntity(image);
        });
    }

    [Test]
    public async Task HiddenImageProfileDoesNotAppearOnExamine()
    {
        await Server.WaitAssertion(() =>
        {
            var image = SEntMan.SpawnEntity("MobHorror", SEntMan.GetComponent<TransformComponent>(SPlayer).Coordinates);
            var target = SEntMan.SpawnEntity(null, SEntMan.GetComponent<TransformComponent>(SPlayer).Coordinates);
            SEntMan.EnsureComponent<HumanoidProfileComponent>(target);
            SEntMan.EnsureComponent<ImitatorDisguiseComponent>(target).Image = image;
            var examine = new ExaminedEvent(new FormattedMessage(), target, SPlayer, true, false);
            SEntMan.EventBus.RaiseLocalEvent(target, examine);
            Assert.That(examine.GetTotalMessage().ToString(), Is.Empty);
            SEntMan.DeleteEntity(target);
            SEntMan.DeleteEntity(image);
        });
    }

    private bool TryCopy(EntityUid implant, EntityUid target)
    {
        var action = new ImitatorCopyActionEvent { Performer = SPlayer, Target = target };
        SEntMan.EventBus.RaiseLocalEvent(implant, action);
        return action.Handled;
    }
}
