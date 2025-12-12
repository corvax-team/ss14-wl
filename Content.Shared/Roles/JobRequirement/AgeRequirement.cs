using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._WL.CCVars;
using Content.Shared.Preferences;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Roles;

/// <summary>
/// Requires the character to be older or younger than a certain age (inclusive)
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class AgeRequirement : JobRequirement
{
    //WL-Changes-start
    [DataField]
    public int? MinAge;

    [DataField]
    public int? MaxAge;
    //WL-Changes-end

    public override bool Check(
        IEntityManager entManager,
        IPrototypeManager protoManager,
        /*WL-Changes-start*/IConfigurationManager cfgMan,/*WL-Changes-end*/
        HumanoidCharacterProfile? profile,
        /*WL-Changes-start*/JobPrototype? job,/*WL-Changes-end*/
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = new FormattedMessage();

        if (profile is null) //the profile could be null if the player is a ghost. In this case we don't need to block the role selection for ghostrole
            return true;

        //WL-Changes-start
        if (job is null)
            return true;

        if (cfgMan.GetCVar(WLCVars.IsAgeCheckNeeded) == false)
            return true;

        var isNeeded = true;
        if (profile.JobUnblockings.ContainsKey(job.ID))
            isNeeded = false;

        if (isNeeded)
        {
            if (MinAge != null && profile.Age < MinAge)
            {
                reason = FormattedMessage.FromMarkupPermissive(Loc.GetString("role-timer-age-too-young",
                    ("age", MinAge)));
                return false;
            }
            if (MaxAge != null && profile.Age > MaxAge)
            {
                reason = FormattedMessage.FromMarkupPermissive(Loc.GetString("role-timer-age-too-old",
                    ("age", MaxAge)));
                return false;
            }
        }
        //WL-Changes-end

        return true;
    }
}
