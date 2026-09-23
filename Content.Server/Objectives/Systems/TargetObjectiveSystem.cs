using Content.Server.Objectives.Components;
using Content.Server.Roles;
using Content.Server.Roles.Jobs;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Shared.GameObjects;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.Objectives.Systems;

/// <summary>
/// Provides API for other components and handles setting the title.
/// </summary>
public sealed partial class TargetObjectiveSystem : EntitySystem
{
    [Dependency] private MetaDataSystem _metaData = default!;
    [Dependency] private SharedJobSystem _job = default!;
    [Dependency] private RoleSystem _role = default!; // WL-Changes

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TargetObjectiveComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
    }

    private void OnAfterAssign(EntityUid uid, TargetObjectiveComponent comp, ref ObjectiveAfterAssignEvent args)
    {
        if (!GetTarget(uid, out var target, comp))
            return;

        _metaData.SetEntityName(uid, GetTitle(target.Value, comp.Title), args.Meta);
    }

    /// <summary>
    /// Sets the Target field for the title and other components to use.
    /// </summary>
    public void SetTarget(EntityUid uid, EntityUid target, TargetObjectiveComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.Target = target;
    }

    /// <summary>
    /// Gets the target from the component.
    /// </summary>
    /// <remarks>
    /// If it is null then the prototype is invalid, just return.
    /// </remarks>
    public bool GetTarget(EntityUid uid, [NotNullWhen(true)] out EntityUid? target, TargetObjectiveComponent? comp = null)
    {
        target = Resolve(uid, ref comp) ? comp.Target : null;
        return target != null;
    }

    private string GetTitle(EntityUid target, string title)
    {
        var targetName = "Unknown";
        if (TryComp<MindComponent>(target, out var mind) && mind.CharacterName != null)
        {
            targetName = mind.CharacterName;
        }

        var jobName = "Unknown";// WL-Changes: Subnames

        var deptName = Loc.GetString("department-Unknown");
        if (_job.MindTryGetJobId(target, out var jobId))
        {
            if (jobId.HasValue && _job.TryGetDepartment(jobId.Value, out var deptProto))
            {
                deptName = Loc.GetString(deptProto.Name);
            }

            // WL-Changes: Subnames start
            if (jobId is not null &&
                    ProtoMan.TryIndex<JobPrototype>(jobId, out var jobProto))
            {
                jobName = jobProto.LocalizedName;

                if (mind != null)
                    jobName = _role.GetSubnameByMind(mind, jobId) ?? jobName;
                else
                    jobName = _role.GetSubnameByEntity(target, jobId) ?? jobName;
            }
            // WL-Changes: Subnames end
        }
        return Loc.GetString(title, ("targetName", targetName), ("job", jobName), ("department", deptName));
    }
}
