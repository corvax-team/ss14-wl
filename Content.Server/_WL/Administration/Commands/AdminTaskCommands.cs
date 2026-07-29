using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Mind;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server._WL.Administration.Commands;

[AdminCommand(AdminFlags.Fun)]
public sealed class AddAdminTaskCommand : IConsoleCommand
{
    public string Command => "addadmintask";
    public string Description => Loc.GetString("cmd-addadmintask-desc");
    public string Help => Loc.GetString("cmd-addadmintask-help");

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 3)
        {
            shell.WriteError(Help);
            return;
        }

        var ckey = args[0];
        var name = args[1];
        var description = args[2];

        var playerMgr = IoCManager.Resolve<IPlayerManager>();
        if (!playerMgr.TryGetSessionByUsername(ckey, out var session))
        {
            shell.WriteError(Loc.GetString("admin-objective-error-player-not-found", ("ckey", ckey)));
            return;
        }

        var entManager = IoCManager.Resolve<IEntityManager>();
        var mindSystem = entManager.System<SharedMindSystem>();
        var manager = entManager.System<AdminObjectiveManagerSystem>();

        if (!mindSystem.TryGetMind(session, out var mindId, out var mind))
        {
            shell.WriteError(Loc.GetString("admin-objective-error-no-mind", ("ckey", ckey)));
            return;
        }

        if (manager.TryAddTask(mindId, mind, name, description, shell.Player?.Name))
            shell.WriteLine(Loc.GetString("admin-objective-success-added", ("name", name), ("ckey", ckey)));
        else
            shell.WriteError(Loc.GetString("admin-objective-error-add-failed"));
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
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
public sealed class RmAdminTaskCommand : IConsoleCommand
{
    public string Command => "rmadmintask";
    public string Description => Loc.GetString("cmd-rmadmintask-desc");
    public string Help => Loc.GetString("cmd-rmadmintask-help");

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2 || !int.TryParse(args[1], out var taskId))
        {
            shell.WriteError(Help);
            return;
        }

        var ckey = args[0];
        var playerMgr = IoCManager.Resolve<IPlayerManager>();
        if (!playerMgr.TryGetSessionByUsername(ckey, out var session))
        {
            shell.WriteError(Loc.GetString("admin-objective-error-player-not-found", ("ckey", ckey)));
            return;
        }

        var entManager = IoCManager.Resolve<IEntityManager>();
        var mindSystem = entManager.System<SharedMindSystem>();
        var manager = entManager.System<AdminObjectiveManagerSystem>();

        if (!mindSystem.TryGetMind(session, out _, out var mind))
        {
            shell.WriteError(Loc.GetString("admin-objective-error-no-mind", ("ckey", ckey)));
            return;
        }

        if (manager.TryRemoveTask(mind, taskId, shell.Player?.Name, out var name))
            shell.WriteLine(Loc.GetString("admin-objective-success-removed", ("name", name ?? ""), ("id", taskId), ("ckey", ckey)));
        else
            shell.WriteError(Loc.GetString("admin-objective-error-task-not-found", ("id", taskId), ("ckey", ckey)));
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "<ckey>"),
            2 => CompletionResult.FromHintOptions(AdminObjectiveCompletionHelper.GetTaskOptions(args[0]), "<id>"),
            _ => CompletionResult.Empty,
        };
    }
}

[AdminCommand(AdminFlags.Fun)]
public sealed class EditAdminTaskCommand : IConsoleCommand
{
    public string Command => "editadmintask";
    public string Description => Loc.GetString("cmd-editadmintask-desc");
    public string Help => Loc.GetString("cmd-editadmintask-help");

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 4 || !int.TryParse(args[1], out var taskId))
        {
            shell.WriteError(Help);
            return;
        }

        var ckey = args[0];
        var field = args[2];
        var value = string.Join(' ', args[3..]);

        var playerMgr = IoCManager.Resolve<IPlayerManager>();
        if (!playerMgr.TryGetSessionByUsername(ckey, out var session))
        {
            shell.WriteError(Loc.GetString("admin-objective-error-player-not-found", ("ckey", ckey)));
            return;
        }

        var entManager = IoCManager.Resolve<IEntityManager>();
        var mindSystem = entManager.System<SharedMindSystem>();
        var manager = entManager.System<AdminObjectiveManagerSystem>();

        if (!mindSystem.TryGetMind(session, out _, out var mind))
        {
            shell.WriteError(Loc.GetString("admin-objective-error-no-mind", ("ckey", ckey)));
            return;
        }

        if (manager.TryEditTask(mind, taskId, field, value, shell.Player?.Name, out var errorKey))
            shell.WriteLine(Loc.GetString("admin-objective-success-edited", ("field", field), ("id", taskId), ("ckey", ckey)));
        else
            shell.WriteError(errorKey is null ? "" : Loc.GetString(errorKey));
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "<ckey>"),
            2 => CompletionResult.FromHintOptions(AdminObjectiveCompletionHelper.GetTaskOptions(args[0]), "<id>"),
            3 => CompletionResult.FromHintOptions(new[]
            {
                new CompletionOption("name"),
                new CompletionOption("description"),
                new CompletionOption("progress"),
            },
            "<field>"),
            4 => CompletionResult.FromHint("<value>"),
            _ => CompletionResult.Empty,
        };
    }
}

internal static class AdminObjectiveCompletionHelper
{
    public static IEnumerable<CompletionOption> GetTaskOptions(string ckey)
    {
        if (!IoCManager.Resolve<IPlayerManager>().TryGetSessionByUsername(ckey, out var session))
            yield break;

        var entManager = IoCManager.Resolve<IEntityManager>();
        var mindSystem = entManager.System<SharedMindSystem>();
        var manager = entManager.System<AdminObjectiveManagerSystem>();

        if (!mindSystem.TryGetMind(session, out _, out var mind))
            yield break;

        foreach (var (taskId, name) in manager.GetTasks(mind))
        {
            yield return new CompletionOption(taskId.ToString(), name);
        }
    }
}
