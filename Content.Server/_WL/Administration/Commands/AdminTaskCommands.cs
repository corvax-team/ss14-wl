using Content.Server.Administration;
using Content.Server.Mind;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server._WL.Administration.Commands;

[AdminCommand(AdminFlags.Fun)]
public sealed partial class AddAdminTaskCommand : LocalizedEntityCommands
{
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private MindSystem _mind = default!;
    [Dependency] private AdminObjectiveManagerSystem _manager = default!;
    public override string Command => "addadmintask";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 3)
        {
            shell.WriteError(Help);
            return;
        }

        var ckey = args[0];
        var name = args[1];
        var description = args[2];

        if (!_player.TryGetSessionByUsername(ckey, out var session))
        {
            shell.WriteError(Loc.GetString("admin-objective-error-player-not-found", ("ckey", ckey)));
            return;
        }

        if (!_mind.TryGetMind(session, out var mindId, out var mind))
        {
            shell.WriteError(Loc.GetString("admin-objective-error-no-mind", ("ckey", ckey)));
            return;
        }

        if (_manager.TryAddTask(mindId, mind, name, description, shell.Player?.Name))
            shell.WriteLine(Loc.GetString("admin-objective-success-added", ("name", name), ("ckey", ckey)));
        else
            shell.WriteError(Loc.GetString("admin-objective-error-add-failed"));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "<ckey>"),
            2 => CompletionResult.FromHint("<name>"),
            3 => CompletionResult.FromHint("<description>"),
            _ => CompletionResult.Empty,
        };
    }
}

[AdminCommand(AdminFlags.Fun)]
public sealed partial class RmAdminTaskCommand : LocalizedEntityCommands
{
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private MindSystem _mind = default!;
    [Dependency] private AdminObjectiveManagerSystem _manager = default!;
    public override string Command => "rmadmintask";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2 || !int.TryParse(args[1], out var taskId))
        {
            shell.WriteError(Help);
            return;
        }

        var ckey = args[0];
        if (!_player.TryGetSessionByUsername(ckey, out var session))
        {
            shell.WriteError(Loc.GetString("admin-objective-error-player-not-found", ("ckey", ckey)));
            return;
        }

        if (!_mind.TryGetMind(session, out _, out var mind))
        {
            shell.WriteError(Loc.GetString("admin-objective-error-no-mind", ("ckey", ckey)));
            return;
        }

        if (_manager.TryRemoveTask(mind, taskId, shell.Player?.Name, out var name))
            shell.WriteLine(Loc.GetString("admin-objective-success-removed", ("name", name ?? ""), ("id", taskId), ("ckey", ckey)));
        else
            shell.WriteError(Loc.GetString("admin-objective-error-task-not-found", ("id", taskId), ("ckey", ckey)));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "<ckey>"),
            2 => CompletionResult.FromHintOptions(AdminObjectiveCompletionHelper.GetTaskOptions(args[0], _player, _mind, _manager), "<id>"),
            _ => CompletionResult.Empty,
        };
    }
}

[AdminCommand(AdminFlags.Fun)]
public sealed partial class EditAdminTaskCommand : LocalizedEntityCommands
{
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private MindSystem _mind = default!;
    [Dependency] private AdminObjectiveManagerSystem _manager = default!;
    public override string Command => "editadmintask";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 4 || !int.TryParse(args[1], out var taskId))
        {
            shell.WriteError(Help);
            return;
        }

        var ckey = args[0];
        var field = args[2];
        var value = string.Join(' ', args[3..]);

        if (!_player.TryGetSessionByUsername(ckey, out var session))
        {
            shell.WriteError(Loc.GetString("admin-objective-error-player-not-found", ("ckey", ckey)));
            return;
        }

        if (!_mind.TryGetMind(session, out _, out var mind))
        {
            shell.WriteError(Loc.GetString("admin-objective-error-no-mind", ("ckey", ckey)));
            return;
        }

        if (_manager.TryEditTask(mind, taskId, field, value, shell.Player?.Name, out var errorKey))
            shell.WriteLine(Loc.GetString("admin-objective-success-edited", ("field", field), ("id", taskId), ("ckey", ckey)));
        else
            shell.WriteError(errorKey is null ? "" : Loc.GetString(errorKey));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "<ckey>"),
            2 => CompletionResult.FromHintOptions(AdminObjectiveCompletionHelper.GetTaskOptions(args[0], _player, _mind, _manager), "<id>"),
            3 => CompletionResult.FromHintOptions(
            [
                new CompletionOption("name"),
                new CompletionOption("description"),
                new CompletionOption("progress"),
            ],
            "<field>"),
            4 => CompletionResult.FromHint("<value>"),
            _ => CompletionResult.Empty,
        };
    }
}

internal static class AdminObjectiveCompletionHelper
{
    public static IEnumerable<CompletionOption> GetTaskOptions(string ckey, IPlayerManager player, MindSystem mind, AdminObjectiveManagerSystem manager)
    {
        if (!player.TryGetSessionByUsername(ckey, out var session))
            yield break;

        if (!mind.TryGetMind(session, out _, out var mindComp))
            yield break;

        foreach (var (taskId, name) in manager.GetTasks(mindComp))
        {
            yield return new CompletionOption(taskId.ToString(), name);
        }
    }
}
