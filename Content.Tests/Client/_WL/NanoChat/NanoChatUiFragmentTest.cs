using System;
using System.Linq;
using Content.Client._WL.CartridgeLoader.Cartridges;
using Content.Shared._WL.CartridgeLoader.Cartridges;
using NUnit.Framework;

namespace Content.Tests.Client._WL.NanoChat;

[TestFixture]
[TestOf(typeof(NanoChatUiFragment))]
public sealed class NanoChatUiFragmentTest
{
    [Test]
    public void IncomingMessageDoesNotMoveViewportWhileReadingHistory()
    {
        const uint ownNumber = 1000;
        const uint sender = 2000;
        const float contentHeight = 500f;
        var viewportBottom = contentHeight - NanoChatUiFragment.FollowBottomThreshold - 1f;

        var follow = NanoChatUiFragment.ShouldFollowNewMessage(
            sender,
            ownNumber,
            contentHeight,
            viewportBottom);

        Assert.That(follow, Is.False);
    }

    [TestCase(2000u, 1000u, 500f, 472f)]
    [TestCase(1000u, 1000u, 500f, 100f)]
    public void NearBottomOrOwnMessageMovesViewport(
        uint sender,
        uint ownNumber,
        float contentHeight,
        float viewportBottom)
    {
        var follow = NanoChatUiFragment.ShouldFollowNewMessage(
            sender,
            ownNumber,
            contentHeight,
            viewportBottom);

        Assert.That(follow, Is.True);
    }

    [Test]
    public void FullSlidingPageStillDetectsNewMessage()
    {
        const uint sender = 2000;
        var previous = Enumerable.Range(0, 50)
            .Select(index => new NanoChatMessage(TimeSpan.FromSeconds(index), index.ToString(), sender))
            .ToList();
        var next = previous.Skip(1).Append(
            new NanoChatMessage(TimeSpan.FromSeconds(50), "50", sender)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(next, Has.Count.EqualTo(previous.Count));
            Assert.That(NanoChatUiFragment.HasNewLatestMessage(previous, next), Is.True);
        });
    }

    [Test]
    public void LoadingOlderMessagesDoesNotLookLikeAnAppend()
    {
        const uint sender = 2000;
        var previous = Enumerable.Range(25, 25)
            .Select(index => new NanoChatMessage(TimeSpan.FromSeconds(index), index.ToString(), sender))
            .ToList();
        var next = Enumerable.Range(0, 50)
            .Select(index => new NanoChatMessage(TimeSpan.FromSeconds(index), index.ToString(), sender))
            .ToList();

        Assert.That(NanoChatUiFragment.HasNewLatestMessage(previous, next), Is.False);
    }

    [TestCase("Мар Густав", "Мар")]
    [TestCase("Мар", "Мар")]
    [TestCase("Мар де Густав", "Мар")]
    [TestCase("Мар 🚀", "Мар")]
    [TestCase("Фаза-Сети", "Фаза-Сети")]
    [TestCase("АН-Гви-Гельдине", "АН-Гви")]
    [TestCase("АН-Гви-Гельдине Другой", "АН-Гви")]
    public void NanoChatUsesCompactDisplayNames(string name, string expected)
    {
        Assert.That(NanoChatUiFragment.ShortenDisplayName(name), Is.EqualTo(expected));
    }

    [TestCase("Зоряна Кудряшова", "Зоряна К.")]
    [TestCase("Одуванчик Печали", "Одуванчик П.")]
    [TestCase("Мар де Густав", "Мар Г.")]
    [TestCase("Фаза-Сети", "Фаза-Сети")]
    [TestCase("АН-Гви-Гельдине", "АН-Гви")]
    [TestCase("АН-Гви-Гельдине Другой", "АН-Гви Д.")]
    public void DirectChatsUseFirstNameAndSurnameInitial(string name, string expected)
    {
        Assert.That(NanoChatUiFragment.FormatDirectDisplayName(name), Is.EqualTo(expected));
    }
}
