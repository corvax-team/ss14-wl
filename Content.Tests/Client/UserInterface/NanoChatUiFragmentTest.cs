using Content.Client._WL.CartridgeLoader.Cartridges;
using NUnit.Framework;

namespace Content.Tests.Client.UserInterface;

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
}
