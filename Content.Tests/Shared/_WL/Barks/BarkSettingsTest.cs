using Content.Shared._WL.Barks;
using NUnit.Framework;

namespace Content.Tests.Shared._WL.Barks;

[TestFixture]
[TestOf(typeof(SpeechBarksComponent))]
public sealed class BarkSettingsTest
{
    [Test]
    public void ExperimentalDefaultsAreMigrated()
    {
        var delays = SpeechBarksComponent.SanitizeDelays(
            SpeechBarksComponent.ExperimentalMinDelay,
            SpeechBarksComponent.ExperimentalMaxDelay);

        Assert.Multiple(() =>
        {
            Assert.That(delays.Min, Is.EqualTo(SpeechBarksComponent.DefaultMinDelay));
            Assert.That(delays.Max, Is.EqualTo(SpeechBarksComponent.DefaultMaxDelay));
        });
    }

    [TestCase(0f, 0f, 0.08f, 0.08f)]
    [TestCase(0.09f, 0.06f, 0.09f, 0.09f)]
    [TestCase(0.2f, 2f, 0.2f, 0.6f)]
    public void DelayRangeIsClamped(float min, float max, float expectedMin, float expectedMax)
    {
        var delays = SpeechBarksComponent.SanitizeDelays(min, max);

        Assert.Multiple(() =>
        {
            Assert.That(delays.Min, Is.EqualTo(expectedMin).Within(0.0001f));
            Assert.That(delays.Max, Is.EqualTo(expectedMax).Within(0.0001f));
        });
    }

    [Test]
    public void NonFinitePitchUsesDefault()
    {
        Assert.Multiple(() =>
        {
            Assert.That(
                SpeechBarksComponent.SanitizePitch(float.NaN),
                Is.EqualTo(SpeechBarksComponent.DefaultPitch));
            Assert.That(
                SpeechBarksComponent.SanitizePitch(float.PositiveInfinity),
                Is.EqualTo(SpeechBarksComponent.DefaultPitch));
            Assert.That(
                SpeechBarksComponent.SanitizePitch(float.NegativeInfinity),
                Is.EqualTo(SpeechBarksComponent.DefaultPitch));
        });
    }

    [Test]
    public void NonFiniteDelaysUseDefaults()
    {
        var delays = SpeechBarksComponent.SanitizeDelays(
            float.NaN,
            float.PositiveInfinity);

        Assert.Multiple(() =>
        {
            Assert.That(delays.Min, Is.EqualTo(SpeechBarksComponent.DefaultMinDelay));
            Assert.That(delays.Max, Is.EqualTo(SpeechBarksComponent.DefaultMaxDelay));
        });
    }
}
