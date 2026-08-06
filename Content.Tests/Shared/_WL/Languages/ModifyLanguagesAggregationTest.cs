using Content.Shared._WL.Languages.Components;
using Content.Shared._WL.Languages;
using Robust.Shared.Prototypes;
using NUnit.Framework;

namespace Content.Tests.Shared._WL.Languages
{
    [TestFixture]
    public sealed class ModifyLanguagesAggregationTest
    {
        [Test]
        public void AggregatingMultipleModifyLanguagesComponents_UnionsLanguages_OrsFlags()
        {
            var compA = new ModifyLanguagesComponent();
            compA.ToRemove = false;
            compA.ToUnderstood = true;
            compA.ToSpeaking = false;
            compA.SpecieLanguage = true;
            compA.Languages.Add(new ProtoId<LanguagePrototype>("lang.a"));

            var compB = new ModifyLanguagesComponent();
            compB.ToRemove = true;
            compB.ToUnderstood = false;
            compB.ToSpeaking = true;
            compB.SpecieLanguage = false;
            compB.Languages.Add(new ProtoId<LanguagePrototype>("lang.b"));

            // Use the shared aggregator, apply to a LanguagesComponent and assert the final state
            var aggregated = ModifyLanguagesAggregator.Aggregate(new[] { compA, compB });

            var outComp = new LanguagesComponent();
            // set a specie language to ensure specie removal behavior can be tested
            outComp.SpecieLanguage = new ProtoId<LanguagePrototype>("specie.lang");

            // Pre-populate to ensure removals/additions act as expected
            outComp.Understood.Add(new ProtoId<LanguagePrototype>("lang.a"));
            outComp.Speaking.Add(new ProtoId<LanguagePrototype>("lang.b"));

            ModifyLanguagesAggregator.ApplyTo(outComp, aggregated);

            // compA made lang.a understood-only, compB made lang.b speaking-only, compB also had ToRemove true,
            // so lang.b should have been removed due to ToRemove, and lang.a should remain in Understood.
            Assert.That(outComp.Understood.Contains(new ProtoId<LanguagePrototype>("lang.a")), Is.True);
            Assert.That(outComp.Speaking.Contains(new ProtoId<LanguagePrototype>("lang.a")), Is.False);

            Assert.That(outComp.Speaking.Contains(new ProtoId<LanguagePrototype>("lang.b")), Is.False);
            Assert.That(outComp.Understood.Contains(new ProtoId<LanguagePrototype>("lang.b")), Is.False);

            Assert.That(aggregated.SpecieLanguage, Is.True);

        }
    }
}
