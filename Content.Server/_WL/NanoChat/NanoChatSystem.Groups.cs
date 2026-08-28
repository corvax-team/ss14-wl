using System.Linq;
using Content.Shared._WL.NanoChat;

namespace Content.Server._WL.NanoChat;

public sealed partial class NanoChatSystem
{
    private readonly Dictionary<uint, NanoChatServerGroup> _groups = new();
    private uint _nextGroupId = 1;

    public bool TryGetGroup(uint groupId, out NanoChatServerGroup group)
        => _groups.TryGetValue(groupId, out group!);

    public bool TryCreateGroup(
        string name,
        NanoChatGroupMember creator,
        IReadOnlyList<NanoChatGroupMember> invited,
        out NanoChatServerGroup group)
    {
        group = default!;

        if (string.IsNullOrWhiteSpace(name) ||
            name.Length > NanoChatGroup.MaxNameLength ||
            _groups.Values.Count(existing => existing.Creator == creator.Number) >= NanoChatGroup.MaxGroupsPerCreator)
            return false;

        var members = new Dictionary<uint, NanoChatGroupMember>
        {
            [creator.Number] = creator with { IsAdmin = true },
        };

        foreach (var member in invited)
        {
            if (member.Number == creator.Number)
                continue;

            members.TryAdd(member.Number, member with { IsAdmin = false });
        }

        if (members.Count < 2 || members.Count > NanoChatGroup.MaxMembers)
            return false;

        var id = _nextGroupId++;
        group = new NanoChatServerGroup(id, creator.Number, name, members);
        _groups.Add(id, group);
        return true;
    }

    public bool TryRenameGroup(uint groupId, uint actor, string name)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            name.Length > NanoChatGroup.MaxNameLength ||
            !TryGetGroup(groupId, out var group) ||
            !group.IsAdmin(actor))
            return false;

        group.Name = name;
        return true;
    }

    public bool TryAddGroupMember(uint groupId, uint actor, NanoChatGroupMember member)
    {
        if (!TryGetGroup(groupId, out var group) ||
            !group.IsAdmin(actor) ||
            group.Members.Count >= NanoChatGroup.MaxMembers ||
            group.Members.ContainsKey(member.Number))
            return false;

        group.Members.Add(member.Number, member with { IsAdmin = false });
        group.JoinOrder.Add(member.Number);
        return true;
    }

    public bool TrySetGroupAdmin(uint groupId, uint actor, uint target, bool admin)
    {
        if (!TryGetGroup(groupId, out var group) ||
            !group.IsAdmin(actor) ||
            !group.Members.TryGetValue(target, out var member) ||
            member.IsAdmin == admin)
            return false;

        if (!admin && group.AdminCount == 1)
            return false;

        group.Members[target] = member with { IsAdmin = admin };
        return true;
    }

    public bool TryRemoveGroupMember(uint groupId, uint actor, uint target)
    {
        if (!TryGetGroup(groupId, out var group) ||
            !group.IsAdmin(actor) ||
            actor == target ||
            !group.Members.TryGetValue(target, out var member))
            return false;

        if (member.IsAdmin && group.AdminCount == 1)
            return false;

        group.Members.Remove(target);
        group.JoinOrder.Remove(target);
        return true;
    }

    public bool TryLeaveGroup(uint groupId, uint actor, out bool deleted)
    {
        deleted = false;
        if (!TryGetGroup(groupId, out var group) || !group.Members.Remove(actor, out var leaving))
            return false;

        group.JoinOrder.Remove(actor);
        if (group.Members.Count == 0)
        {
            _groups.Remove(groupId);
            deleted = true;
            return true;
        }

        if (leaving.IsAdmin && group.AdminCount == 0)
        {
            var successor = group.JoinOrder[0];
            group.Members[successor] = group.Members[successor] with { IsAdmin = true };
        }

        return true;
    }

    public bool TryDeleteGroup(uint groupId, uint actor)
    {
        if (!TryGetGroup(groupId, out var group) || !group.IsAdmin(actor))
            return false;

        return _groups.Remove(groupId);
    }

    private void ResetGroups()
    {
        _groups.Clear();
        _nextGroupId = 1;
    }
}

public sealed class NanoChatServerGroup
{
    public uint Id { get; }
    public uint Creator { get; }
    public string Name { get; set; }
    public Dictionary<uint, NanoChatGroupMember> Members { get; }
    public List<uint> JoinOrder { get; }

    public int AdminCount => Members.Values.Count(member => member.IsAdmin);

    public NanoChatServerGroup(uint id, uint creator, string name, Dictionary<uint, NanoChatGroupMember> members)
    {
        Id = id;
        Creator = creator;
        Name = name;
        Members = members;
        JoinOrder = members.Keys.ToList();
    }

    public bool IsAdmin(uint number)
        => Members.TryGetValue(number, out var member) && member.IsAdmin;

    public NanoChatGroup Snapshot(bool hasUnread = false, bool notificationsMuted = false)
        => new(Id, Name, new Dictionary<uint, NanoChatGroupMember>(Members), hasUnread, notificationsMuted);
}
