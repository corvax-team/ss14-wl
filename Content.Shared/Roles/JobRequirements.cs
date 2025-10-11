using System.Diagnostics.CodeAnalysis;
using Content.Shared.Preferences;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

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
        [NotNullWhen(false)] out FormattedMessage? reason,
        IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile)
    {
        var sys = entManager.System<SharedRoleSystem>();
        var requirements = sys.GetRoleRequirements(job);
        return TryRequirementsMet(requirements, playTimes, out reason, entManager, protoManager, profile, /*WL-Changes-start*/job/*WL-Changes-end*/);
    }

    /// <summary>
    /// Checks if the list of requirements are met by the provided play-times.
    /// </summary>
    /// <param name="requirements"> The requirements to test. </param>
    /// <param name="playTimes"> The playtimes used for the check. </param>
    /// <param name="reason"> If the requirements were not met, details are provided here. </param>
    /// <returns>Returns true if all requirements were met or there were no requirements.</returns>
    public static bool TryRequirementsMet(
        HashSet<JobRequirement>? requirements,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason,
        IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        /*WL-Changes-start*/JobPrototype? job = null/*WL-Changes-end*/)
    {
        reason = null;
        if (requirements == null)
            return true;

        foreach (var requirement in requirements)
        {
            if (!requirement.Check(entManager, protoManager, profile, /*WL-Changes-start*/job/*WL-Changes-end*/, playTimes, out reason))
                return false;
        }

        return true;
    }
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

    public virtual IReadOnlyList<CVarValueWrapper>? CheckingCVars => null;

    public abstract bool Check(
        IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        /*WL-Changes-start*/JobPrototype? job,/*WL-Changes-end*/
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason);

    //WL-Changes-start
    public readonly record struct CVarValueWrapper(CVarDef<bool> CVar, bool Value)
    {
        public static implicit operator CVarValueWrapper((CVarDef<bool> CVar, bool Value) tuple)
        {
            return new CVarValueWrapper(tuple.CVar, tuple.Value);
        }
    }
    //WL-Changes-end
}
