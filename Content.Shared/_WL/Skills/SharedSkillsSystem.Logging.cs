using System.Linq;
using Content.Shared._WL.Skills.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Robust.Shared.Player;

namespace Content.Shared._WL.Skills;

public abstract partial class SharedSkillsSystem
{
    [Dependency] private ISharedAdminLogManager _skillLogs = default!;

    private readonly HashSet<EntityUid> _loggingSkills = new();
    private static readonly SkillType[] LoggedSkills = Enum.GetValues<SkillType>();

    /// <summary>
    /// Logs the completed operation. Grouped operations combine nested changes into a single entry.
    /// Ungrouped scopes can span a UI lifetime without suppressing unrelated changes.
    /// </summary>
    public IDisposable? BeginSkillChange(EntityUid uid, string reason, ICommonSession? actor = null,
        string? source = null, EntityUid? sourceEntity = null, bool force = false, bool group = true)
    {
        if (!_skillLogs.Enabled || group && !_loggingSkills.Add(uid))
            return null;

        return new SkillLogScope(this, uid, CaptureSkills(uid), reason, actor, source, sourceEntity, force, group);
    }

    private SkillSnapshot? CaptureSkills(EntityUid uid)
    {
        return TryComp<SkillsComponent>(uid, out var comp)
            ? new SkillSnapshot(LoggedSkills.Select(skill => comp.Skills.GetValueOrDefault(skill, 1)).ToArray(),
                comp.SpentPoints, comp.UnspentPoints, comp.BonusPoints, comp.CurrentJob?.Id)
            : null;
    }

    private void FinishSkillChange(SkillLogScope scope)
    {
        if (scope.Group)
            _loggingSkills.Remove(scope.Uid);
        if (CaptureSkills(scope.Uid) is not { } after)
            return;

        var changes = new List<string>();
        if (scope.Before is { } before)
        {
            for (var i = 0; i < LoggedSkills.Length; i++)
            {
                if (before.Levels[i] != after.Levels[i])
                    changes.Add($"{LoggedSkills[i]} {before.Levels[i]}->{after.Levels[i]}");
            }

            AddChange("spent", before.Spent, after.Spent);
            AddChange("unspent", before.Unspent, after.Unspent);
            AddChange("bonus", before.Bonus, after.Bonus);
            if (before.Job != after.Job)
                changes.Add($"job {before.Job ?? "none"}->{after.Job ?? "none"}");
        }

        if (scope.Before != null && changes.Count == 0 && !scope.Force)
            return;

        var difference = scope.Before == null ? "initial state"
            : changes.Count == 0 ? "unchanged" : string.Join(", ", changes);
        var table = string.Join(", ", LoggedSkills.Select((skill, i) => $"{skill}={after.Levels[i]}"));
        var actor = scope.Actor;
        var reason = scope.Reason;
        var source = scope.Source ?? "system";
        var job = after.Job ?? "none";
        var spent = after.Spent;
        var unspent = after.Unspent;
        var bonus = after.Bonus;
        WriteSkillLog(_skillLogs, scope.SourceEntity,
            $"Skills for {scope.Uid:target}: reason={reason:reason}; actor={actor}; source={source:source}; job={job:job}. Changes: {difference:difference}. Skills: {table:table}. Points: spent={spent}, unspent={unspent}, bonus={bonus}.");

        void AddChange(string name, int oldValue, int newValue)
        {
            if (oldValue != newValue)
                changes.Add($"{name} {oldValue}->{newValue}");
        }
    }

    private static void WriteSkillLog(ISharedAdminLogManager logger, EntityUid? sourceEntity,
        [System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("logger")] ref LogStringHandler message)
    {
        if (!logger.Enabled)
            return;

        if (sourceEntity is { } entity)
        {
            message.AppendLiteral(" sourceEntity=");
            message.AppendFormatted(entity, nameof(sourceEntity));
            message.AppendLiteral(".");
        }

        logger.Add(LogType.Skills, LogImpact.Low, ref message);
    }

    private sealed record SkillSnapshot(int[] Levels, int Spent, int Unspent, int Bonus, string? Job);

    private sealed class SkillLogScope(SharedSkillsSystem system, EntityUid uid, SkillSnapshot? before,
        string reason, ICommonSession? actor, string? source, EntityUid? sourceEntity, bool force, bool group) : IDisposable
    {
        public readonly EntityUid Uid = uid;
        public readonly SkillSnapshot? Before = before;
        public readonly string Reason = reason;
        public readonly ICommonSession? Actor = actor;
        public readonly string? Source = source;
        public readonly EntityUid? SourceEntity = sourceEntity;
        public readonly bool Force = force;
        public readonly bool Group = group;
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            system.FinishSkillChange(this);
        }
    }
}
