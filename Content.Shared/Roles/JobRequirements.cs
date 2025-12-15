using Content.Shared._WL.CCVars;
using Content.Shared.Preferences;
using Content.Shared.Turrets;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Shared.Roles;

public static class JobRequirements
{
    /// <summary>
    /// Checks if the requirements of the job are met by the provided play-times.
    /// </summary>
    /// <param name="job"> The job to test. </param>
    /// <param name="playTimes"> The playtimes used for the check. </param>
    /// <param name="reason"> If the requirements were not met, details are provided here. </param>
    /// <returns>Returns true if all requirements were met or there were no requirements.</returns>
    public static bool TryRequirementsMet(
        JobPrototype job,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out /*WL-Changes-start*/FormattedMessage[]?/*WL-Changes-end*/ reasons,
        IEntityManager entManager,
        IPrototypeManager protoManager,
        /*WL-Changes-start*/IConfigurationManager cfgMan,/*WL-Changes-end*/
        HumanoidCharacterProfile? profile)
    {
        var sys = entManager.System<SharedRoleSystem>();
        var requirements = sys.GetRoleRequirements(job);
        return TryRequirementsMet(requirements, playTimes, out reasons, entManager, protoManager, cfgMan, profile, /*WL-Changes-start*/job/*WL-Changes-end*/);
    }

    /// <summary>
    /// Checks if the list of requirements are met by the provided play-times.
    /// </summary>
    /// <param name="requirements"> The requirements to test. </param>
    /// <param name="playTimes"> The playtimes used for the check. </param>
    /// <param name="reasons"> If the requirements were not met, details are provided here. </param>
    /// <returns>Returns true if all requirements were met or there were no requirements.</returns>
    public static bool TryRequirementsMet(
        HashSet<JobRequirement>? requirements,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out /*WL-Changes-start*/FormattedMessage[]?/*WL-Changes-end*/ reasons,
        IEntityManager entManager,
        IPrototypeManager protoManager,
        /*WL-Changes-start*/IConfigurationManager cfgMan,/*WL-Changes-end*/
        HumanoidCharacterProfile? profile,
        /*WL-Changes-start*/JobPrototype? job = null/*WL-Changes-end*/)
    {
        reasons = null;

        if (requirements == null)
            return true;

        // WL-Changes-start
        if (!IsRoleRestrictionChecksEnabled(cfgMan))
            return true;

        var innerReasons = new List<FormattedMessage>();
        var successful = true;

        foreach (var requirement in requirements)
        {
            if (!requirement.Check(
                    entManager,
                    protoManager,
                    cfgMan,
                    profile,
                    /*WL-Changes-start*/job/*WL-Changes-end*/,
                    playTimes,
                    out var reason))
            {
                successful = false;

                if (reason != null)
                    innerReasons.Add(reason);
            }
        }

        if (!successful)
        {
            reasons = [.. innerReasons];
            return false;
        }
        // WL-Changes-end

        return true;
    }

    // WL-Changes-start
    [return: NotNullIfNotNull(nameof(reasons))]
    public static FormattedMessage? JoinReasons(IEnumerable<FormattedMessage>? reasons)
    {
        if (reasons == null)
            return null;

        return FormattedMessage.FromMarkupOrThrow(string.Join("\n", reasons.Select(f => f.ToMarkup())));
    }

    public static bool IsRoleRestrictionChecksEnabled(IConfigurationManager cfgMan)
    {
        return cfgMan.GetCVar(WLCVars.RoleRestrictionChecksEnabled);
    }
    // WL-Changes-end
}

/// <summary>
/// Abstract class for playtime and other requirements for role gates.
/// </summary>
[ImplicitDataDefinitionForInheritors]
[Serializable, NetSerializable]
public abstract partial class JobRequirement
{
    [DataField]
    public bool Inverted;

    public abstract bool Check(
        IEntityManager entManager,
        IPrototypeManager protoManager,
        /*WL-Changes-start*/IConfigurationManager cfgMan,/*WL-Changes-end*/
        HumanoidCharacterProfile? profile,
        /*WL-Changes-start*/JobPrototype? job,/*WL-Changes-end*/
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason);
}
