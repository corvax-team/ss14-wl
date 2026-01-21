using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Robust.Server.ServerStatus;

namespace Content.Server.Administration;

public sealed partial class ServerApi
{
    //WL-Changes-start
    [GeneratedRegex("(\\{\\s*\\$\\s*([^}\\s]+)\\s*\\})")]
    private static partial Regex ParametrSearchRegex();
    //WL-Changes-end

    private void RegisterHandler(HttpMethod method, string exactPath, Func<IStatusHandlerContext, Task> handler)
    {
        _statusHost.AddHandler(async context =>
        {
            if (context.RequestMethod != method || context.Url.AbsolutePath != exactPath)
                return false;

            if (!await CheckAccess(context))
                return true;

            await handler(context);
            return true;
        });
    }

    private void RegisterActorHandler(HttpMethod method, string exactPath, Func<IStatusHandlerContext, Actor, Task> handler)
    {
        RegisterHandler(method, exactPath, async context =>
        {
            if (await CheckActor(context) is not { } actor)
                return;

            await handler(context, actor);
        });
    }

    //WL-Changes-start
    private void RegisterParameterizedActorHandler(
        HttpMethod method,
        string exactPath,
        Func<IStatusHandlerContext, Actor, Dictionary<string, string>, Task> handler)
    {
        RegisterParameterizedHandler(method, exactPath, async (context, maps) =>
        {
            if (await CheckActor(context) is not { } actor)
                return;

            await handler(context, actor, maps);
        });
    }

    private void RegisterParameterizedHandler(
        HttpMethod method,
        string exactPath,
        Func<IStatusHandlerContext, Dictionary<string, string>, Task> handler)
    {
        _statusHost.AddHandler(async context =>
        {
            var absolute_path = context.Url.AbsolutePath;

            if (context.RequestMethod != method || !CheckPathes(absolute_path, exactPath))
                return false;

            if (!await CheckAccess(context))
                return true;

            var formatted_maps = GetMapArguments(absolute_path, exactPath);
            if (formatted_maps.Count == 0)
                return true;

            await handler(context, formatted_maps);
            return true;
        });
    }

    private static bool CheckPathes(string realPath, string predictedPath)
    {
        var search_regex = ParametrSearchRegex();

        var is_match = search_regex.Matches(predictedPath)
            .ToList()
            .TrueForAll(match =>
            {
                if (!match.Success)
                    return false;

                var to_replace = match.Groups[1].Value;

                var inner_regex = new Regex(predictedPath.Replace(to_replace, "(.*)"));
                var inner_match = inner_regex.Match(realPath);

                return inner_match.Success;
            });

        return is_match;
    }

    private static Dictionary<string, string> GetMapArguments(string realPath, string predictedPath)
    {
        var search_regex = ParametrSearchRegex();

        var dict = new Dictionary<string, string>();

        var matches = search_regex.Matches(predictedPath);
        foreach (var match in matches.ToList())
        {
            if (!match.Success)
                continue;

            var to_replace = match.Groups[1].Value;
            var name = match.Groups[2].Value;

            var inner_regex = new Regex(predictedPath.Replace(to_replace, "(.*)"));
            var inner_match = inner_regex.Match(realPath).Groups[1].Value;

            dict.Add(name.Trim(), inner_match.Trim());
        }

        return dict;
    }
    //WL-Changes-end

    /// <summary>
    /// Async helper function which runs a task on the main thread and returns the result.
    /// </summary>
    private async Task<T> RunOnMainThread<T>(Func<T> func)
    {
        var taskCompletionSource = new TaskCompletionSource<T>();
        _taskManager.RunOnMainThread(() =>
        {
            try
            {
                taskCompletionSource.TrySetResult(func());
            }
            catch (Exception e)
            {
                taskCompletionSource.TrySetException(e);
            }
        });

        var result = await taskCompletionSource.Task;
        return result;
    }

    /// <summary>
    /// Runs an action on the main thread. This does not return any value and is meant to be used for void functions. Use <see cref="RunOnMainThread{T}"/> for functions that return a value.
    /// </summary>
    private async Task RunOnMainThread(Action action)
    {
        var taskCompletionSource = new TaskCompletionSource();
        _taskManager.RunOnMainThread(() =>
        {
            try
            {
                action();
                taskCompletionSource.TrySetResult();
            }
            catch (Exception e)
            {
                taskCompletionSource.TrySetException(e);
            }
        });

        await taskCompletionSource.Task;
    }

    private async Task RunOnMainThread(Func<Task> action)
    {
        var taskCompletionSource = new TaskCompletionSource();
        // ReSharper disable once AsyncVoidLambda
        _taskManager.RunOnMainThread(async () =>
        {
            try
            {
                await action();
                taskCompletionSource.TrySetResult();
            }
            catch (Exception e)
            {
                taskCompletionSource.TrySetException(e);
            }
        });

        await taskCompletionSource.Task;
    }

    /// <summary>
    /// Helper function to read JSON encoded data from the request body.
    /// </summary>
    private static async Task<T?> ReadJson<T>(IStatusHandlerContext context) where T : notnull
    {
        try
        {
            var json = await context.RequestBodyJsonAsync<T>();
            if (json == null)
                await RespondBadRequest(context, "Request body is null");

            return json;
        }
        catch (Exception e)
        {
            await RespondBadRequest(context, "Unable to parse request body", ExceptionData.FromException(e));
            return default;
        }
    }

    private static async Task RespondError(
        IStatusHandlerContext context,
        ErrorCode errorCode,
        HttpStatusCode statusCode,
        string message,
        ExceptionData? exception = null)
    {
        await context.RespondJsonAsync(new BaseResponse(message, errorCode, exception), statusCode)
            .ConfigureAwait(false);
    }

    private static async Task RespondBadRequest(
        IStatusHandlerContext context,
        string message,
        ExceptionData? exception = null)
    {
        await RespondError(context, ErrorCode.BadRequest, HttpStatusCode.BadRequest, message, exception)
            .ConfigureAwait(false);
    }

    private static async Task RespondOk(IStatusHandlerContext context)
    {
        await context.RespondJsonAsync(new BaseResponse("OK"))
            .ConfigureAwait(false);
    }

    private static string FormatLogActor(Actor actor)
    {
        var record = actor.Record;

        return $"{record.LastSeenUserName} ({record.UserId.UserId})";
    }
}
