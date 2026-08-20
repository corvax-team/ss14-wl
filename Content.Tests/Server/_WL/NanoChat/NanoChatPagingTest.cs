using System;
using System.Collections.Generic;
using System.Linq;
using Content.Server._WL.CartridgeLoader.Cartridges;
using Content.Server._WL.NanoChat;
using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared._WL.CartridgeLoader.Cartridges;
using Content.Shared._WL.NanoChat;
using NUnit.Framework;

namespace Content.Tests.Server._WL.NanoChat;

[TestFixture]
[TestOf(typeof(NanoChatCartridgeSystem))]
public sealed class NanoChatPagingTest
{
    [Test]
    public void GroupMemberLimitIncludesCreator()
    {
        var system = new NanoChatSystem();
        var creator = new NanoChatGroupMember(1, "Creator");
        var maximumInviteList = Enumerable.Range(2, NanoChatGroup.MaxMembers - 1)
            .Select(number => new NanoChatGroupMember((uint) number, number.ToString()))
            .ToList();

        Assert.That(system.TryCreateGroup("Full group", creator, maximumInviteList, out var group), Is.True);
        Assert.That(group.Members, Has.Count.EqualTo(NanoChatGroup.MaxMembers));

        maximumInviteList.Add(new NanoChatGroupMember(1000, "Too many"));
        Assert.That(system.TryCreateGroup("Too large", creator, maximumInviteList, out _), Is.False);
    }

    [Test]
    public void GroupAlwaysKeepsAnAdministrator()
    {
        var system = new NanoChatSystem();
        var creator = new NanoChatGroupMember(1, "Creator");
        var second = new NanoChatGroupMember(2, "Second");
        Assert.That(system.TryCreateGroup("Group", creator, [second], out var group), Is.True);

        Assert.That(system.TrySetGroupAdmin(group.Id, creator.Number, creator.Number, false), Is.False);
        Assert.That(system.TrySetGroupAdmin(group.Id, creator.Number, second.Number, true), Is.True);
        Assert.That(system.TrySetGroupAdmin(group.Id, creator.Number, creator.Number, false), Is.True);
        Assert.That(group.IsAdmin(second.Number), Is.True);
    }

    [Test]
    public void CreatorCannotExceedActiveGroupLimit()
    {
        var system = new NanoChatSystem();
        var creator = new NanoChatGroupMember(1, "Creator");
        var second = new NanoChatGroupMember(2, "Second");
        uint firstGroupId = 0;

        for (var i = 0; i < NanoChatGroup.MaxGroupsPerCreator; i++)
        {
            Assert.That(system.TryCreateGroup($"Group {i}", creator, [second], out var group), Is.True);
            if (firstGroupId == 0)
                firstGroupId = group.Id;
        }

        Assert.That(system.TryCreateGroup("One too many", creator, [second], out _), Is.False);
        Assert.That(system.TryDeleteGroup(firstGroupId, creator.Number), Is.True);
        Assert.That(system.TryCreateGroup("Replacement", creator, [second], out _), Is.True);
    }

    [Test]
    public void RetainedHistoryIsPagedWithoutMutatingStorage()
    {
        const uint chat = 1234;
        var conversation = new NanoChatConversationId(NanoChatConversationType.Direct, chat);
        var history = Enumerable.Range(0, 120)
            .Select(index => new NanoChatMessage(TimeSpan.FromMinutes(index), index.ToString(), chat))
            .ToList();
        var source = new Dictionary<uint, List<NanoChatMessage>> { [chat] = history };
        var requested = new Dictionary<NanoChatConversationId, int>();
        var hasOlder = new HashSet<NanoChatConversationId>();

        var firstPage = NanoChatCartridgeSystem.PageMessages(
            source,
            NanoChatConversationType.Direct,
            requested,
            hasOlder);

        Assert.Multiple(() =>
        {
            Assert.That(source[chat], Has.Count.EqualTo(120));
            Assert.That(firstPage[chat], Has.Count.EqualTo(NanoChatCartridgeComponent.MessagePageSize));
            Assert.That(firstPage[chat][0].Content, Is.EqualTo("70"));
            Assert.That(hasOlder, Does.Contain(conversation));
        });

        requested[conversation] = 100;
        hasOlder.Clear();
        var secondPage = NanoChatCartridgeSystem.PageMessages(
            source,
            NanoChatConversationType.Direct,
            requested,
            hasOlder);

        Assert.Multiple(() =>
        {
            Assert.That(secondPage[chat], Has.Count.EqualTo(100));
            Assert.That(secondPage[chat][0].Content, Is.EqualTo("20"));
            Assert.That(hasOlder, Does.Contain(conversation));
        });
    }

    [Test]
    public void HistoryRetentionKeepsNewestMessages()
    {
        const int retained = NanoChatCardComponent.DefaultMaxMessagesPerConversation;
        var history = CreateHistory(1234, retained + 100);

        SharedNanoChatSystem.TrimHistory(history, retained);

        Assert.Multiple(() =>
        {
            Assert.That(history, Has.Count.EqualTo(retained));
            Assert.That(history[0].Content, Is.EqualTo("100"));
            Assert.That(history[^1].Content, Is.EqualTo((retained + 99).ToString()));
        });
    }

    [Test]
    public void LogProbePreviewIsBoundedWithoutLosingTotalCounts()
    {
        const uint directChat = 1234;
        const uint groupChat = 5678;
        var directMessages = CreateHistory(directChat, 100);
        var groupMessages = CreateHistory(groupChat, 100);
        var data = new NanoChatData(
            new Dictionary<uint, NanoChatRecipient>
            {
                [directChat] = new(directChat, "Direct"),
            },
            new Dictionary<uint, List<NanoChatMessage>>
            {
                [directChat] = directMessages,
            },
            new Dictionary<uint, int>
            {
                [directChat] = directMessages.Count,
            },
            new Dictionary<uint, NanoChatGroup>
            {
                [groupChat] = new(groupChat, "Group", new Dictionary<uint, NanoChatGroupMember>()),
            },
            new Dictionary<uint, List<NanoChatMessage>>
            {
                [groupChat] = groupMessages,
            },
            new Dictionary<uint, int>
            {
                [groupChat] = groupMessages.Count,
            },
            new HashSet<uint> { directChat },
            9999,
            default);

        var preview = NanoChatLogProbePreview.Create(data, 30, 20);

        Assert.Multiple(() =>
        {
            Assert.That(preview.Messages.Values.Sum(messages => messages.Count) +
                        preview.GroupMessages.Values.Sum(messages => messages.Count),
                Is.EqualTo(30));
            Assert.That(preview.Messages[directChat], Has.Count.EqualTo(20));
            Assert.That(preview.Messages[directChat][0].Content, Is.EqualTo("80"));
            Assert.That(preview.GroupMessages[groupChat], Has.Count.EqualTo(10));
            Assert.That(preview.MessageCounts[directChat], Is.EqualTo(100));
            Assert.That(preview.GroupMessageCounts[groupChat], Is.EqualTo(100));
            Assert.That(preview.BlockedNumbers, Does.Contain(directChat));
        });
    }

    private static List<NanoChatMessage> CreateHistory(uint sender, int count)
    {
        return Enumerable.Range(0, count)
            .Select(index => new NanoChatMessage(TimeSpan.FromMinutes(index), index.ToString(), sender))
            .ToList();
    }
}
