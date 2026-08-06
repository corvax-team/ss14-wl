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

            // Use the shared aggregator and assert its result
            var aggregated = ModifyLanguagesAggregator.Aggregate(new[] { compA, compB });

            Assert.That(aggregated.Languages.Contains(new ProtoId<LanguagePrototype>("lang.a")));
            Assert.That(aggregated.Languages.Contains(new ProtoId<LanguagePrototype>("lang.b")));
            Assert.That(aggregated.Languages.Count, Is.EqualTo(2));

            Assert.That(aggregated.ToRemove, Is.True);
            Assert.That(aggregated.ToUnderstood, Is.True);
            Assert.That(aggregated.ToSpeaking, Is.True);
            Assert.That(aggregated.SpecieLanguage, Is.True);
        }
    }
}
