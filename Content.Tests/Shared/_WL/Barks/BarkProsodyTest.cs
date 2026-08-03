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

    [TestCase("", 0)]
    [TestCase("...", 0)]
    [TestCase("?!, - —", 0)]
    [TestCase("Ку.", 1)]
    [TestCase("Привет.", 2)]
    [TestCase("Здрав-ствуй-те.", 3)]
    [TestCase("Здравствуйте.", 3)]
    [TestCase("Привет, как дела?", 5)]
    [TestCase("hello", 2)]
    [TestCase("voice", 1)]
    [TestCase("table", 2)]
    [TestCase("123 РП", 2)]
    public void BarkCountFollowsSpokenSyllables(string message, int expected)
    {
        Assert.That(BarkProsody.GetBarkCount(message), Is.EqualTo(expected));
    }

    [Test]
    public void BarkCountIsCappedForLongMessages()
    {
        Assert.That(
            BarkProsody.GetBarkCount(new string('а', 100)),
            Is.EqualTo(24));
    }

    [Test]
    public void PunctuationCreatesRhythmWithoutExtraBarks()
    {
        var rhythm = BarkProsody.GetBarkRhythm("Привет, мир. Как дела?");

        Assert.That(rhythm, Is.EqualTo(new[]
        {
            BarkBoundary.None,
            BarkBoundary.Comma,
            BarkBoundary.Period,
            BarkBoundary.None,
            BarkBoundary.None,
            BarkBoundary.Question,
        }));
    }

    [Test]
    public void EllipsisCreatesTheLongestPause()
    {
        var rhythm = BarkProsody.GetBarkRhythm("Да... Нет!");

        Assert.Multiple(() =>
        {
            Assert.That(rhythm, Is.EqualTo(new[]
            {
                BarkBoundary.Ellipsis,
                BarkBoundary.Exclamation,
            }));
            Assert.That(
                rhythm[0].GetAdditionalDelay(),
                Is.GreaterThan(rhythm[1].GetAdditionalDelay()));
        });
    }

    [Test]
    public void PunctuationChangesExistingBarksWithoutAddingNewOnes()
    {
        var plain = BarkProsody.GetBarkRhythm("Привет мир");
        var punctuated = BarkProsody.GetBarkRhythm("Привет, мир. Да!");
        var dashed = BarkProsody.GetBarkRhythm("Привет — мир");

        Assert.Multiple(() =>
        {
            Assert.That(plain, Has.Length.EqualTo(3));
            Assert.That(punctuated, Has.Length.EqualTo(4));
            Assert.That(punctuated[1], Is.EqualTo(BarkBoundary.Comma));
            Assert.That(punctuated[2], Is.EqualTo(BarkBoundary.Period));
            Assert.That(punctuated[3], Is.EqualTo(BarkBoundary.Exclamation));
            Assert.That(dashed, Has.Length.EqualTo(3));
            Assert.That(dashed[1], Is.EqualTo(BarkBoundary.Clause));
        });
    }
}
