using System.Linq;
using Content.Server.Administration.Logs;
using Content.Shared._WL.Administration;
using Content.Shared.Database;
using Content.Shared.Mind;
using Content.Shared.Objectives.Systems;

namespace Content.Server._WL.Administration.Commands;

public sealed partial class AdminObjectiveManagerSystem : EntitySystem
{
    [Dependency] private SharedObjectivesSystem _objectives = default!;
    [Dependency] private MetaDataSystem _metaData = default!;
    [Dependency] private IAdminLogManager _adminLogs = default!;

    private const string AdminObjectiveProto = "AdminObjective";
    private readonly Dictionary<EntityUid, int> _nextTaskId = new();

    public bool TryAddTask(
        EntityUid mindId,
        MindComponent mind,
        string name,
        string description,
        string? setBy)
    {
        if (!_objectives.TryCreateObjective((mindId, mind), AdminObjectiveProto, out var created) || created is not { } uid)
            return false;

        _metaData.SetEntityName(uid, name);
        _metaData.SetEntityDescription(uid, description);

        var adminComp = EnsureComp<AdminObjectiveComponent>(uid);
        adminComp.TaskId = _nextTaskId[mindId] = _nextTaskId.GetValueOrDefault(mindId) + 1;

        mind.Objectives.Add(uid); // Yeah, we spawn shit in nullspace.

        _adminLogs.Add(LogType.AdminMessage,
            LogImpact.Extreme,
            $"{setBy} added admin objective '{name}' (#{adminComp.TaskId}) to {ToPrettyString(mind.OwnedEntity)}");

        return true;
    }

    public bool TryRemoveTask(MindComponent mind, int taskId, string? deletedBy, out string? name)
    {
        name = null;
        var found = mind.Objectives.FirstOrDefault(uid => TryComp<AdminObjectiveComponent>(uid, out var comp) && comp.TaskId == taskId);

        if (found == default || !TryComp<AdminObjectiveComponent>(found, out var adminComp))
            return false;

        name = MetaData(found).EntityName;

        _adminLogs.Add(LogType.AdminMessage,
            LogImpact.Extreme,
            $"{deletedBy} removed admin objective '{name}' (#{adminComp.TaskId}) from {ToPrettyString(mind.OwnedEntity)}");

        mind.Objectives.Remove(found);
        Del(found);

        return true;
    }

    public bool TryEditTask(MindComponent mind, int taskId, string field, string value, string? changedBy, out string? error)
    {
        error = null;
        var found = mind.Objectives.FirstOrDefault(uid => TryComp<AdminObjectiveComponent>(uid, out var comp) && comp.TaskId == taskId);

        if (found == default)
        {
            error = Loc.GetString("admin-objective-error-task-not-found", ("id", taskId), ("ckey", found.Id.ToString()));
            return false;
        }

        var adminComp = Comp<AdminObjectiveComponent>(found);
        var name = MetaData(found).EntityName; // Needed to save name

        switch (field.ToLowerInvariant())
        {
            case "name":
                _metaData.SetEntityName(found, value);
                break;
            case "description":
                _metaData.SetEntityDescription(found, value);
                break;
            case "progress":
            {
                if (!float.TryParse(value, out var progress) || progress < 0f || progress > 1f)
                {
                    error = Loc.GetString("admin-objective-error-invalid-progress");
                    return false;
                }

                adminComp.Progress = progress;
                break;
            }
            default:
                error = Loc.GetString("admin-objective-error-invalid-field");
                return false;
        }

        _adminLogs.Add(LogType.AdminMessage,
            LogImpact.Extreme,
            $"{changedBy} has changed {field} field of admin objective '{name}' (#{adminComp.TaskId}), assigned to {ToPrettyString(mind.OwnedEntity)} to {value}");

        return true;
    }

    public IEnumerable<(int TaskId, string Name)> GetTasks(MindComponent mind)
    {
        foreach (var obj in mind.Objectives)
        {
            if (TryComp<AdminObjectiveComponent>(obj, out var comp))
                yield return (comp.TaskId, MetaData(obj).EntityName);
        }
    }
}
