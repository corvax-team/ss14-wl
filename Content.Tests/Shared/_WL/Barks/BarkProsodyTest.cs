using Content.Shared._WL.Barks;
using NUnit.Framework;

namespace Content.Tests.Shared._WL.Barks;

[TestFixture]
public sealed class BarkProsodyTest
{
    [TestCase("")]
    [TestCase("Обычная фраза")]
    [TestCase("Обычная фраза.")]
    [TestCase("Вопрос?")]
    public void NeutralEndingsDoNotChangeSpeech(string message)
    {
        var prosody = BarkProsody.FromMessage(message);

        Assert.That(prosody, Is.EqualTo(BarkProsody.Neutral));
        Assert.That(prosody.GetPitchMultiplier(0f), Is.EqualTo(1f));
        Assert.That(prosody.GetPitchMultiplier(1f), Is.EqualTo(1f));
    }

    [TestCase("Не уверен..")]
    [TestCase("Не уверен...")]
    [TestCase("Не уверен...» ")]
    public void DotsLowerAndSlowSpeech(string message)
    {
        var prosody = BarkProsody.FromMessage(message);

        Assert.Multiple(() =>
        {
            Assert.That(prosody.GetPitchMultiplier(0f), Is.LessThan(1f));
            Assert.That(prosody.GetPitchMultiplier(1f), Is.LessThan(prosody.GetPitchMultiplier(0f)));
            Assert.That(prosody.DelayScale, Is.GreaterThan(1f));
            Assert.That(prosody.VolumeOffset, Is.LessThan(0f));
        });
    }

    [Test]
    public void MoreDotsProduceStrongerEmotion()
    {
        var twoDots = BarkProsody.FromMessage("Не уверен..");
        var ellipsis = BarkProsody.FromMessage("Не уверен...");

        Assert.Multiple(() =>
        {
            Assert.That(ellipsis.GetPitchMultiplier(1f), Is.LessThan(twoDots.GetPitchMultiplier(1f)));
            Assert.That(ellipsis.DelayScale, Is.GreaterThan(twoDots.DelayScale));
        });
    }

    [TestCase("Да!")]
    [TestCase("Да!!")]
    [TestCase("Да!!!")]
    [TestCase("Да!!!» ")]
    public void ExclamationsRaiseAndAccelerateSpeech(string message)
    {
        var prosody = BarkProsody.FromMessage(message);

        Assert.Multiple(() =>
        {
            Assert.That(prosody.GetPitchMultiplier(0f), Is.GreaterThan(1f));
            Assert.That(prosody.GetPitchMultiplier(1f), Is.GreaterThan(prosody.GetPitchMultiplier(0f)));
            Assert.That(prosody.DelayScale, Is.LessThan(1f));
            Assert.That(prosody.VolumeOffset, Is.GreaterThan(0f));
        });
    }

    [Test]
    public void MoreExclamationsProduceStrongerEmotion()
    {
        var one = BarkProsody.FromMessage("Да!");
        var two = BarkProsody.FromMessage("Да!!");
        var three = BarkProsody.FromMessage("Да!!!");

        Assert.Multiple(() =>
        {
            Assert.That(two.GetPitchMultiplier(1f), Is.GreaterThan(one.GetPitchMultiplier(1f)));
            Assert.That(three.GetPitchMultiplier(1f), Is.GreaterThan(two.GetPitchMultiplier(1f)));
            Assert.That(two.DelayScale, Is.LessThan(one.DelayScale));
            Assert.That(three.DelayScale, Is.LessThan(two.DelayScale));
        });
    }

    [TestCase("", 1)]
    [TestCase("а", 1)]
    [TestCase("аб", 1)]
    [TestCase("абв", 2)]
    [TestCase("абвг", 2)]
    [TestCase("абвгде", 3)]
    public void BarkCountUsesOneGrainPerThreeCharacters(string message, int expected)
    {
        Assert.That(BarkProsody.GetBarkCount(message), Is.EqualTo(expected));
    }
}
