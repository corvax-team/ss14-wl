using Content.Shared.CCVar;
using Content.Shared.Localizations;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Preferences;
using Content.Shared.Roles.Jobs;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;

namespace Content.Shared.Roles;

[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class RoleTimeRequirement : JobRequirement
{
    /// <summary>
    /// What particular role they need the time requirement with.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<PlayTimeTrackerPrototype> Role;

    /// <inheritdoc cref="DepartmentTimeRequirement.Time"/>
    [DataField(required: true)]
    public TimeSpan Time;

    public override bool Check(
        IEntityManager entManager,
        IPrototypeManager protoManager,
        /*WL-Changes-start*/IConfigurationManager cfgMan,/*WL-Changes-end*/
        HumanoidCharacterProfile? profile,
        /*WL-Changes-start*/JobPrototype? job,/*WL-Changes-end*/
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = null; // no loc

        // WL-Changes-start
        if (cfgMan.GetCVar(CCVars.GameRoleTimers) == false)
            return true;
        // WL-Changes-end

        string proto = Role;

        playTimes.TryGetValue(proto, out var roleTime);
        var roleDiffSpan = Time - roleTime;
        var roleDiff = roleDiffSpan.TotalMinutes;
        var formattedRoleDiff = ContentLocalizationManager.FormatPlaytime(roleDiffSpan);
        var departmentColor = Color.Yellow;

        if (!entManager.EntitySysManager.TryGetEntitySystem(out SharedJobSystem? jobSystem))
        {
            reason = FormattedMessage.FromUnformatted("Internal error"); // WL-Changes
            return false;
        }

        var jobProto = jobSystem.GetJobPrototype(proto);

        if (jobSystem.TryGetDepartment(jobProto, out var departmentProto))
            departmentColor = departmentProto.Color;

        if (!protoManager.TryIndex<JobPrototype>(jobProto, out var indexedJob))
        {
            reason = FormattedMessage.FromUnformatted("Internal error"); // WL-Changes
            return false;
        }

        if (!Inverted)
        {
            if (roleDiff <= 0)
                return true;

            reason = FormattedMessage.FromMarkupPermissive(Loc.GetString(
                "role-timer-role-insufficient",
                ("time", formattedRoleDiff),
                ("job", indexedJob.LocalizedName),
                ("departmentColor", departmentColor.ToHex())));
            return false;
        }

        if (roleDiff <= 0)
        {
            reason = FormattedMessage.FromMarkupPermissive(Loc.GetString(
                "role-timer-role-too-high",
                ("time", formattedRoleDiff),
                ("job", indexedJob.LocalizedName),
                ("departmentColor", departmentColor.ToHex())));
            return false;
        }

        reason = null; // WL-Changes
        return true;
    }
}
